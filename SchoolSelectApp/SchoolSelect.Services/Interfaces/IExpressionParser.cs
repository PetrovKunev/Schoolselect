// 1. Интерфейс за парсинг на математически изрази
namespace SchoolSelect.Services.Interfaces
{
    public interface IExpressionParser
    {
        /// <summary>
        /// Оценява формулата с подадените променливи.
        /// </summary>
        double Evaluate(string formula, IDictionary<string, double> variables);
    }
}