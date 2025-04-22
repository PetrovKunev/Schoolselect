using SchoolSelect.Common;
using SchoolSelect.Data.Models;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    public class DefaultVariableResolver : IVariableResolver
    {
        // Речник за транслитерация от кирилица на латиница
        private static readonly Dictionary<string, string> _transliterationMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "БЕЛ", "BEL" },
            { "МАТ", "MAT" },
            { "ЧЕз", "CHOZ" },
            { "КМИТ", "KMIT" },
            { "ФИЗ", "FIZ" },
            { "ХИМ", "HIM" },
            { "БИО", "BIO" },
            { "ИСТ", "IST" },
            { "ГЕО", "GEO" },
            { "ФИЛ", "FIL" },
            { "ФВС", "FVS" },
            { "АЕ", "AE" },
            { "НЕ", "NE" },
            { "ФЕ", "FE" },
            { "ИЕ", "IE" },
            { "ИТ", "IT" },
            { "РЕ", "RE" }
        };

        public IDictionary<string, double> Resolve(UserGrades userGrades)
        {
            var vars = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            // 1) Основни предмети - добавяме ги с латински и кирилски имена
            // НВО точки
            vars["BEL"] = userGrades.BulgarianExamPoints;
            vars["MAT"] = userGrades.MathExamPoints;
            vars["БЕЛ"] = userGrades.BulgarianExamPoints;
            vars["МАТ"] = userGrades.MathExamPoints;

            // Годишни оценки, превърнати в точки
            if (userGrades.BulgarianGrade.HasValue)
            {
                vars["BEL_POINTS"] = ConvertGradeToPoints(userGrades.BulgarianGrade.Value);
                vars["БЕЛ_POINTS"] = vars["BEL_POINTS"];
            }

            if (userGrades.MathGrade.HasValue)
            {
                vars["MAT_POINTS"] = ConvertGradeToPoints(userGrades.MathGrade.Value);
                vars["МАТ_POINTS"] = vars["MAT_POINTS"];
            }

            // Суровите годишни оценки (2-6)
            if (userGrades.BulgarianGrade.HasValue)
            {
                vars["BEL_GRADE"] = userGrades.BulgarianGrade.Value;
                vars["БЕЛ_GRADE"] = userGrades.BulgarianGrade.Value;
            }

            if (userGrades.MathGrade.HasValue)
            {
                vars["MAT_GRADE"] = userGrades.MathGrade.Value;
                vars["МАТ_GRADE"] = userGrades.MathGrade.Value;
            }

            // 3) Допълнителни оценки
            if (userGrades.AdditionalGrades != null)
            {
                foreach (var ag in userGrades.AdditionalGrades)
                {
                    // Преобразуваме кода на латиница за съвместимост с NCalc
                    string latinCode = TransliterateSubjectCode(ag.SubjectCode);

                    // Добавяме променливи и под оригиналното им име и под латинизираната версия
                    switch (ag.ComponentType)
                    {
                        case ComponentTypes.NationalExam:
                            // НВО точки
                            vars[$"{latinCode}"] = ag.Value;
                            vars[$"{ag.SubjectCode}"] = ag.Value;
                            break;

                        case ComponentTypes.EntranceExam:
                            // Точки от приемен изпит
                            vars[$"{latinCode}_EXAM"] = ag.Value;
                            vars[$"{ag.SubjectCode}_EXAM"] = ag.Value;
                            break;

                        case ComponentTypes.YearlyGrade:
                            // Годишна оценка (2-6)
                            vars[$"{latinCode}_GRADE"] = ag.Value;
                            vars[$"{ag.SubjectCode}_GRADE"] = ag.Value;

                            // Годишна оценка превърната в точки
                            vars[$"{latinCode}_POINTS"] = ConvertGradeToPoints(ag.Value);
                            vars[$"{ag.SubjectCode}_POINTS"] = ConvertGradeToPoints(ag.Value);
                            break;

                        case ComponentTypes.YearlyGradeAsPoints:
                            // Точки от преобразувана годишна оценка
                            vars[$"{latinCode}_POINTS"] = ag.Value;
                            vars[$"{ag.SubjectCode}_POINTS"] = ag.Value;
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

        private string TransliterateSubjectCode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Проверяваме дали имаме предварително дефинирана транслитерация
            if (_transliterationMap.TryGetValue(input, out var mappedValue))
                return mappedValue;

            // Ако нямаме, правим символ по символ транслитерация
            return input
                .Replace("А", "A").Replace("Б", "B").Replace("В", "V")
                .Replace("Г", "G").Replace("Д", "D").Replace("Е", "E")
                .Replace("Ж", "J").Replace("З", "Z").Replace("И", "I")
                .Replace("Й", "Y").Replace("К", "K").Replace("Л", "L")
                .Replace("М", "M").Replace("Н", "N").Replace("О", "O")
                .Replace("П", "P").Replace("Р", "R").Replace("С", "S")
                .Replace("Т", "T").Replace("У", "U").Replace("Ф", "F")
                .Replace("Х", "H").Replace("Ц", "C").Replace("Ч", "CH")
                .Replace("Ш", "SH").Replace("Щ", "SHT").Replace("Ъ", "A")
                .Replace("Ь", "").Replace("Ю", "YU").Replace("Я", "YA");
        }
    }
}