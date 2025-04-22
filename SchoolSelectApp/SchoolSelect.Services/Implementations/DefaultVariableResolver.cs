using SchoolSelect.Common;
using SchoolSelect.Data.Models;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    public class DefaultVariableResolver : IVariableResolver
    {
        public IDictionary<string, double> Resolve(UserGrades userGrades)
        {
            var vars = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            // 1) Точки от НВО
            vars["BEL"] = userGrades.BulgarianExamPoints;
            vars["MAT"] = userGrades.MathExamPoints;

            // 2) Годишни оценки, превърнати в точки
            vars["BEL_POINTS"] = ConvertGradeToPoints(userGrades.BulgarianGrade ?? 0);
            vars["MAT_POINTS"] = ConvertGradeToPoints(userGrades.MathGrade ?? 0);

            // 3) Допълнителни оценки
            if (userGrades.AdditionalGrades != null)
            {
                foreach (var ag in userGrades.AdditionalGrades)
                {
                    // Превръщаме оригиналния код (кирилица или латиница) в ASCII-only
                    string code = TransliterateToLatin(ag.SubjectCode);

                    switch (ag.ComponentType)
                    {
                        case ComponentTypes.NationalExam:
                            vars[$"{code}1"] = ag.Value;
                            break;
                        case ComponentTypes.EntranceExam:
                            vars[$"{code}2"] = ag.Value;
                            break;
                        case ComponentTypes.YearlyGrade:
                            // годишна оценка и като точки
                            vars[code] = ag.Value;
                            vars[$"{code}_POINTS"] = ConvertGradeToPoints(ag.Value);
                            break;
                        case ComponentTypes.YearlyGradeAsPoints:
                            vars[$"{code}_POINTS"] = ag.Value;
                            break;
                    }
                }
            }

            return vars;
        }

        private static double ConvertGradeToPoints(double grade)
        {
            if (grade >= 5.50) return 50;
            if (grade >= 4.50) return 39;
            if (grade >= 3.50) return 26;
            if (grade >= 2.50) return 15;
            return 0;
        }

        private string TransliterateToLatin(string input)
        {
            // Вашата съществуваща карта за транслитерация
            return input
                .Replace("А", "A").Replace("Б", "B").Replace("В", "V")
                .Replace("Г", "G").Replace("Д", "D").Replace("Е", "E")
                .Replace("Ж", "J").Replace("З", "Z").Replace("И", "I")
                .Replace("Й", "Y").Replace("К", "K").Replace("Л", "L")
                .Replace("М", "M").Replace("Н", "N").Replace("О", "O")
                .Replace("П", "P").Replace("Р", "R").Replace("С", "S")
                .Replace("Т", "T").Replace("У", "U").Replace("Ф", "F")
                .Replace("Х", "X").Replace("Ц", "C").Replace("Ч", "CH")
                .Replace("Ш", "SH").Replace("Щ", "SHT").Replace("Ъ", "A")
                .Replace("Ь", "").Replace("Ю", "YU").Replace("Я", "YA")
                // и обратните замени, ако трявба да се нормализира латиница
                ;
        }
    }
}
