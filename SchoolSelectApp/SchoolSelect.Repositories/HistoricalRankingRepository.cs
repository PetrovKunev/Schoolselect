using System.Linq;
using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Repositories
{
    // HistoricalRankingRepository.cs
    public class HistoricalRankingRepository : Repository<HistoricalRanking>, IHistoricalRankingRepository
    {
        public HistoricalRankingRepository(ApplicationDbContext context) : base(context) { }

        private ApplicationDbContext AppContext => _context as ApplicationDbContext;

        public async Task<IEnumerable<HistoricalRanking>> GetRankingsBySchoolIdAsync(int schoolId)
        {
            return await AppContext.HistoricalRankings
                .Where(r => r.SchoolId == schoolId)
                .OrderByDescending(r => r.Year)
                .ThenBy(r => r.Round)
                .ToListAsync();
        }

        public async Task<IEnumerable<HistoricalRanking>> GetRankingsByProfileIdAsync(int profileId)
        {
            return await AppContext.HistoricalRankings
                .Where(r => r.ProfileId == profileId)
                .OrderByDescending(r => r.Year)
                .ThenBy(r => r.Round)
                .ToListAsync();
        }

        public async Task<IEnumerable<HistoricalRanking>> GetRankingsByYearAsync(int year)
        {
            return await AppContext.HistoricalRankings
                .Where(r => r.Year == year)
                .OrderBy(r => r.Round)
                .ToListAsync();
        }

        public async Task<double> GetAverageMinimumScoreAsync(int schoolId, int profileId = 0, int years = 3)
        {
            var currentYear = DateTime.UtcNow.Year;

            var query = AppContext.HistoricalRankings
                .Where(r => r.SchoolId == schoolId)
                .Where(r => r.Year >= currentYear - years);

            if (profileId > 0)
            {
                query = query.Where(r => r.ProfileId == profileId);
            }

            var rankings = await query.ToListAsync();

            if (!rankings.Any())
                return 0;

            return rankings.Average(r => r.MinimumScore);
        }

        /// <summary>
        /// Връща всички исторически класирания за даден набор от профили
        /// </summary>
        public async Task<IEnumerable<HistoricalRanking>> GetRankingsByProfileIdsAsync(IEnumerable<int> profileIds)
        {
            return await AppContext.HistoricalRankings
                .Where(r => profileIds.Contains(r.ProfileId ?? 0))
                .ToListAsync();
        }
    }
}
