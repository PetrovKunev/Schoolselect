using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Repositories
{
    public class SchoolProfileRepository : Repository<SchoolProfile>, ISchoolProfileRepository
    {
        public SchoolProfileRepository(ApplicationDbContext context) : base(context) { }

        private ApplicationDbContext AppContext => _context as ApplicationDbContext;

        /// <summary>
        /// Връща всички профили за дадено училище
        /// </summary>
        /// <param name="schoolId">ID на училището</param>
        /// <returns>Списък с профили</returns>
        public async Task<IEnumerable<SchoolProfile>> GetProfilesBySchoolIdAsync(int schoolId)
        {
            return await AppContext.SchoolProfiles
                .Where(p => p.SchoolId == schoolId)
                .ToListAsync();
        }

        /// <summary>
        /// Връща профил заедно с историческите класирания
        /// </summary>
        /// <param name="profileId">ID на профила</param>
        /// <returns>Профил с историческите класирания или null ако не съществува</returns>
        public async Task<SchoolProfile?> GetProfileWithHistoricalRankingsAsync(int profileId)
        {
            return await AppContext.SchoolProfiles
                .Include(p => p.Rankings.OrderByDescending(r => r.Year))
                .SingleOrDefaultAsync(p => p.Id == profileId);
        }

        /// <summary>
        /// Връща профил заедно с формулите за балообразуване
        /// </summary>
        /// <param name="profileId">ID на профила</param>
        /// <returns>Профил с формулите за балообразуване или null ако не съществува</returns>
        public async Task<SchoolProfile?> GetProfileWithAdmissionFormulasAsync(int profileId)
        {
            return await AppContext.SchoolProfiles
                .Include(p => p.AdmissionFormulas)
                    .ThenInclude(f => f.Components)
                .SingleOrDefaultAsync(p => p.Id == profileId);
        }
    }
}