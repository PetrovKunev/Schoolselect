using SchoolSelect.Data.Models;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        /// <summary>
        /// Връща всички одобрени отзиви за дадено училище
        /// </summary>
        /// <param name="schoolId">ID на училището</param>
        /// <returns>Списък с отзиви</returns>
        Task<IEnumerable<Review>> GetReviewsBySchoolIdAsync(int schoolId);

        /// <summary>
        /// Връща всички отзиви на даден потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Списък с отзиви</returns>
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(Guid userId);

        /// <summary>
        /// Връща всички неодобрени отзиви
        /// </summary>
        /// <returns>Списък с чакащи отзиви</returns>
        Task<IEnumerable<Review>> GetPendingReviewsAsync();
    }
}