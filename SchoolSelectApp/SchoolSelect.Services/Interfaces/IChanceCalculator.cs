using SchoolSelect.Data.Models;

namespace SchoolSelect.Services.Interfaces
{
    // 3. Интерфейс за изчисляване на шансове
    public interface IChanceCalculator
    {
        /// <summary>
        /// Изчислява процент шанс въз основа на калкулиран бал и исторически минимален бал.
        /// </summary>
        double Calculate(double score, double historicalMin);

        /// <summary>
        /// Опционално: резервен метод за калкулиране на бал, ако няма формула.
        /// </summary>
        double FallbackScore(UserGrades userGrades);
    }
}
