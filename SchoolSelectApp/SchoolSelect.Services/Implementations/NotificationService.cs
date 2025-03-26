using Microsoft.Extensions.Logging;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Notification> CreateNotificationAsync(Guid userId, string title, string content, int notificationType, int? referenceId = null)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Content = content,
                    NotificationType = notificationType,
                    ReferenceId = referenceId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> SendNotificationToAllAsync(string title, string content, int notificationType)
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllAsync();
                var notifications = new List<Notification>();

                foreach (var user in users)
                {
                    notifications.Add(new Notification
                    {
                        UserId = user.Id,
                        Title = title,
                        Content = content,
                        NotificationType = notificationType,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });
                }

                await _unitOfWork.Notifications.AddRangeAsync(notifications);
                await _unitOfWork.CompleteAsync();

                return notifications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to all users");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> SendNotificationToUsersAsync(IEnumerable<Guid> userIds, string title, string content, int notificationType)
        {
            try
            {
                var notifications = new List<Notification>();

                foreach (var userId in userIds)
                {
                    notifications.Add(new Notification
                    {
                        UserId = userId,
                        Title = title,
                        Content = content,
                        NotificationType = notificationType,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });
                }

                await _unitOfWork.Notifications.AddRangeAsync(notifications);
                await _unitOfWork.CompleteAsync();

                return notifications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to specific users");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> SendNotificationByPreferenceAsync(Func<UserPreference, bool> preferenceFilter, string title, string content, int notificationType)
        {
            try
            {
                // Получаваме всички предпочитания
                var allPreferences = await _unitOfWork.UserPreferences.GetAllAsync();

                // Филтрираме ги според предоставения филтър
                var filteredPreferences = allPreferences.Where(preferenceFilter);

                // Извличаме уникалните потребителски ID-та
                var userIds = filteredPreferences.Select(p => p.UserId).Distinct();

                // Създаваме известия за тези потребители
                return await SendNotificationToUsersAsync(userIds, title, content, notificationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification by preference");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(Guid userId)
        {
            try
            {
                return await _unitOfWork.Notifications.GetNotificationsByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(Guid userId)
        {
            try
            {
                return await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> MarkAsReadAsync(int notificationId, Guid userId)
        {
            try
            {
                // Първо проверяваме дали известието принадлежи на този потребител
                var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);

                if (notification.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to mark notification {NotificationId} owned by another user", userId, notificationId);
                    return false;
                }

                await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            try
            {
                await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);

                // Връщаме броя на маркираните известия
                var unreadNotificationsCount = (await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(userId)).Count();
                return unreadNotificationsCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
        {
            try
            {
                var unreadNotifications = await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(userId);
                return unreadNotifications.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Notification?> GetByIdAsync(int notificationId)
        {
            try
            {
                return await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification with ID {NotificationId}", notificationId);
                return null;
            }
        }
    }
}