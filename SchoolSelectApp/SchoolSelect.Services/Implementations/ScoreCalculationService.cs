using Microsoft.Extensions.Logging;
using SchoolSelect.Common;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    /// <summary>
    /// Услуга за изчисляване на бал по формула - преработена версия,
    /// използваща компонентния подход за изчисление
    /// </summary>
    public class ScoreCalculationService : IScoreCalculationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ScoreCalculationService> _logger;

        public ScoreCalculationService(
            IUnitOfWork unitOfWork,
            ILogger<ScoreCalculationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<double> CalculateScoreAsync(int formulaId, UserGrades userGrades)
        {
            var formula = await _unitOfWork.AdmissionFormulas.GetFormulaWithComponentsAsync(formulaId);

            if (formula == null)
            {
                _logger.LogWarning("Формула с ID {FormulaId} не е намерена", formulaId);
                return 0;
            }

            // Проверяваме дали формулата има компоненти
            if (formula.HasComponents && formula.Components != null && formula.Components.Any())
            {
                _logger.LogInformation("Изчисляване на бал по компоненти за формула с ID {FormulaId}", formulaId);
                return CalculateByComponents(formula, userGrades);
            }
            else
            {
                _logger.LogWarning("Формула с ID {FormulaId} няма компоненти. Използваме резервен метод.", formulaId);
                return CalculateDefaultScore(userGrades);
            }
        }

        private double CalculateByComponents(AdmissionFormula formula, UserGrades userGrades)
        {
            double totalScore = 0;

            foreach (var component in formula.Components)
            {
                double value = 0;

                // Определяне на стойността според типа на компонента
                value = component.ComponentType switch
                {
                    // Базови типове
                    ComponentTypes.NationalExam => GetExamPoints(component.SubjectCode, userGrades),
                    ComponentTypes.YearlyGrade => ConvertGradeToPoints(GetGrade(component.SubjectCode, userGrades)),

                    ComponentTypes.EntranceExam => GetExamPoints(component.SubjectCode, userGrades),

                    // Нови специализирани типове за олимпиади и състезания
                    ComponentTypes.PhysicsOlympiad60Percent => GetCompetitionPoints(component.SubjectCode, "ФИЗ", "PhysicsOlympiad", userGrades),
                    ComponentTypes.BiologyOlympiad60Percent => GetCompetitionPoints(component.SubjectCode, "БИО", "BiologyOlympiad", userGrades),
                    ComponentTypes.ChemistryOlympiad60Percent => GetCompetitionPoints(component.SubjectCode, "ХИМ", "ChemistryOlympiad", userGrades),
                    ComponentTypes.ChakalovTalentedBiologist => GetCompetitionPoints(component.SubjectCode, "БИО", "ChakalovBiology", userGrades),
                    ComponentTypes.ChakalovTalentedChemist => GetCompetitionPoints(component.SubjectCode, "ХИМ", "ChakalovChemistry", userGrades),
                    ComponentTypes.GeographyOlympiad60Percent => GetCompetitionPoints(component.SubjectCode, "ГЕО", "GeographyOlympiad", userGrades),

                    // Default за неразпознати типове
                    _ => 0
                };

                // Прилагане на коефициента
                double componentScore = component.Multiplier * value;
                _logger.LogDebug("Компонент {SubjectCode} ({SubjectName}): {Multiplier} * {Value} = {Score}",
                    component.SubjectCode, component.SubjectName, component.Multiplier, value, componentScore);

                totalScore += componentScore;
            }

            _logger.LogInformation("Общ бал: {TotalScore}", totalScore);
            return totalScore;
        }

        /// <summary>
        /// Метод за изчисляване на точки от олимпиади и състезания
        /// </summary>
        private double GetCompetitionPoints(string subjectCode, string defaultSubject, string competitionType, UserGrades userGrades)
        {
            // Определяме предмета - ако е подаден конкретен, използваме него, иначе използваме подразбиращия се
            string effectiveSubject = !string.IsNullOrEmpty(subjectCode) ? subjectCode : defaultSubject;

            // Търсим в допълнителните оценки по код на предмет и тип на компонента според състезанието
            var additionalGrade = userGrades.AdditionalGrades
                .FirstOrDefault(g => g.SubjectCode.Equals(effectiveSubject, StringComparison.OrdinalIgnoreCase) &&
                                    g.Value > 0 &&
                                    g.SubjectName.Contains(competitionType, StringComparison.OrdinalIgnoreCase));

            if (additionalGrade != null)
            {
                _logger.LogDebug("Намерена е оценка от състезание {CompetitionType} по предмет {Subject}: {Value}",
                    competitionType, effectiveSubject, additionalGrade.Value);
                return additionalGrade.Value;
            }

            // Ако не намерим директно съвпадение по описание, търсим по-общо съвпадение за олимпиада или Чакалов
            if (competitionType.Contains("Olympiad"))
            {
                // Търсим оценка от олимпиада по предмета
                additionalGrade = userGrades.AdditionalGrades
                    .FirstOrDefault(g => g.SubjectCode.Equals(effectiveSubject, StringComparison.OrdinalIgnoreCase) &&
                                        g.Value > 0 &&
                                        g.SubjectName.Contains("олимпиада", StringComparison.OrdinalIgnoreCase));
            }
            else if (competitionType.Contains("Chakalov"))
            {
                // Търсим оценка от състезание Чакалов
                additionalGrade = userGrades.AdditionalGrades
                    .FirstOrDefault(g => g.SubjectCode.Equals(effectiveSubject, StringComparison.OrdinalIgnoreCase) &&
                                        g.Value > 0 &&
                                        g.SubjectName.Contains("чакалов", StringComparison.OrdinalIgnoreCase));
            }

            if (additionalGrade != null)
            {
                _logger.LogDebug("Намерена е обща оценка от състезание за предмет {Subject}: {Value}",
                    effectiveSubject, additionalGrade.Value);
                return additionalGrade.Value;
            }

            _logger.LogWarning("Не е намерена оценка от състезание {CompetitionType} по предмет {Subject}",
                competitionType, effectiveSubject);
            return 0;
        }

        /// <summary>
        /// Резервен метод за изчисление на бал
        /// </summary>
        private double CalculateDefaultScore(UserGrades userGrades)
        {
            _logger.LogInformation("Използване на стандартна формула (2*БЕЛ_НВО + 2*МАТ_НВО + 1*БЕЛ_годишна + 1*МАТ_годишна)");

            // Стандартна формула
            double examScore = (2 * userGrades.BulgarianExamPoints) + (2 * userGrades.MathExamPoints);
            double gradeScore = ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0) +
                               ConvertGradeToPoints(userGrades.MathGrade ?? 0);

            double totalScore = examScore + gradeScore;
            _logger.LogInformation("Стандартен бал: {TotalScore}", totalScore);
            return totalScore;
        }

        /// <summary>
        /// Взима точки от НВО според кода на предмета
        /// </summary>
        private double GetExamPoints(string subjectCode, UserGrades userGrades)
        {
            return subjectCode.ToUpperInvariant() switch
            {
                "БЕЛ" => userGrades.BulgarianExamPoints,
                "МАТ" => userGrades.MathExamPoints,
                _ => GetAdditionalExamPoints(subjectCode, userGrades)
            };
        }

        /// <summary>
        /// Взима оценка според кода на предмета
        /// </summary>
        private double GetGrade(string subjectCode, UserGrades userGrades)
        {
            return subjectCode.ToUpperInvariant() switch
            {
                "БЕЛ" => userGrades.BulgarianGrade ?? 0,
                "МАТ" => userGrades.MathGrade ?? 0,
                _ => GetAdditionalGrade(subjectCode, userGrades)
            };
        }

        /// <summary>
        /// Взима годишна оценка като точки (без конвертиране)
        /// </summary>
        private double GetGradeAsPoints(string subjectCode, UserGrades userGrades)
        {
            // Тук можете да добавите специална логика за годишни оценки като точки
            // За момента просто връщаме оценката
            return GetGrade(subjectCode, userGrades);
        }

        /// <summary>
        /// Взима точки от допълнителен изпит според кода
        /// </summary>
        private double GetAdditionalExamPoints(string subjectCode, UserGrades userGrades)
        {
            var additionalGrade = userGrades.AdditionalGrades
                .FirstOrDefault(g => g.SubjectCode.Equals(subjectCode, StringComparison.OrdinalIgnoreCase) &&
                                    g.ComponentType == ComponentTypes.NationalExam);

            return additionalGrade?.Value ?? 0;
        }

        /// <summary>
        /// Взима допълнителна оценка според кода
        /// </summary>
        private double GetAdditionalGrade(string subjectCode, UserGrades userGrades)
        {
            var additionalGrade = userGrades.AdditionalGrades
                .FirstOrDefault(g => g.SubjectCode.Equals(subjectCode, StringComparison.OrdinalIgnoreCase) &&
                                    g.ComponentType == ComponentTypes.YearlyGrade);

            return additionalGrade?.Value ?? 0;
        }

        /// <summary>
        /// Конвертира оценки (2-6) в точки (0-50)
        /// </summary>
        private static double ConvertGradeToPoints(double grade)
        {
            if (grade >= 5.50) return 50;
            if (grade >= 4.50) return 39;
            if (grade >= 3.50) return 26;
            if (grade >= 2.50) return 15;
            return 0;
        }
    }
}