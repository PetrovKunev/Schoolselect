using Microsoft.Extensions.Options;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Data.Models;
using SchoolSelect.Services.Configurations;
using Microsoft.Extensions.Logging;

namespace SchoolSelect.Services.Implementations
{
    /// <summary>
    /// Изчислява шанса за прием по бал
    /// </summary>
    public class DefaultChanceCalculator : IChanceCalculator
    {
        private readonly ChanceCalculatorOptions _opts;
        private readonly ILogger<DefaultChanceCalculator> _logger;

        public DefaultChanceCalculator(
            IOptions<ChanceCalculatorOptions> options,
            ILogger<DefaultChanceCalculator> logger)
        {
            _opts = options.Value;
            _logger = logger;
        }

        public double Calculate(double score, double historicalMin)
        {
            _logger.LogDebug("Изчисляване на шанс: бал {Score}, исторически минимум {HistoricalMin}",
                score, historicalMin);

            if (historicalMin <= 0)
            {
                _logger.LogWarning("Липсва исторически минимум, връщаме стандартен шанс {DefaultChance}%",
                    _opts.DefaultChance);
                return _opts.DefaultChance;
            }

            // Разлика между текущия бал и историческия минимум
            double difference = score - historicalMin;
            _logger.LogDebug("Разлика от минималния бал: {Difference}", difference);

            if (difference >= _opts.HighThreshold)
            {
                _logger.LogDebug("Много висок шанс (>= {Threshold}): {Chance}%",
                    _opts.HighThreshold, _opts.VeryHighChance);
                return _opts.VeryHighChance;
            }

            if (difference >= 0)
            {
                // Линейна интерполация в диапазона [HighMinChance, HighMaxChance]
                var ratio = difference / _opts.HighThreshold;
                var chance = _opts.HighMinChance + (ratio * (_opts.HighMaxChance - _opts.HighMinChance));

                _logger.LogDebug("Висок шанс (0-{Threshold}): {Chance}%", _opts.HighThreshold, chance);
                return chance;
            }

            if (difference >= -_opts.LowThreshold)
            {
                // Линейна интерполация в диапазона [LowMinChance, LowMaxChance]
                var ratio = (difference + _opts.LowThreshold) / _opts.LowThreshold;
                var chance = _opts.LowMinChance + (ratio * (_opts.LowMaxChance - _opts.LowMinChance));

                _logger.LogDebug("Нисък шанс (-{Threshold}-0): {Chance}%", _opts.LowThreshold, chance);
                return chance;
            }

            _logger.LogDebug("Много нисък шанс (< -{Threshold}): {Chance}%",
                _opts.LowThreshold, _opts.VeryLowChance);
            return _opts.VeryLowChance;
        }

        public double FallbackScore(UserGrades userGrades)
        {
            _logger.LogWarning("Използване на резервно изчисление на бал");

            // Опростен бал: двоен Бел + двоен Мат + точки от НВО
            var score = ((userGrades.BulgarianGrade ?? 0) * 2 * 20) +
                   ((userGrades.MathGrade ?? 0) * 2 * 20) +
                   userGrades.BulgarianExamPoints +
                   userGrades.MathExamPoints;

            _logger.LogInformation("Резервен бал: {Score}", score);
            return score;
        }
    }
}