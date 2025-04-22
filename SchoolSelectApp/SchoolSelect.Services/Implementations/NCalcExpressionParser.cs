using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    // Имплементация на IExpressionParser с NCalc
    public class NCalcExpressionParser : IExpressionParser
    {
        private readonly ConcurrentDictionary<string, NCalc.Expression> _cache = new();

        public double Evaluate(string formula, IDictionary<string, double> variables)
        {
            // Преобразуване на формулата, за да замени кирилските символи със съответните им латински аналози
            var transformedFormula = TransformAdmissionFormula(formula);

            var expr = _cache.GetOrAdd(transformedFormula, f => new NCalc.Expression(f));

            // Добавяне на всички променливи към израза
            foreach (var kvp in variables)
                expr.Parameters[kvp.Key] = kvp.Value;

            try
            {
                var result = expr.Evaluate();
                return Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                // Log подробна грешка с оригиналната и трансформираната формула
                throw new InvalidOperationException(
                    $"Грешка при изчисляване на формула. Оригинал: '{formula}', Трансформирана: '{transformedFormula}'", ex);
            }
        }

        // Метод за трансформиране на формулата - заменя кирилски идентификатори с латински
        private string TransformAdmissionFormula(string formula)
        {
            // Премахване на невалидни символи като '?'
            formula = Regex.Replace(formula, @"[?]", "М");

            // Проверяваме дали формулата е от подобен вид (с две части, разделени с +)
            var parts = formula.Split(new string[] { ") + (" }, StringSplitOptions.None);

            if (parts.Length == 2)
            {
                // Първа част - НВО точки
                string firstPart = parts[0]
                    .Replace("БЕЛ", "BEL")
                    .Replace("МАТ", "MAT");

                // Втора част - годишни оценки, преобразувани в точки
                string secondPart = parts[1]
                    .Replace("БЕЛ", "BEL_POINTS")
                    .Replace("МАТ", "MAT_POINTS")
                    .Replace("М", "MAT_POINTS")  // Важно: М е съкращение на МАТ
                    .Replace("КМИТ", "KMIT_POINTS")
                    .Replace("ЧЕ", "CHOZ_POINTS")
                    .Replace("ГИ", "GI_POINTS")
                    .Replace("БЗО", "BZO_POINTS")
                    .Replace("ХООС", "HOOS_POINTS")
                    .Replace("ЧЕз", "CHOZ_POINTS");

                return firstPart + ") + (" + secondPart;
            }

            // Ако формулата е с различна структура, само транслитерираме кирилицата
            return formula
                .Replace("БЕЛ", "BEL")
                .Replace("МАТ", "MAT")
                .Replace("М", "MAT")  // Добавено: М е съкращение на МАТ
                .Replace("КМИТ", "KMIT")
                .Replace("ЧЕ", "CHOZ")
                .Replace("ГИ", "GI")
                .Replace("БЗО", "BZO")
                .Replace("ХООС", "HOOS")
                .Replace("ЧЕз", "CHOZ");
        }
    }
}