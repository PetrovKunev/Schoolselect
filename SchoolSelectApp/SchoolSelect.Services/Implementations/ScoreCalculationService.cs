using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    /// <summary>
    /// Услуга за изчисляване на бал по формула
    /// </summary>
    public class ScoreCalculationService : IScoreCalculationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExpressionParser _parser; // Запазваме старата логика за обратна съвместимост

        public ScoreCalculationService(IUnitOfWork unitOfWork, IExpressionParser parser)
        {
            _unitOfWork = unitOfWork;
            _parser = parser;
        }

        public async Task<double> CalculateScoreAsync(int formulaId, UserGrades userGrades)
        {
            var formula = await _unitOfWork.AdmissionFormulas.GetByIdAsync(formulaId);

            // Проверяваме дали формулата е структурирана
            if (formula.IsStructured)
            {
                // Използваме структурирания подход
                return CalculateStructuredScore(formula, userGrades);
            }
            else
            {
                // Използваме старата логика за обратна съвместимост
                var variables = ResolveVariables(userGrades);
                return _parser.Evaluate(formula.FormulaExpression, variables);
            }
        }

        private double CalculateStructuredScore(AdmissionFormula formula, UserGrades userGrades)
        {
            // НВО компонента
            double examScore =
                formula.BelExamMultiplier * userGrades.BulgarianExamPoints +
                formula.MatExamMultiplier * userGrades.MathExamPoints;

            // Годишни оценки компонента
            double gradeScore =
                formula.BelGradeMultiplier * ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0) +
                formula.MatGradeMultiplier * ConvertGradeToPoints(userGrades.MathGrade ?? 0);

            // Проверка за допълнителни предмети (КМИТ и др.)
            if (formula.KmitGradeMultiplier > 0)
            {
                // Търсим КМИТ в допълнителните оценки
                var kmitGrade = userGrades.AdditionalGrades.FirstOrDefault(g => g.SubjectCode == "КМИТ");
                if (kmitGrade != null)
                {
                    gradeScore += formula.KmitGradeMultiplier * ConvertGradeToPoints(kmitGrade.Value);
                }
            }

            return examScore + gradeScore;
        }

        private static double ConvertGradeToPoints(double grade)
        {
            if (grade >= 5.50) return 50;
            if (grade >= 4.50) return 39;
            if (grade >= 3.50) return 26;
            if (grade >= 2.50) return 15;
            return 0;
        }

        // Метод от старата логика
        private IDictionary<string, double> ResolveVariables(UserGrades userGrades)
        {
            // Имплементация на старата логика
            var variables = new Dictionary<string, double>();

            // Основни променливи
            variables["BEL"] = userGrades.BulgarianExamPoints;
            variables["MAT"] = userGrades.MathExamPoints;

            // Годишни оценки като точки
            variables["BEL_POINTS"] = ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0);
            variables["MAT_POINTS"] = ConvertGradeToPoints(userGrades.MathGrade ?? 0);

            // Добавяне на допълнителни променливи от оценките
            foreach (var grade in userGrades.AdditionalGrades)
            {
                string code = NormalizeSubjectCode(grade.SubjectCode);
                variables[code] = grade.Value;
                variables[$"{code}_POINTS"] = ConvertGradeToPoints(grade.Value);
            }

            return variables;
        }

        private string NormalizeSubjectCode(string code)
        {
            // Опростена нормализация на кодовете на предметите
            return code.ToUpperInvariant().Trim();
        }
    }
}