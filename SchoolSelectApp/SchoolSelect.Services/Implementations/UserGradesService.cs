using Microsoft.Extensions.Logging;
using SchoolSelect.Common;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;
using System.Data;
using System.Text.RegularExpressions;

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

            try
            {
                // Извличане на отделните части на формулата
                string cleanedFormula = formulaExpression.Trim();

                // Проверяваме дали формулата отговаря на шаблона с две части в скоби
                if (cleanedFormula.Contains("+") && cleanedFormula.Contains("("))
                {
                    // Извличаме двете части от формулата, заградени в скоби
                    var match = Regex.Match(cleanedFormula, @"\(([^)]+)\)\s*\+\s*\(([^)]+)\)");
                    if (match.Success && match.Groups.Count >= 3)
                    {
                        string nvoFormulaPart = match.Groups[1].Value.Trim();
                        string gradesFormulaPart = match.Groups[2].Value.Trim();

                        _logger.LogDebug($"Извлечена НВО формула: {nvoFormulaPart}");
                        _logger.LogDebug($"Извлечена формула за годишни оценки: {gradesFormulaPart}");

                        // В първата част са точките от НВО
                        double nvoScore = CalculateNVOScore(userGrades, nvoFormulaPart);
                        _logger.LogDebug($"Резултат от НВО част: {nvoScore}");

                        // Във втората част са годишни оценки, превърнати в точки
                        double gradesScore = CalculateGradesScore(userGrades, gradesFormulaPart);
                        _logger.LogDebug($"Резултат от годишни оценки: {gradesScore}");

                        // Общият резултат е сумата от двете части
                        double totalScore = nvoScore + gradesScore;
                        _logger.LogDebug($"Общ резултат (НВО + оценки): {nvoScore} + {gradesScore} = {totalScore}");

                        return totalScore;
                    }
                    else
                    {
                        _logger.LogWarning("Формулата не отговаря на очаквания шаблон с две части в скоби.");
                    }
                }

                // Ако не успеем да разделим формулата на части по шаблона, опитваме общ подход
                _logger.LogDebug("Опитваме общ подход за изчисляване на формулата.");

                // Замяна на всички променливи в израза с техните стойности
                var valueDict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    // НВО точки
                    { "БЕЛ", userGrades.BulgarianExamPoints },
                    { "BEL", userGrades.BulgarianExamPoints },
                    { "М", userGrades.MathExamPoints },
                    { "M", userGrades.MathExamPoints },
                    { "МАТ", userGrades.MathExamPoints },
                    { "MAT", userGrades.MathExamPoints },
                    
                    // Годишни оценки, превърнати в точки
                    { "БЕЛ_ТОЧКИ", ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0) },
                    { "BEL_ТОЧКИ", ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0) },
                    { "МАТ_ТОЧКИ", ConvertGradeToPoints(userGrades.MathGrade ?? 0) },
                    { "MAT_ТОЧКИ", ConvertGradeToPoints(userGrades.MathGrade ?? 0) }
                };

                // Заместваме всички променливи във формулата
                string processedFormula = cleanedFormula;
                foreach (var entry in valueDict)
                {
                    processedFormula = ReplaceVariableInFormula(processedFormula, entry.Key, entry.Value);
                }

                _logger.LogDebug($"Формула след заместване на променливи: {processedFormula}");

                // Оценяваме израза
                using (var dt = new DataTable())
                {
                    var result = Convert.ToDouble(dt.Compute(processedFormula, string.Empty));
                    _logger.LogDebug($"Резултат от изчисление: {result}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Грешка при парсване на формула '{formulaExpression}': {ex.Message}");
                throw;
            }
        }

        // Метод за заместване на променлива във формула
        private string ReplaceVariableInFormula(string formula, string variable, double value)
        {
            // Използваме регулярен израз, за да заместим променливата само когато е самостоятелна дума
            return Regex.Replace(
                formula,
                $@"\b{Regex.Escape(variable)}\b",
                value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            );
        }

        // Изчисляване на частта с НВО точки
        private double CalculateNVOScore(UserGrades userGrades, string formula)
        {
            _logger.LogDebug($"Изчисляване на НВО част: {formula}");

            // Подготвяме речник със стойностите за НВО
            var nvoValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "БЕЛ", userGrades.BulgarianExamPoints },
                { "BEL", userGrades.BulgarianExamPoints },
                { "MAT", userGrades.MathExamPoints },
                { "МАТ", userGrades.MathExamPoints },
                { "M", userGrades.MathExamPoints },
                { "М", userGrades.MathExamPoints }
            };

            // Заместваме всички променливи в израза
            string processedFormula = formula;
            foreach (var entry in nvoValues)
            {
                processedFormula = ReplaceVariableInFormula(processedFormula, entry.Key, entry.Value);
            }

            _logger.LogDebug($"НВО формула след заместване: {processedFormula}");

            // Изчисляваме и връщаме резултата
            using (var dt = new DataTable())
            {
                var result = Convert.ToDouble(dt.Compute(processedFormula, string.Empty));
                _logger.LogDebug($"Резултат от НВО част: {result}");
                return result;
            }
        }

        // Изчисляване на частта с годишни оценки (превърнати в точки)
        private double CalculateGradesScore(UserGrades userGrades, string formula)
        {
            _logger.LogDebug($"Изчисляване на част с годишни оценки: {formula}");

            // Подготвяме речник със стойностите за годишни оценки, превърнати в точки
            var gradeValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "БЕЛ", ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0) },
                { "BEL", ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0) },
                { "MAT", ConvertGradeToPoints(userGrades.MathGrade ?? 0) },
                { "МАТ", ConvertGradeToPoints(userGrades.MathGrade ?? 0) },
                { "M", ConvertGradeToPoints(userGrades.MathGrade ?? 0) },
                { "М", ConvertGradeToPoints(userGrades.MathGrade ?? 0) }
            };

            // Заместваме всички променливи в израза
            string processedFormula = formula;
            foreach (var entry in gradeValues)
            {
                processedFormula = ReplaceVariableInFormula(processedFormula, entry.Key, entry.Value);
            }

            _logger.LogDebug($"Формула с годишни оценки след заместване: {processedFormula}");

            // Изчисляваме и връщаме резултата
            using (var dt = new DataTable())
            {
                var result = Convert.ToDouble(dt.Compute(processedFormula, string.Empty));
                _logger.LogDebug($"Резултат от годишни оценки: {result}");
                return result;
            }
        }

        // Опростен метод за изчисляване на израз
        private double EvaluateSimpleExpression(string expression)
        {
            try
            {
                // Премахваме интервали
                expression = expression.Replace(" ", "");

                // Използваме DataTable.Compute за безопасно изчисление
                using (var dt = new DataTable())
                {
                    return Convert.ToDouble(dt.Compute(expression, ""));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Грешка при изчисляване на израз с DataTable: {ex.Message}");
                return EvaluateExpressionFallback(expression);
            }
        }

        // Резервен метод за изчисляване (ако DataTable.Compute не работи)
        private double EvaluateExpressionFallback(string expression)
        {
            // Опростена обработка на аритметични изрази
            expression = expression.Replace(" ", "").Replace("(", "").Replace(")", "");

            // Събиране
            if (expression.Contains("+"))
            {
                var parts = expression.Split('+');
                return parts.Sum(p => EvaluateExpressionFallback(p));
            }

            // Умножение
            if (expression.Contains("*"))
            {
                var parts = expression.Split('*');
                double result = 1;
                foreach (var part in parts)
                {
                    if (double.TryParse(part, out double partValue))
                    {
                        result *= partValue;
                    }
                    else
                    {
                        _logger.LogWarning($"Не може да се парсне стойност: {part}");
                    }
                }
                return result;
            }

            // Просто число
            if (double.TryParse(expression, out double expressionValue))
            {
                return expressionValue;
            }

            _logger.LogWarning($"Не може да се изчисли израз: {expression}");
            return 0;
        }

        // Метод за подготовка на речник с всички възможни стойности
        private Dictionary<string, double> PrepareValueDictionary(UserGrades userGrades)
        {
            var valueMap = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            // Добавяме основните оценки (като оценки и като точки)
            valueMap["БЕЛ"] = userGrades.BulgarianGrade ?? 0;  // Годишна оценка по БЕЛ
            valueMap["МАТ"] = userGrades.MathGrade ?? 0;       // Годишна оценка по Математика
            valueMap["BEL"] = userGrades.BulgarianGrade ?? 0;  // Латински вариант
            valueMap["MAT"] = userGrades.MathGrade ?? 0;       // Латински вариант

            // Добавяме конвертираните в точки оценки
            valueMap["БЕЛ_ТОЧКИ"] = ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0);
            valueMap["МАТ_ТОЧКИ"] = ConvertGradeToPoints(userGrades.MathGrade ?? 0);
            valueMap["BEL_ТОЧКИ"] = ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0);
            valueMap["MAT_ТОЧКИ"] = ConvertGradeToPoints(userGrades.MathGrade ?? 0);

            // Добавяме точките от НВО
            valueMap["БЕЛ1"] = userGrades.BulgarianExamPoints; // НВО по БЕЛ
            valueMap["М"] = userGrades.MathExamPoints;         // НВО по Математика (формат от примера)
            valueMap["МАТ1"] = userGrades.MathExamPoints;      // Алтернативен формат за НВО по Математика
            valueMap["M"] = userGrades.MathExamPoints;         // Латински вариант
            valueMap["MAT1"] = userGrades.MathExamPoints;      // Латински вариант
            valueMap["BEL1"] = userGrades.BulgarianExamPoints; // Латински вариант

            // Проверяваме дали имаме допълнителни оценки
            if (userGrades.AdditionalGrades != null)
            {
                // Добавяме всички допълнителни оценки с техните кодове
                foreach (var grade in userGrades.AdditionalGrades)
                {
                    string key = grade.SubjectCode;
                    string keyLatin = NormalizeToLatin(key);
                    string keyCyrillic = NormalizeToCyrillic(key);

                    // Ако имаме различни типове компоненти, добавяме индикатор за типа
                    if (grade.ComponentType == ComponentTypes.NationalExam)
                    {
                        valueMap[$"{key}1"] = grade.Value;
                        valueMap[$"{keyLatin}1"] = grade.Value;
                        valueMap[$"{keyCyrillic}1"] = grade.Value;
                    }
                    else if (grade.ComponentType == ComponentTypes.EntranceExam)
                    {
                        valueMap[$"{key}2"] = grade.Value;
                        valueMap[$"{keyLatin}2"] = grade.Value;
                        valueMap[$"{keyCyrillic}2"] = grade.Value;
                    }
                    else if (grade.ComponentType == ComponentTypes.YearlyGrade)
                    {
                        valueMap[key] = grade.Value;
                        valueMap[keyLatin] = grade.Value;
                        valueMap[keyCyrillic] = grade.Value;

                        // Добавяме и превърнати в точки годишни оценки
                        valueMap[$"{key}_ТОЧКИ"] = ConvertGradeToPoints(grade.Value);
                        valueMap[$"{keyLatin}_ТОЧКИ"] = ConvertGradeToPoints(grade.Value);
                        valueMap[$"{keyCyrillic}_ТОЧКИ"] = ConvertGradeToPoints(grade.Value);
                    }
                    else if (grade.ComponentType == ComponentTypes.YearlyGradeAsPoints)
                    {
                        valueMap[$"{key}_ТОЧКИ"] = grade.Value;
                        valueMap[$"{keyLatin}_ТОЧКИ"] = grade.Value;
                        valueMap[$"{keyCyrillic}_ТОЧКИ"] = grade.Value;
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

        // Помощен метод за трансформиране на кирилица в латиница
        private string NormalizeToLatin(string input)
        {
            return input
                .Replace("А", "A")
                .Replace("Б", "B")
                .Replace("В", "B")
                .Replace("Г", "G")
                .Replace("Д", "D")
                .Replace("Е", "E")
                .Replace("Ж", "J")
                .Replace("З", "Z")
                .Replace("И", "I")
                .Replace("Й", "Y")
                .Replace("К", "K")
                .Replace("Л", "L")
                .Replace("М", "M")
                .Replace("Н", "H")
                .Replace("О", "O")
                .Replace("П", "P")
                .Replace("Р", "P")
                .Replace("С", "C")
                .Replace("Т", "T")
                .Replace("У", "U")
                .Replace("Ф", "F")
                .Replace("Х", "X")
                .Replace("Ц", "C")
                .Replace("Ч", "CH")
                .Replace("Ш", "SH")
                .Replace("Щ", "SHT")
                .Replace("Ъ", "A")
                .Replace("Ь", "")
                .Replace("Ю", "YU")
                .Replace("Я", "YA");
        }

        // Помощен метод за трансформиране на латиница в кирилица
        private string NormalizeToCyrillic(string input)
        {
            return input
                .Replace("A", "А")
                .Replace("B", "В")
                .Replace("C", "С")
                .Replace("D", "Д")
                .Replace("E", "Е")
                .Replace("F", "Ф")
                .Replace("G", "Г")
                .Replace("H", "Н")
                .Replace("I", "И")
                .Replace("J", "Ж")
                .Replace("K", "К")
                .Replace("L", "Л")
                .Replace("M", "М")
                .Replace("N", "Н")
                .Replace("O", "О")
                .Replace("P", "П")
                .Replace("Q", "К")
                .Replace("R", "Р")
                .Replace("S", "С")
                .Replace("T", "Т")
                .Replace("U", "У")
                .Replace("V", "В")
                .Replace("W", "В")
                .Replace("X", "Х")
                .Replace("Y", "Й")
                .Replace("Z", "З");
        }

        // Метод за изчисляване на математически израз
        private double EvaluateExpression(string expression)
        {
            try
            {
                // Премахваме интервали
                expression = expression.Replace(" ", "");

                // Използваме DataTable.Compute за безопасно изчисление
                using (var dt = new DataTable())
                {
                    var result = Convert.ToDouble(dt.Compute(expression, string.Empty));
                    _logger.LogDebug($"Резултат от изчисление: {result}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Грешка с DataTable.Compute: {ex.Message}, опитваме ръчно изчисление");

                // Премахваме скобите и пространствата
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
                        double mulResult = 1.0;

                        foreach (var factor in factors)
                        {
                            if (double.TryParse(factor, out double factorValue))
                            {
                                mulResult *= factorValue;
                            }
                            else
                            {
                                _logger.LogWarning($"Не може да се парсне числото: {factor}");
                            }
                        }

                        result += mulResult;
                        _logger.LogDebug($"Умножение: {component} = {mulResult}");
                    }
                    // Проверяваме дали компонентът е просто число
                    else if (double.TryParse(component, out double value))
                    {
                        result += value;
                        _logger.LogDebug($"Добавяне на стойност: {value}");
                    }
                    else
                    {
                        _logger.LogWarning($"Неочакван формат на компонент: {component}");
                    }
                }

                _logger.LogDebug($"Изчислен резултат: {result}");
                return result;
            }
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