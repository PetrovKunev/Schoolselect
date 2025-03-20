using SchoolSelect.Data.Models;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        /// <summary>
        /// Връща потребител по потребителско име
        /// </summary>
        /// <param name="username">Потребителско име</param>
        /// <returns>Потребител или null ако не съществува</returns>
        Task<ApplicationUser?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Връща потребител по имейл адрес
        /// </summary>
        /// <param name="email">Имейл адрес</param>
        /// <returns>Потребител или null ако не съществува</returns>
        Task<ApplicationUser?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Проверява дали имейл адрес не се използва от друг потребител
        /// </summary>
        /// <param name="email">Имейл адрес</param>
        /// <returns>true ако имейлът не се използва, false в противен случай</returns>
        Task<bool> IsEmailUniqueAsync(string email);

        /// <summary>
        /// Проверява дали потребителско име не се използва от друг потребител
        /// </summary>
        /// <param name="username">Потребителско име</param>
        /// <returns>true ако потребителското име не се използва, false в противен случай</returns>
        Task<bool> IsUsernameUniqueAsync(string username);
    }
}