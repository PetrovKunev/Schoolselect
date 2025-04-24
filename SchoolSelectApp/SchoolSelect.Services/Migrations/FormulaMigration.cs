using System.Text.RegularExpressions;
using SchoolSelect.Repositories.Interfaces;

namespace SchoolSelect.Services.Migrations
{
    public class FormulaMigration
    {
        public static async Task MigrateFormulasAsync(IUnitOfWork unitOfWork)
        {
            var formulas = await unitOfWork.AdmissionFormulas.GetAllAsync();

            foreach (var formula in formulas)
            {
                // Анализираме формулата и извличаме множителите
                ExtractMultipliers(formula.FormulaExpression, out var belExam, out var matExam,
                                  out var belGrade, out var matGrade, out var kmitGrade);

                // Обновяваме структурираните полета
                formula.BelExamMultiplier = belExam;
                formula.MatExamMultiplier = matExam;
                formula.BelGradeMultiplier = belGrade;
                formula.MatGradeMultiplier = matGrade;
                formula.KmitGradeMultiplier = kmitGrade;
                formula.IsStructured = true;

                unitOfWork.AdmissionFormulas.Update(formula);
            }

            await unitOfWork.CompleteAsync();
        }

        private static void ExtractMultipliers(string formula, out double belExam, out double matExam,
                                              out double belGrade, out double matGrade, out double kmitGrade)
        {
            // Инициализираме стойностите по подразбиране
            belExam = matExam = belGrade = matGrade = kmitGrade = 0;

            // Разделяме формулата на две части: НВО и годишни оценки
            var parts = formula.Split(new string[] { ") + (" }, StringSplitOptions.None);
            if (parts.Length != 2) return;

            // Анализираме първата част (НВО)
            string examPart = parts[0];
            ExtractCoefficient(examPart, "БЕЛ", out belExam);
            ExtractCoefficient(examPart, "МАТ", out matExam);

            // Анализираме втората част (годишни оценки)
            string gradePart = parts[1];
            ExtractCoefficient(gradePart, "БЕЛ", out belGrade);
            ExtractCoefficient(gradePart, "МАТ", out matGrade);
            ExtractCoefficient(gradePart, "М", out var mGrade); // М като съкращение
            if (mGrade > 0 && matGrade == 0) matGrade = mGrade;
            ExtractCoefficient(gradePart, "КМИТ", out kmitGrade);
        }

        private static void ExtractCoefficient(string part, string subject, out double coefficient)
        {
            coefficient = 0;
            // Търсим шаблона "X * SUBJECT"
            var pattern = @"(\d+(\.\d+)?) \* " + subject;
            var match = Regex.Match(part, pattern);
            if (match.Success && match.Groups.Count > 1)
            {
                double.TryParse(match.Groups[1].Value, out coefficient);
            }
        }
    }
}
