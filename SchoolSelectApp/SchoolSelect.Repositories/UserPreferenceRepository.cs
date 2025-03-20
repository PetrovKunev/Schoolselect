using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Repositories
{
    public class UserPreferenceRepository : Repository<UserPreference>, IUserPreferenceRepository
    {
        public UserPreferenceRepository(ApplicationDbContext context) : base(context) { }

        private ApplicationDbContext AppContext => _context as ApplicationDbContext;

        /// <summary>
        /// Връща всички запазени предпочитания на потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Списък с предпочитания</returns>
        public async Task<IEnumerable<UserPreference>> GetPreferencesByUserIdAsync(Guid userId)
        {
            return await AppContext.UserPreferences
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Връща предпочитание с конкретно име на потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <param name="preferenceName">Име на предпочитанието</param>
        /// <returns>Предпочитание или null ако не съществува</returns>
        public async Task<UserPreference?> GetPreferenceByNameAsync(Guid userId, string preferenceName)
        {
            return await AppContext.UserPreferences
                .SingleOrDefaultAsync(p => p.UserId == userId && p.PreferenceName == preferenceName);
        }
    }
}