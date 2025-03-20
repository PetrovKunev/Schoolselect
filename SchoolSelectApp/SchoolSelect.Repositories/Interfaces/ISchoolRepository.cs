// ISchoolRepository.cs
using SchoolSelect.Data.Models;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface ISchoolRepository : IRepository<School>
    {
        Task<IEnumerable<School>> GetSchoolsWithProfilesAsync();
        Task<School> GetSchoolWithDetailsAsync(int id);
        Task<IEnumerable<School>> SearchSchoolsAsync(string term);
        Task<IEnumerable<School>> GetSchoolsByDistrictAsync(string district);
        Task<IEnumerable<School>> GetTopRatedSchoolsAsync(int count);
        Task<double> CalculateAverageRatingAsync(int schoolId);
    }
}