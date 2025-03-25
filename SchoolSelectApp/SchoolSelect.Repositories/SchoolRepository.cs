// SchoolRepository.cs
using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Repositories
{
    public class SchoolRepository : Repository<School>, ISchoolRepository
    {
        public SchoolRepository(ApplicationDbContext context) : base(context) { }

        private ApplicationDbContext AppContext => _context as ApplicationDbContext;

        public async Task<IEnumerable<School>> GetSchoolsWithProfilesAsync()
        {
            return await AppContext.Schools
                .Include(s => s.Profiles)
                .ToListAsync() ?? Enumerable.Empty<School>();
        }

        public async Task<School> GetSchoolWithDetailsAsync(int id)
        {
            return await AppContext.Schools
                .Include(s => s.Profiles)
                .Include(s => s.HistoricalRankings)
                .Include(s => s.Facilities)
                .Include(s => s.Reviews)
                    .ThenInclude(r => r.User)
                .SingleOrDefaultAsync(s => s.Id == id) ?? throw new InvalidOperationException("School not found");
        }

        public async Task<IEnumerable<School>> SearchSchoolsAsync(string term, int? take = null)
        {
            if (string.IsNullOrWhiteSpace(term))
                return await GetAllAsync();

            term = term.Trim().ToLower();

            var query = AppContext.Schools
                .Where(s => s.Name.ToLower().Contains(term) ||
                            s.City.ToLower().Contains(term) ||
                            s.District.ToLower().Contains(term));

            if (take.HasValue)
                query = query.Take(take.Value);

            return await query.ToListAsync() ?? Enumerable.Empty<School>();
        }

        public async Task<IEnumerable<School>> GetSchoolsByDistrictAsync(string district)
        {
            return await AppContext.Schools
                .Where(s => s.District == district)
                .ToListAsync() ?? Enumerable.Empty<School>();
        }

        public async Task<IEnumerable<School>> GetTopRatedSchoolsAsync(int count)
        {
            return await AppContext.Schools
                .OrderByDescending(s => s.AverageRating)
                .Take(count)
                .ToListAsync() ?? Enumerable.Empty<School>();
        }

        public async Task<double> CalculateAverageRatingAsync(int schoolId)
        {
            var reviews = await AppContext.Reviews
                .Where(r => r.SchoolId == schoolId && r.IsApproved)
                .ToListAsync();

            if (!reviews.Any())
                return 0;

            return reviews.Average(r => r.Rating);
        }
    }
}