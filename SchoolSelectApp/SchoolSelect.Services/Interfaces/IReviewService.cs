using SchoolSelect.Data.Models;

namespace SchoolSelect.Services.Interfaces
{
    public interface IReviewService
    {
        /// <summary>
        /// Връща всички одобрени отзиви за дадено училище
        /// </summary>
        Task<IEnumerable<Review>> GetReviewsBySchoolIdAsync(int schoolId);

        /// <summary>
        /// Връща всички отзиви на даден потребител
        /// </summary>
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(Guid userId);

        /// <summary>
        /// Връща всички неодобрени отзиви за модерация
        /// </summary>
        Task<IEnumerable<Review>> GetPendingReviewsAsync();

        /// <summary>
        /// Създава нов отзив
        /// </summary>
        Task<Review> CreateReviewAsync(Review review);

        /// <summary>
        /// Обновява съществуващ отзив
        /// </summary>
        Task UpdateReviewAsync(Review review);

        /// <summary>
        /// Одобрява отзив
        /// </summary>
        Task ApproveReviewAsync(int reviewId);

        /// <summary>
        /// Отхвърля отзив
        /// </summary>
        Task RejectReviewAsync(int reviewId);

        /// <summary>
        /// Изтрива отзив
        /// </summary>
        Task DeleteReviewAsync(int reviewId);

        /// <summary>
        /// Проверява дали потребител вече е оставил отзив за училище
        /// </summary>
        Task<bool> HasUserReviewedSchoolAsync(Guid userId, int schoolId);

        /// <summary>
        /// Преизчислява средната оценка на училище
        /// </summary>
        Task RecalculateSchoolRatingAsync(int schoolId);
    }
}