using Microsoft.Extensions.Options;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Data.Models;
using SchoolSelect.Services.Configurations;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using SchoolSelect.Repositories.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    /// <summary>
    /// Изчислява шанса за прием по бал, с подобрена логика за по-реалистични резултати
    /// </summary>
    public class DefaultChanceCalculator : IChanceCalculator
    {
        private readonly ChanceCalculatorOptions _opts;
        private readonly ILogger<DefaultChanceCalculator> _logger;
        private readonly IHistoricalRankingRepository? _rankingRepository;

        public DefaultChanceCalculator(
            IOptions<ChanceCalculatorOptions> options,
            ILogger<DefaultChanceCalculator> logger,
            IHistoricalRankingRepository? rankingRepository = null) // Optional for backward compatibility
        {
            _opts = options.Value;
            _logger = logger;
            _rankingRepository = rankingRepository;
        }

        public double Calculate(double score, double historicalMin, int profileId = 0, int schoolId = 0)
        {
            _logger.LogDebug("Изчисляване на шанс: бал {Score}, исторически минимум {HistoricalMin}",
                score, historicalMin);

            if (historicalMin <= 0)
            {
                _logger.LogWarning("Липсва исторически минимум, връщаме стандартен шанс {DefaultChance}%",
                    _opts.DefaultChance);
                return _opts.DefaultChance;
            }

            // Основна разлика между баловете (абсолютна)
            double absoluteDifference = score - historicalMin;

            // Относителна разлика като процент от минималния бал
            double relativeDifference = absoluteDifference / historicalMin * 100;

            _logger.LogDebug("Абсолютна разлика: {AbsoluteDifference}, Относителна разлика: {RelativeDifference}%",
                absoluteDifference, relativeDifference);

            // Приложи корекция на базата на исторически тенденции, ако е възможно
            double trendFactor = 1.0;
            if (_rankingRepository != null && profileId > 0)
            {
                trendFactor = CalculateTrendFactor(profileId, schoolId).Result;
                _logger.LogDebug("Фактор на тенденцията: {TrendFactor}", trendFactor);
            }

            // Изчисляване на крайния шанс
            double chance = CalculateChanceWithThresholds(absoluteDifference, relativeDifference, trendFactor);

            _logger.LogInformation("Краен изчислен шанс: {Chance}%", chance);
            return chance;
        }

        /// <summary>
        /// Изчислява крайния шанс на базата на различни прагове и фактори
        /// </summary>
        private double CalculateChanceWithThresholds(double absoluteDifference, double relativeDifference, double trendFactor)
        {
            // Увеличени прагове за по-реалистични шансове
            double extremeHighThreshold = _opts.HighThreshold * 5; // Например 100 вместо 20
            double veryHighThreshold = _opts.HighThreshold * 2.5; // Например 50 вместо 20 
            double highThreshold = _opts.HighThreshold; // Оригиналния праг (например 20)

            // Отчитане на относителната разлика
            bool isRelativelyHigh = relativeDifference >= 30; // 30% над минималния бал
            bool isRelativelyVeryHigh = relativeDifference >= 50; // 50% над минималния бал

            // Определяне на базовия шанс според абсолютната разлика
            double baseChance;

            if (absoluteDifference >= extremeHighThreshold || isRelativelyVeryHigh)
            {
                // Екстремно висок шанс при много голяма разлика
                baseChance = _opts.VeryHighChance;
                _logger.LogDebug("Екстремно висок шанс: {Chance}%", baseChance);
            }
            else if (absoluteDifference >= veryHighThreshold || isRelativelyHigh)
            {
                // Много висок шанс при значителна разлика
                double ratio = (absoluteDifference - veryHighThreshold) / (extremeHighThreshold - veryHighThreshold);
                ratio = Math.Max(0, Math.Min(1, ratio)); // Ограничаване в интервала [0,1]

                baseChance = 90 + (ratio * (_opts.VeryHighChance - 90));
                _logger.LogDebug("Много висок шанс: {Chance}%", baseChance);
            }
            else if (absoluteDifference >= highThreshold)
            {
                // Висок шанс при добра разлика
                double ratio = (absoluteDifference - highThreshold) / (veryHighThreshold - highThreshold);
                ratio = Math.Max(0, Math.Min(1, ratio)); // Ограничаване в интервала [0,1]

                baseChance = 80 + (ratio * (90 - 80));
                _logger.LogDebug("Висок шанс: {Chance}%", baseChance);
            }
            else if (absoluteDifference >= 0)
            {
                // Среден към висок шанс при положителна разлика
                double ratio = absoluteDifference / highThreshold;
                ratio = Math.Max(0, Math.Min(1, ratio)); // Ограничаване в интервала [0,1]

                baseChance = _opts.HighMinChance + (ratio * (80 - _opts.HighMinChance));
                _logger.LogDebug("Среден към висок шанс: {Chance}%", baseChance);
            }
            else if (absoluteDifference >= -_opts.LowThreshold)
            {
                // Нисък към среден шанс при малка отрицателна разлика
                double ratio = (absoluteDifference + _opts.LowThreshold) / _opts.LowThreshold;
                ratio = Math.Max(0, Math.Min(1, ratio)); // Ограничаване в интервала [0,1]

                baseChance = _opts.LowMinChance + (ratio * (_opts.LowMaxChance - _opts.LowMinChance));
                _logger.LogDebug("Нисък към среден шанс: {Chance}%", baseChance);
            }
            else
            {
                // Много нисък шанс при голяма отрицателна разлика
                baseChance = _opts.VeryLowChance;
                _logger.LogDebug("Много нисък шанс: {Chance}%", baseChance);
            }

            // Приложи фактора на тенденцията (увеличи шанса при намаляващ тренд, намали при растящ)
            double adjustedChance = baseChance * trendFactor;

            // Ограничаване на крайния резултат в интервала [0, 99.9]
            adjustedChance = Math.Max(0, Math.Min(99.9, adjustedChance));

            return Math.Round(adjustedChance, 1); // Закръгляне до 1 знак след десетичната точка
        }

        /// <summary>
        /// Изчислява фактор на базата на историческа тенденция на минималния бал
        /// </summary>
        private async Task<double> CalculateTrendFactor(int profileId, int schoolId)
        {
            try
            {
                // По подразбиране, ако няма достатъчно данни
                double defaultFactor = 1.0;

                // Само ако имаме достъп до историческите данни
                if (_rankingRepository is null)
                {
                    return defaultFactor;
                }

                // Взимаме данните за последните 3 години, сортирани по година
                var rankings = (await _rankingRepository.GetRankingsByProfileIdAsync(profileId))
                    .Where(r => r.ProfileId == profileId || (profileId == 0 && r.SchoolId == schoolId))
                    .OrderByDescending(r => r.Year)
                    .ThenBy(r => r.Round)
                    .ToList();

                // Ако няма достатъчно данни, връщаме неутрален фактор
                if (rankings.Count <= 1)
                {
                    _logger.LogInformation("Недостатъчно исторически данни за анализ на тенденция");
                    return defaultFactor;
                }

                // Изчисляваме средногодишната промяна
                List<double> yearlyChanges = new List<double>();

                for (int i = 0; i < rankings.Count - 1; i++)
                {
                    if (rankings[i].Year == rankings[i + 1].Year) continue;

                    int yearDifference = rankings[i].Year - rankings[i + 1].Year;
                    if (yearDifference <= 0) continue;

                    double scoreChange = rankings[i].MinimumScore - rankings[i + 1].MinimumScore;
                    double averageYearlyChange = scoreChange / yearDifference;

                    yearlyChanges.Add(averageYearlyChange);
                }

                if (yearlyChanges.Count == 0)
                {
                    return defaultFactor;
                }

                // Средна промяна на бала
                double averageChange = yearlyChanges.Average();

                _logger.LogDebug("Средногодишна промяна на минималния бал: {AverageChange}", averageChange);

                // Изчисляваме фактор на базата на тенденцията:
                // - При растящ тренд (положителна промяна) намаляваме шанса (фактор < 1)
                // - При намаляващ тренд (отрицателна промяна) увеличаваме шанса (фактор > 1)

                // Мащабираме с параметър, за да не е прекалено чувствителен
                double sensitivity = 0.02; // Може да се настрои
                double trendFactor = 1.0 - (averageChange * sensitivity);

                // Ограничаваме фактора в разумни граници
                trendFactor = Math.Max(0.8, Math.Min(1.2, trendFactor));

                return trendFactor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при изчисляване на исторически тенденции");
                return 1.0; // Неутрален фактор при грешка
            }
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

        /// <summary>
        /// Основният метод Calculate с обратна съвместимост
        /// </summary>
        public double Calculate(double score, double historicalMin)
        {
            return Calculate(score, historicalMin, 0, 0);
        }
    }
}