using SchoolSelect.Data.Models;

namespace SchoolSelect.Services.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// Създава и запазва ново известие за потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <param name="title">Заглавие на известието</param>
        /// <param name="content">Съдържание на известието</param>
        /// <param name="notificationType">Тип известие: 1 = Промяна в училище, 2 = Ново класиране, 3 = Системно съобщение</param>
        /// <param name="referenceId">ID на референциран обект (по избор)</param>
        /// <returns>Създаденото известие</returns>
        Task<Notification> CreateNotificationAsync(Guid userId, string title, string content, int notificationType, int? referenceId = null);

        /// <summary>
        /// Изпраща известие до всички потребители
        /// </summary>
        /// <param name="title">Заглавие на известието</param>
        /// <param name="content">Съдържание на известието</param>
        /// <param name="notificationType">Тип известие</param>
        /// <returns>Брой създадени известия</returns>
        Task<int> SendNotificationToAllAsync(string title, string content, int notificationType);

        /// <summary>
        /// Изпраща известие до списък от потребители
        /// </summary>
        /// <param name="userIds">Списък с ID-та на потребители</param>
        /// <param name="title">Заглавие на известието</param>
        /// <param name="content">Съдържание на известието</param>
        /// <param name="notificationType">Тип известие</param>
        /// <returns>Брой създадени известия</returns>
        Task<int> SendNotificationToUsersAsync(IEnumerable<Guid> userIds, string title, string content, int notificationType);

        /// <summary>
        /// Изпраща известие до потребители с определени предпочитания
        /// </summary>
        /// <param name="preferenceFilter">Функция за филтриране на потребители по предпочитания</param>
        /// <param name="title">Заглавие на известието</param>
        /// <param name="content">Съдържание на известието</param>
        /// <param name="notificationType">Тип известие</param>
        /// <returns>Брой създадени известия</returns>
        Task<int> SendNotificationByPreferenceAsync(Func<UserPreference, bool> preferenceFilter, string title, string content, int notificationType);

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
        /// <param name="userId">ID на потребителя (за валидация)</param>
        /// <returns>true при успех, false при неуспех</returns>
        Task<bool> MarkAsReadAsync(int notificationId, Guid userId);

        /// <summary>
        /// Маркира всички известия на даден потребител като прочетени
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Брой маркирани известия</returns>
        Task<int> MarkAllAsReadAsync(Guid userId);

        /// <summary>
        /// Връща броя на непрочетените известия на даден потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Брой непрочетени известия</returns>
        Task<int> GetUnreadNotificationCountAsync(Guid userId);

        /// <summary>
        /// Връща известие по ID
        /// </summary>
        /// <param name="notificationId">ID на известието</param>
        /// <returns>Известието или null, ако не съществува</returns>
        Task<Notification?> GetByIdAsync(int notificationId);
    }
}