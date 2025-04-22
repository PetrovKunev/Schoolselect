using Microsoft.Extensions.Options;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Data.Models;
using SchoolSelect.Services.Configurations;

namespace SchoolSelect.Services.Implementations
{
    // Имплементация на IChanceCalculator
    public class DefaultChanceCalculator : IChanceCalculator
    {
        private readonly ChanceCalculatorOptions _opts;

        public DefaultChanceCalculator(IOptions<ChanceCalculatorOptions> options)
        {
            _opts = options.Value;
        }

        public double Calculate(double score, double historicalMin)
        {
            if (historicalMin <= 0)
                return _opts.DefaultChance;

            if (score >= historicalMin + _opts.HighThreshold)
                return _opts.VeryHighChance;

            if (score >= historicalMin)
            {
                var ratio = (score - historicalMin) / _opts.HighThreshold;
                return _opts.HighMinChance + (ratio * (_opts.HighMaxChance - _opts.HighMinChance));
            }

            if (score >= historicalMin - _opts.LowThreshold)
            {
                var ratio = (score - (historicalMin - _opts.LowThreshold)) / _opts.LowThreshold;
                return _opts.LowMinChance + (ratio * (_opts.LowMaxChance - _opts.LowMinChance));
            }

            return _opts.VeryLowChance;
        }

        public double FallbackScore(UserGrades userGrades)
        {
            // TODO: Опростен бал: двоен Бел + двоен М + точки от НВО
            return ((userGrades.BulgarianGrade ?? 0) * 2) +
                   ((userGrades.MathGrade ?? 0) * 2) +
                   userGrades.BulgarianExamPoints +
                   userGrades.MathExamPoints;
        }
    }
}
