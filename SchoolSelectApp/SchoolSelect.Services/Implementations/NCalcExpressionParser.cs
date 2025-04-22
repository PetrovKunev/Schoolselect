using System.Collections.Concurrent;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    // Имплементация на IExpressionParser с NCalc
    public class NCalcExpressionParser : IExpressionParser
    {
        private readonly ConcurrentDictionary<string, NCalc.Expression> _cache = new();

        public double Evaluate(string formula, IDictionary<string, double> variables)
        {
            var expr = _cache.GetOrAdd(formula, f => new NCalc.Expression(f));
            foreach (var kvp in variables)
                expr.Parameters[kvp.Key] = kvp.Value;

            var result = expr.Evaluate();
            return Convert.ToDouble(result);
        }
    }
}
