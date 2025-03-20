using SchoolSelect.Data.Models;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        /// <summary>
        /// Връща всички известия на даден потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Списък с известия</returns>
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(Guid userId);

        /// <summary>
        /// Връща непрочетените известия на даден потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Списък с непрочетени известия</returns>
        Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(Guid userId);

        /// <summary>
        /// Маркира известие като прочетено
        /// </summary>
        /// <param name="notificationId">ID на известието</param>
        Task MarkAsReadAsync(int notificationId);

        /// <summary>
        /// Маркира всички известия на даден потребител като прочетени
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        Task MarkAllAsReadAsync(Guid userId);
    }
}