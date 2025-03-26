using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Extensions
{
    /// <summary>
    /// Разширения за услугата за известия
    /// </summary>
    public static class NotificationServiceExtensions
    {
        /// <summary>
        /// Изпраща известие до потребители, които наблюдават конкретно училище
        /// </summary>
        public static async Task<int> SendNotificationToSchoolObserversAsync(
            this INotificationService notificationService,
            IUnitOfWork unitOfWork,
            int schoolId,
            string title,
            string content,
            int notificationType = 1)
        {
            // Намираме всички потребители, които имат това училище в сравнения или предпочитания
            var userIdsFromComparisons = await GetUserIdsFromComparisonSetsAsync(unitOfWork, schoolId);
            var userIdsFromPreferences = await GetUserIdsFromPreferencesAsync(unitOfWork, schoolId);

            // Обединяваме ID-тата
            var userIds = userIdsFromComparisons.Union(userIdsFromPreferences).Distinct();

            // Изпращаме известие
            return await notificationService.SendNotificationToUsersAsync(
                userIds,
                title,
                content,
                notificationType);
        }

        /// <summary>
        /// Изпраща известие до потребители, които наблюдават конкретен профил
        /// </summary>
        public static async Task<int> SendNotificationToProfileObserversAsync(
            this INotificationService notificationService,
            IUnitOfWork unitOfWork,
            int profileId,
            string title,
            string content,
            int notificationType = 2)
        {
            // Намираме потребители, които имат този профил в сравнения
            var comparisonSetUsers = await unitOfWork.ComparisonSets.FindAsync(cs =>
                cs.Items.Any(i => i.ProfileId == profileId));

            var userIds = comparisonSetUsers.Select(cs => cs.UserId).Distinct();

            // Намираме потребители, които имат този профил в предпочитания
            // За това трябва да извлечем името на профила и да търсим по него
            var profile = await unitOfWork.SchoolProfiles.GetByIdAsync(profileId);
            var profileName = profile.Name;

            var userPreferences = await unitOfWork.UserPreferences.FindAsync(up =>
                up.PreferredProfiles.Contains(profileName));

            var userIdsFromPreferences = userPreferences.Select(up => up.UserId).Distinct();

            // Обединяваме резултатите
            var allUserIds = userIds.Union(userIdsFromPreferences).Distinct();

            // Изпращаме известие
            return await notificationService.SendNotificationToUsersAsync(
                allUserIds,
                title,
                content,
                notificationType);
        }

        /// <summary>
        /// Изпраща известие при промяна в минималния бал за класиране
        /// </summary>
        public static async Task<int> SendScoreChangeNotificationAsync(
            this INotificationService notificationService,
            IUnitOfWork unitOfWork,
            int profileId,
            double oldScore,
            double newScore)
        {
            var profile = await unitOfWork.SchoolProfiles.GetByIdAsync(profileId);
            var school = await unitOfWork.Schools.GetByIdAsync(profile.SchoolId);

            string title = $"Промяна в минималния бал за {profile.Name}";
            string content = $"Минималният бал за профил {profile.Name} в {school.Name} е променен от {oldScore} на {newScore}.";

            return await notificationService.SendNotificationToProfileObserversAsync(
                unitOfWork,
                profileId,
                title,
                content,
                2); // Тип 2 = Ново класиране
        }

        #region Private Helper Methods

        private static async Task<IEnumerable<Guid>> GetUserIdsFromComparisonSetsAsync(
            IUnitOfWork unitOfWork,
            int schoolId)
        {
            var comparisonSets = await unitOfWork.ComparisonSets.FindAsync(cs =>
                cs.Items.Any(i => i.SchoolId == schoolId));

            return comparisonSets.Select(cs => cs.UserId).Distinct();
        }

        private static async Task<IEnumerable<Guid>> GetUserIdsFromPreferencesAsync(
            IUnitOfWork unitOfWork,
            int schoolId)
        {
            // Намираме училището, за да знаем района
            var school = await unitOfWork.Schools.GetByIdAsync(schoolId);
            var district = school.District;

            // Намираме всички предпочитания с този район
            var userPreferences = await unitOfWork.UserPreferences.FindAsync(up =>
                up.UserDistrict == district);

            return userPreferences.Select(up => up.UserId).Distinct();
        }

        #endregion
    }
}