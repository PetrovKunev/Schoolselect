// SchoolRepository.cs
using Microsoft.EntityFrameworkCore;
using SchoolSelect.Common;
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
            var school = await AppContext.Schools
                .Include(s => s.Profiles)
                .Include(s => s.HistoricalRankings)
                .Include(s => s.Facilities)
                .Include(s => s.Reviews)
                    .ThenInclude(r => r.User)
                .SingleOrDefaultAsync(s => s.Id == id);

            if (school == null)
            {
                throw new InvalidOperationException($"School with ID {id} not found.");
            }

            return school;
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

        public async Task<IEnumerable<School>> GetSchoolsByProfileTypeAsync(string profileType)
        {
            // Преобразуваме string към enum
            if (Enum.TryParse<ProfileType>(profileType, out var profileTypeEnum))
            {
                // Използваме директно enum стойността, без ToString()
                return await AppContext.Schools
                    .Include(s => s.Profiles)
                    .Where(s => s.Profiles.Any(p => p.Type == profileTypeEnum))
                    .ToListAsync() ?? Enumerable.Empty<School>();
            }

            return Enumerable.Empty<School>();
        }
    }
}