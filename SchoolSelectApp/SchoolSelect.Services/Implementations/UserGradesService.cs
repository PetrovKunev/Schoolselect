using Microsoft.Extensions.Logging;
using SchoolSelect.Common;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Implementations
{
    public class UserGradesService : IUserGradesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserGradesService> _logger;

        public UserGradesService(IUnitOfWork unitOfWork, ILogger<UserGradesService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<UserGradesViewModel>> GetUserGradesAsync(Guid userId)
        {
            var userGrades = await _unitOfWork.UserGrades.GetGradesByUserIdAsync(userId);

            return userGrades.Select(g => new UserGradesViewModel
            {
                Id = g.Id,
                ConfigurationName = g.ConfigurationName,
                BulgarianGrade = g.BulgarianGrade ?? 0,
                MathGrade = g.MathGrade ?? 0,
                BulgarianExamPoints = g.BulgarianExamPoints,
                MathExamPoints = g.MathExamPoints,
                CreatedAt = g.CreatedAt,
                AdditionalGrades = g.AdditionalGrades.Select(ag => new UserAdditionalGradeViewModel
                {
                    Id = ag.Id,
                    SubjectCode = ag.SubjectCode,
                    SubjectName = ag.SubjectName,
                    ComponentType = ag.ComponentType,
                    Value = ag.Value
                }).ToList()
            }).ToList();
        }

        public async Task<UserGradesViewModel?> GetUserGradeByIdAsync(int gradesId)
        {
            var grades = await _unitOfWork.UserGrades.GetGradesWithAdditionalGradesAsync(gradesId);

            if (grades == null)
            {
                return null;
            }

            return new UserGradesViewModel
            {
                Id = grades.Id,
                ConfigurationName = grades.ConfigurationName,
                BulgarianGrade = grades.BulgarianGrade ?? 0,
                MathGrade = grades.MathGrade ?? 0,
                BulgarianExamPoints = grades.BulgarianExamPoints,
                MathExamPoints = grades.MathExamPoints,
                CreatedAt = grades.CreatedAt,
                AdditionalGrades = grades.AdditionalGrades.Select(ag => new UserAdditionalGradeViewModel
                {
                    Id = ag.Id,
                    SubjectCode = ag.SubjectCode,
                    SubjectName = ag.SubjectName,
                    ComponentType = ag.ComponentType,
                    Value = ag.Value
                }).ToList()
            };
        }

        public async Task<int> CreateUserGradesAsync(UserGradesInputModel model, Guid userId)
        {
            var userGrades = new UserGrades
            {
                UserId = userId,
                ConfigurationName = model.ConfigurationName,
                BulgarianGrade = model.BulgarianGrade,
                MathGrade = model.MathGrade,
                BulgarianExamPoints = model.BulgarianExamPoints,
                MathExamPoints = model.MathExamPoints,
                CreatedAt = DateTime.UtcNow
            };

            // Добавяне на допълнителни оценки, ако има такива
            if (model.AdditionalGrades != null && model.AdditionalGrades.Any())
            {
                foreach (var additionalGrade in model.AdditionalGrades)
                {
                    userGrades.AdditionalGrades.Add(new UserAdditionalGrade
                    {
                        SubjectCode = additionalGrade.SubjectCode,
                        SubjectName = additionalGrade.SubjectName,
                        ComponentType = additionalGrade.ComponentType,
                        Value = additionalGrade.Value
                    });
                }
            }

            await _unitOfWork.UserGrades.AddAsync(userGrades);
            await _unitOfWork.CompleteAsync();

            return userGrades.Id;
        }

        public async Task UpdateUserGradesAsync(UserGradesInputModel model, int gradesId)
        {
            var userGrades = await _unitOfWork.UserGrades.GetGradesWithAdditionalGradesAsync(gradesId);

            if (userGrades == null)
            {
                throw new InvalidOperationException($"Не е намерен набор от оценки с ID {gradesId}");
            }

            userGrades.ConfigurationName = model.ConfigurationName;
            userGrades.BulgarianGrade = model.BulgarianGrade;
            userGrades.MathGrade = model.MathGrade;
            userGrades.BulgarianExamPoints = model.BulgarianExamPoints;
            userGrades.MathExamPoints = model.MathExamPoints;

            // Изтриване на всички стари допълнителни оценки
            userGrades.AdditionalGrades.Clear();

            // Добавяне на нови допълнителни оценки
            if (model.AdditionalGrades != null && model.AdditionalGrades.Any())
            {
                foreach (var additionalGrade in model.AdditionalGrades)
                {
                    userGrades.AdditionalGrades.Add(new UserAdditionalGrade
                    {
                        SubjectCode = additionalGrade.SubjectCode,
                        SubjectName = additionalGrade.SubjectName,
                        ComponentType = additionalGrade.ComponentType,
                        Value = additionalGrade.Value
                    });
                }
            }

            _unitOfWork.UserGrades.Update(userGrades);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteUserGradesAsync(int gradesId)
        {
            var userGrades = await _unitOfWork.UserGrades.GetByIdAsync(gradesId);

            if (userGrades == null)
            {
                throw new InvalidOperationException($"Не е намерен набор от оценки с ID {gradesId}");
            }

            _unitOfWork.UserGrades.Remove(userGrades);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<SchoolChanceViewModel> CalculateChanceAsync(int gradesId, int schoolId)
        {
            var userGrades = await _unitOfWork.UserGrades.GetGradesWithAdditionalGradesAsync(gradesId);
            var school = await _unitOfWork.Schools.GetSchoolWithDetailsAsync(schoolId);

            if (userGrades == null || school == null)
            {
                throw new InvalidOperationException("Невалидни данни за изчисление");
            }

            var profiles = await _unitOfWork.SchoolProfiles.GetProfilesBySchoolIdAsync(schoolId);
            var result = new SchoolChanceViewModel
            {
                School = new SchoolViewModel
                {
                    Id = school.Id,
                    Name = school.Name,
                    District = school.District,
                    City = school.City,
                    AverageRating = school.AverageRating,
                    ReviewsCount = school.RatingsCount
                },
                UserGrades = new UserGradesViewModel
                {
                    Id = userGrades.Id,
                    ConfigurationName = userGrades.ConfigurationName,
                    BulgarianGrade = userGrades.BulgarianGrade ?? 0,
                    MathGrade = userGrades.MathGrade ?? 0,
                    BulgarianExamPoints = userGrades.BulgarianExamPoints,
                    MathExamPoints = userGrades.MathExamPoints
                },
                ProfileChances = new List<ProfileChanceViewModel>()
            };

            // Изчисляване на шанс за всеки профил
            foreach (var profile in profiles)
            {
                // Получаване на последната формула за балообразуване за профила
                var admissionFormula = await _unitOfWork.AdmissionFormulas.GetCurrentFormulaForProfileAsync(profile.Id);

                // Получаване на историческите данни за минимален бал
                var historicalRankings = await _unitOfWork.HistoricalRankings.GetRankingsByProfileIdAsync(profile.Id);
                var latestRanking = historicalRankings.OrderByDescending(r => r.Year).FirstOrDefault();

                double calculatedScore = 0;
                double minimumScoreLastYear = latestRanking?.MinimumScore ?? 0;
                double chancePercentage = 50; // Стойност по подразбиране, ако нямаме данни

                if (admissionFormula != null)
                {
                    // Изчисляване на бал според формулата
                    calculatedScore = CalculateScore(userGrades, admissionFormula);

                    // Изчисляване на шанс за прием
                    chancePercentage = CalculateChancePercentage(calculatedScore, minimumScoreLastYear);
                }
                else
                {
                    // Ако нямаме формула, използваме опростен начин за изчисляване на бала
                    // Например: БЕЛ (удвоен) + Математика (удвоен) + точки от НВО
                    calculatedScore = ((userGrades.BulgarianGrade ?? 0) * 2) +
                                     ((userGrades.MathGrade ?? 0) * 2) +
                                     userGrades.BulgarianExamPoints +
                                     userGrades.MathExamPoints;

                    // Ако имаме исторически данни, изчисляваме шанса
                    if (minimumScoreLastYear > 0)
                    {
                        chancePercentage = CalculateChancePercentage(calculatedScore, minimumScoreLastYear);
                    }
                }

                result.ProfileChances.Add(new ProfileChanceViewModel
                {
                    ProfileId = profile.Id,
                    ProfileName = profile.Name,
                    CalculatedScore = calculatedScore,
                    MinimumScoreLastYear = minimumScoreLastYear,
                    ChancePercentage = chancePercentage,
                    AvailablePlaces = profile.AvailablePlaces
                });
            }

            return result;
        }

        // Изчисляване на бал според формулата за прием
        private double CalculateScore(UserGrades userGrades, AdmissionFormula formula)
        {
            _logger.LogDebug($"Изчисляване на бал за формула: {formula.FormulaExpression}");

            double score = 0;

            // 1. Опитваме с парсване и изчисляване на текстовата формула
            if (!string.IsNullOrEmpty(formula.FormulaExpression))
            {
                try
                {
                    double parsedScore = ParseAndCalculateFormula(userGrades, formula.FormulaExpression);
                    _logger.LogDebug($"Бал от парсване на формула: {parsedScore}");
                    return Math.Round(parsedScore, 2);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Неуспешно парсване на формула: {ex.Message}, опитваме с компоненти");
                }
            }

            // 2. Ако парсването не успее или няма формула, опитваме с компонентите
            if (formula.Components != null && formula.Components.Any())
            {
                score = CalculateScoreFromComponents(userGrades, formula.Components);
                _logger.LogDebug($"Бал изчислен от компоненти: {score}");
                return Math.Round(score, 2);
            }

            // 3. Ако нито един от методите не успее, използваме резервна формула
            score = ((userGrades.BulgarianGrade ?? 0) * 2) +
                    ((userGrades.MathGrade ?? 0) * 2) +
                    userGrades.BulgarianExamPoints +
                    userGrades.MathExamPoints;

            _logger.LogDebug($"Бал от стандартна формула: {score}");
            return Math.Round(score, 2);
        }

        // Метод за парсване и изчисляване на формулата
        private double ParseAndCalculateFormula(UserGrades userGrades, string formulaExpression)
        {
            _logger.LogDebug($"Парсване на формула: {formulaExpression}");

            // Подготвяме копие на формулата, което ще модифицираме
            string workingFormula = formulaExpression;

            // Създаваме речник с всички възможни термове в формулата и техните стойности
            var valueMap = PrepareValueDictionary(userGrades);

            // Извършваме заместване на всички променливи с техните стойности
            foreach (var entry in valueMap)
            {
                // Спазваме формата на заместване, който виждаме в примерите
                // Например: "2 * БЕЛ" => "2 * 5.50"
                string searchPattern1 = $" * {entry.Key}";
                string replaceWith1 = $" * {entry.Value}";

                // Също търсим заместване без интервал: "2*БЕЛ" => "2*5.50"
                string searchPattern2 = $"*{entry.Key}";
                string replaceWith2 = $"*{entry.Value}";

                // Заместваме всички срещания
                workingFormula = workingFormula.Replace(searchPattern1, replaceWith1)
                                               .Replace(searchPattern2, replaceWith2);
            }

            _logger.LogDebug($"Формула след заместване на променливи: {workingFormula}");

            // Опитваме да изчислим резултата на модифицираната формула
            return EvaluateExpression(workingFormula);
        }

        // Метод за подготовка на речник с всички възможни стойности
        private Dictionary<string, double> PrepareValueDictionary(UserGrades userGrades)
        {
            var valueMap = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            // Добавяме основните оценки (като оценки и като точки)
            valueMap["БЕЛ"] = userGrades.BulgarianGrade ?? 0;  // Годишна оценка по БЕЛ
            valueMap["МАТ"] = userGrades.MathGrade ?? 0;       // Годишна оценка по Математика

            // Добавяме конвертираните в точки оценки
            valueMap["БЕЛ_ТОЧКИ"] = ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0);
            valueMap["МАТ_ТОЧКИ"] = ConvertGradeToPoints(userGrades.MathGrade ?? 0);

            // Добавяме точките от НВО
            valueMap["БЕЛ1"] = userGrades.BulgarianExamPoints; // НВО по БЕЛ
            valueMap["М"] = userGrades.MathExamPoints;         // НВО по Математика (формат от примера)
            valueMap["МАТ1"] = userGrades.MathExamPoints;      // Алтернативен формат за НВО по Математика

            // Проверяваме дали имаме допълнителни оценки
            if (userGrades.AdditionalGrades != null)
            {
                // Добавяме всички допълнителни оценки с техните кодове
                foreach (var grade in userGrades.AdditionalGrades)
                {
                    string key = grade.SubjectCode;

                    // Ако имаме различни типове компоненти, добавяме индикатор за типа
                    if (grade.ComponentType == ComponentTypes.NationalExam)
                    {
                        key = $"{grade.SubjectCode}1";
                    }
                    else if (grade.ComponentType == ComponentTypes.EntranceExam)
                    {
                        key = $"{grade.SubjectCode}2";
                    }

                    valueMap[key] = grade.Value;

                    // Добавяме и стойности за конвертирани в точки оценки
                    if (grade.ComponentType == ComponentTypes.YearlyGrade)
                    {
                        valueMap[$"{key}_ТОЧКИ"] = ConvertGradeToPoints(grade.Value);
                    }

                    // Добавяме и запис с оригиналния код за гъвкавост
                    if (!valueMap.ContainsKey(grade.SubjectCode))
                    {
                        valueMap[grade.SubjectCode] = grade.Value;
                    }
                }
            }

            // Логваме всички налични стойности за дебъг
            foreach (var entry in valueMap)
            {
                _logger.LogDebug($"Стойност за {entry.Key}: {entry.Value}");
            }

            return valueMap;
        }

        // Метод за изчисляване на математически израз
        private double EvaluateExpression(string expression)
        {
            // Премахваме излишните скоби и пространства
            expression = expression.Replace("(", "").Replace(")", "").Replace(" ", "");

            _logger.LogDebug($"Опростен израз за изчисление: {expression}");

            // Разделяме израза на компоненти по оператора за събиране
            string[] addComponents = expression.Split('+');
            double result = 0;

            // Обработваме всеки компонент
            foreach (string component in addComponents)
            {
                // Проверяваме дали компонентът съдържа умножение
                if (component.Contains("*"))
                {
                    // Разделяме по оператора за умножение
                    string[] factors = component.Split('*');

                    if (factors.Length != 2)
                    {
                        throw new ArgumentException($"Неочакван формат на компонент: {component}");
                    }

                    // Парсваме и умножаваме факторите
                    if (double.TryParse(factors[0], out double factor1) &&
                        double.TryParse(factors[1], out double factor2))
                    {
                        result += factor1 * factor2;
                        _logger.LogDebug($"Умножение: {factor1} * {factor2} = {factor1 * factor2}");
                    }
                    else
                    {
                        throw new ArgumentException($"Не може да се парсне числова стойност: {factors[0]} или {factors[1]}");
                    }
                }
                // Проверяваме дали компонентът е просто число
                else if (double.TryParse(component, out double value))
                {
                    result += value;
                    _logger.LogDebug($"Добавяне на стойност: {value}");
                }
                else
                {
                    throw new ArgumentException($"Неочакван формат на компонент: {component}");
                }
            }

            _logger.LogDebug($"Изчислен резултат: {result}");
            return result;
        }

        // Помощен метод за изчисляване на бал от компоненти на формулата
        private double CalculateScoreFromComponents(UserGrades userGrades, IEnumerable<FormulaComponent> components)
        {
            double score = 0;

            foreach (var component in components)
            {
                double componentValue = GetComponentValue(userGrades, component.SubjectCode, component.ComponentType);
                double contribution = componentValue * component.Multiplier;

                _logger.LogDebug($"Компонент: {component.SubjectCode} (тип {component.ComponentType}), " +
                                $"стойност: {componentValue}, коефициент: {component.Multiplier}, " +
                                $"принос: {contribution}");

                score += contribution;
            }

            return score;
        }

        // Метод за получаване на стойност на компонент от оценките
        private double GetComponentValue(UserGrades userGrades, string subjectCode, int componentType)
        {
            // Проверка за основните предмети
            if (subjectCode == SubjectCodes.BulgarianLanguage) // "БЕЛ"
            {
                if (componentType == ComponentTypes.YearlyGrade) // Годишна оценка
                {
                    return userGrades.BulgarianGrade ?? 0;
                }
                else if (componentType == ComponentTypes.NationalExam) // НВО
                {
                    return userGrades.BulgarianExamPoints;
                }
                else if (componentType == ComponentTypes.YearlyGradeAsPoints) // Годишна оценка като точки
                {
                    return ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0);
                }
            }
            else if (subjectCode == SubjectCodes.Mathematics) // "МАТ"
            {
                if (componentType == ComponentTypes.YearlyGrade) // Годишна оценка
                {
                    return userGrades.MathGrade ?? 0;
                }
                else if (componentType == ComponentTypes.NationalExam) // НВО
                {
                    return userGrades.MathExamPoints;
                }
                else if (componentType == ComponentTypes.YearlyGradeAsPoints) // Годишна оценка като точки
                {
                    return ConvertGradeToPoints(userGrades.MathGrade ?? 0);
                }
            }

            // Търсим в допълнителните оценки
            var additionalGrade = userGrades.AdditionalGrades
                ?.FirstOrDefault(ag => ag.SubjectCode == subjectCode && ag.ComponentType == componentType);

            if (additionalGrade != null)
            {
                if (componentType == ComponentTypes.YearlyGradeAsPoints)
                {
                    return ConvertGradeToPoints(additionalGrade.Value);
                }
                return additionalGrade.Value;
            }

            // Проверка за конвертиране между типове компоненти
            if (componentType == ComponentTypes.YearlyGradeAsPoints)
            {
                // Търсим годишна оценка и я конвертираме в точки
                var yearlyGrade = userGrades.AdditionalGrades?
                    .FirstOrDefault(ag => ag.SubjectCode == subjectCode && ag.ComponentType == ComponentTypes.YearlyGrade);

                if (yearlyGrade != null)
                {
                    return ConvertGradeToPoints(yearlyGrade.Value);
                }
            }

            // Проверяваме и за близки съвпадения (например AE вместо АЕ)
            if (componentType == ComponentTypes.YearlyGrade || componentType == ComponentTypes.YearlyGradeAsPoints)
            {
                var similarGrade = userGrades.AdditionalGrades?
                    .FirstOrDefault(ag => (ag.ComponentType == ComponentTypes.YearlyGrade) &&
                                    IsSubjectCodeSimilar(ag.SubjectCode, subjectCode));

                if (similarGrade != null)
                {
                    _logger.LogWarning($"Използвана приблизителна оценка за {subjectCode}: " +
                                     $"{similarGrade.SubjectCode} = {similarGrade.Value}");

                    if (componentType == ComponentTypes.YearlyGradeAsPoints)
                    {
                        return ConvertGradeToPoints(similarGrade.Value);
                    }
                    return similarGrade.Value;
                }
            }

            // Ако не намерим оценка, връщаме 0
            _logger.LogWarning($"Не е намерена оценка за предмет {subjectCode}, тип {componentType}");
            return 0;
        }

        // Помощен метод за проверка на близки съвпадения на кодове на предмети
        private bool IsSubjectCodeSimilar(string code1, string code2)
        {
            // Проверка за идентични кодове
            if (string.Equals(code1, code2, StringComparison.OrdinalIgnoreCase))
                return true;

            // Проверка за AE/АЕ, HE/НЕ, и т.н. (латиница vs кирилица)
            string normalized1 = NormalizeSubjectCode(code1);
            string normalized2 = NormalizeSubjectCode(code2);

            return string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase);
        }

        // Помощен метод за нормализиране на кодове на предмети
        private string NormalizeSubjectCode(string code)
        {
            // Заместваме латински букви с кирилски и обратно
            return code.Replace("A", "А")
                       .Replace("А", "A")
                       .Replace("E", "Е")
                       .Replace("Е", "E")
                       .Replace("O", "О")
                       .Replace("О", "O")
                       .Replace("C", "С")
                       .Replace("С", "C")
                       .Replace("P", "Р")
                       .Replace("Р", "P")
                       .Replace("X", "Х")
                       .Replace("Х", "X")
                       .Replace("B", "В")
                       .Replace("В", "B")
                       .Replace("H", "Н")
                       .Replace("Н", "H")
                       .Replace("M", "М")
                       .Replace("М", "M");
        }

        // Изчисляване на процент шанс за прием
        private double CalculateChancePercentage(double calculatedScore, double minimumScore)
        {
            if (minimumScore <= 0)
            {
                return 50; // Няма данни за минимален бал, връщаме 50%
            }

            if (calculatedScore >= minimumScore + 20)
            {
                return 99; // Много висок бал
            }
            else if (calculatedScore >= minimumScore)
            {
                // Линейна интерполация между 75% и 95%
                double ratio = (calculatedScore - minimumScore) / 20.0;
                return 75 + (ratio * 20);
            }
            else if (calculatedScore >= minimumScore - 20)
            {
                // Линейна интерполация между 15% и 75%
                double ratio = (calculatedScore - (minimumScore - 20)) / 20.0;
                return 15 + (ratio * 60);
            }
            else
            {
                return 10; // Много нисък бал
            }
        }

        private static double ConvertGradeToPoints(double grade)
        {
            if (grade >= 5.50) return 50; // Отличен 6
            if (grade >= 4.50) return 39; // Много добър 5
            if (grade >= 3.50) return 26; // Добър 4
            if (grade >= 2.50) return 15; // Среден 3
            return 0; // По-ниски оценки
        }

    }
}