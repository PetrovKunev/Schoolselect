// IHistoricalRankingRepository.cs
using SchoolSelect.Data.Models;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface IHistoricalRankingRepository : IRepository<HistoricalRanking>
    {
        Task<IEnumerable<HistoricalRanking>> GetRankingsBySchoolIdAsync(int schoolId);
        Task<IEnumerable<HistoricalRanking>> GetRankingsByProfileIdAsync(int profileId);
        Task<IEnumerable<HistoricalRanking>> GetRankingsByYearAsync(int year);
        Task<double> GetAverageMinimumScoreAsync(int schoolId, int profileId = 0, int years = 3);
        /// <summary>
        /// Връща списък с исторически класирания по множество профилни IDs
        /// </summary>
        Task<IEnumerable<HistoricalRanking>> GetRankingsByProfileIdsAsync(IEnumerable<int> profileIds);
    }
}
