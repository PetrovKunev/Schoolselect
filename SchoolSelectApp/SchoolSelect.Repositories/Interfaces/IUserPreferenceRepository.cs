using SchoolSelect.Data.Models;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface IUserPreferenceRepository : IRepository<UserPreference>
    {
        /// <summary>
        /// Връща всички запазени предпочитания на потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Списък с предпочитания</returns>
        Task<IEnumerable<UserPreference>> GetPreferencesByUserIdAsync(Guid userId);

        /// <summary>
        /// Връща предпочитание с конкретно име на потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <param name="preferenceName">Име на предпочитанието</param>
        /// <returns>Предпочитание или null ако не съществува</returns>
        Task<UserPreference?> GetPreferenceByNameAsync(Guid userId, string preferenceName);
    }
}