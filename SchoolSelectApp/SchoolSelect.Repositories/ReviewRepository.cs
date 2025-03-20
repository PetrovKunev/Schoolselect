using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context) { }

        private ApplicationDbContext AppContext => _context as ApplicationDbContext;

        /// <summary>
        /// Връща всички одобрени отзиви за дадено училище
        /// </summary>
        /// <param name="schoolId">ID на училището</param>
        /// <returns>Списък с отзиви</returns>
        public async Task<IEnumerable<Review>> GetReviewsBySchoolIdAsync(int schoolId)
        {
            return await AppContext.Reviews
                .Where(r => r.SchoolId == schoolId && r.IsApproved)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Връща всички отзиви на даден потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Списък с отзиви</returns>
        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(Guid userId)
        {
            return await AppContext.Reviews
                .Where(r => r.UserId == userId)
                .Include(r => r.School)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Връща всички неодобрени отзиви
        /// </summary>
        /// <returns>Списък с чакащи отзиви</returns>
        public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
        {
            return await AppContext.Reviews
                .Where(r => !r.IsApproved)
                .Include(r => r.School)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}