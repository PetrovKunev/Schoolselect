using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Repositories
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        private ApplicationDbContext AppContext => _context as ApplicationDbContext;

        /// <summary>
        /// Връща потребител по потребителско име
        /// </summary>
        /// <param name="username">Потребителско име</param>
        /// <returns>Потребител или null ако не съществува</returns>
        public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
        {
            return await AppContext.Users
                .SingleOrDefaultAsync(u => u.UserName == username);
        }

        /// <summary>
        /// Връща потребител по имейл адрес
        /// </summary>
        /// <param name="email">Имейл адрес</param>
        /// <returns>Потребител или null ако не съществува</returns>
        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await AppContext.Users
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Проверява дали имейл адрес не се използва от друг потребител
        /// </summary>
        /// <param name="email">Имейл адрес</param>
        /// <returns>true ако имейлът не се използва, false в противен случай</returns>
        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await AppContext.Users
                .AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// Проверява дали потребителско име не се използва от друг потребител
        /// </summary>
        /// <param name="username">Потребителско име</param>
        /// <returns>true ако потребителското име не се използва, false в противен случай</returns>
        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            return !await AppContext.Users
                .AnyAsync(u => u.UserName == username);
        }
    }
}