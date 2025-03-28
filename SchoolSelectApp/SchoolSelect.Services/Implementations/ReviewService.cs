// ReviewService.cs
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public ReviewService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Review>> GetReviewsBySchoolIdAsync(int schoolId)
        {
            return await _unitOfWork.Reviews.GetReviewsBySchoolIdAsync(schoolId);
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(Guid userId)
        {
            return await _unitOfWork.Reviews.GetReviewsByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
        {
            return await _unitOfWork.Reviews.GetPendingReviewsAsync();
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            // Проверка дали потребителят вече има отзив за това училище
            if (await HasUserReviewedSchoolAsync(review.UserId, review.SchoolId))
            {
                throw new InvalidOperationException("Вече сте публикували отзив за това училище.");
            }

            // Всеки нов отзив е неодобрен първоначално
            review.IsApproved = false;
            review.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.CompleteAsync();

            // Известяваме администраторите за нов отзив, който чака одобрение
            await NotifyAdminsForPendingReviewAsync(review);

            return review;
        }

        public async Task UpdateReviewAsync(Review review)
        {
            var existingReview = await _unitOfWork.Reviews.GetByIdAsync(review.Id);

            // При редактиране на отзива, той отново трябва да мине през модерация
            existingReview.Content = review.Content;
            existingReview.Rating = review.Rating;
            existingReview.IsApproved = false;

            _unitOfWork.Reviews.Update(existingReview);
            await _unitOfWork.CompleteAsync();

            // Известяваме администраторите за редактиран отзив, който чака одобрение
            await NotifyAdminsForPendingReviewAsync(existingReview);
        }

        public async Task ApproveReviewAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

            review.IsApproved = true;
            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.CompleteAsync();

            // Преизчисляваме рейтинга на училището
            await RecalculateSchoolRatingAsync(review.SchoolId);

            // Известяваме потребителя, че отзивът му е одобрен
            await NotifyUserForApprovedReviewAsync(review);
        }

        public async Task RejectReviewAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

            // При отхвърляне просто изтриваме отзива
            _unitOfWork.Reviews.Remove(review);
            await _unitOfWork.CompleteAsync();

            // Известяваме потребителя, че отзивът му е отхвърлен
            await NotifyUserForRejectedReviewAsync(review);
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            int schoolId = review.SchoolId;

            _unitOfWork.Reviews.Remove(review);
            await _unitOfWork.CompleteAsync();

            // Преизчисляваме рейтинга на училището след изтриване на отзив
            await RecalculateSchoolRatingAsync(schoolId);
        }

        public async Task<bool> HasUserReviewedSchoolAsync(Guid userId, int schoolId)
        {
            var userReviews = await _unitOfWork.Reviews.GetReviewsByUserIdAsync(userId);
            return userReviews.Any(r => r.SchoolId == schoolId);
        }

        public async Task RecalculateSchoolRatingAsync(int schoolId)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            double averageRating = await _unitOfWork.Schools.CalculateAverageRatingAsync(schoolId);

            // Актуализиране на средния рейтинг на училището
            school.AverageRating = averageRating;

            // Получаване на броя на одобрените отзиви
            var approvedReviews = await _unitOfWork.Reviews.FindAsync(r => r.SchoolId == schoolId && r.IsApproved);
            school.RatingsCount = approvedReviews.Count();

            _unitOfWork.Schools.Update(school);
            await _unitOfWork.CompleteAsync();
        }

        // Помощни методи за изпращане на известия
        private async Task NotifyAdminsForPendingReviewAsync(Review review)
        {
            // Тук ще изпращаме известия на администраторите за нов отзив, който чака одобрение
            // Това ще бъде имплементирано по-късно с NotificationService
            var school = await _unitOfWork.Schools.GetByIdAsync(review.SchoolId);

            // Примерна имплементация, трябва да се доразвие
            // В реалността, тук ще трябва да идентифицираме потребителите с роля "Admin" или "Moderator"
            // и да изпратим известие на всеки от тях
        }

        private async Task NotifyUserForApprovedReviewAsync(Review review)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(review.SchoolId);

            // Създаваме известие за потребителя
            var notification = new Notification
            {
                UserId = review.UserId,
                Title = "Вашият отзив е одобрен",
                Content = $"Вашият отзив за {school.Name} беше одобрен и вече е публикуван.",
                NotificationType = 3, // Системно съобщение
                ReferenceId = review.SchoolId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CompleteAsync();
        }

        private async Task NotifyUserForRejectedReviewAsync(Review review)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(review.SchoolId);

            // Създаваме известие за потребителя
            var notification = new Notification
            {
                UserId = review.UserId,
                Title = "Вашият отзив не беше одобрен",
                Content = $"За съжаление, вашият отзив за {school.Name} не беше одобрен. Моля, прегледайте нашите правила за публикуване на отзиви и опитайте отново.",
                NotificationType = 3, // Системно съобщение
                ReferenceId = review.SchoolId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CompleteAsync();
        }
    }
}