using Microsoft.Extensions.DependencyInjection;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Extensions;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Triggers
{
    /// <summary>
    /// Статичен клас, съдържащ методи за автоматично генериране на известия
    /// </summary>
    public static class NotificationTriggers
    {
        /// <summary>
        /// Генерира известие при промяна в данните на училище
        /// </summary>
        public static async Task OnSchoolUpdatedAsync(
            IServiceProvider services,
            School school,
            bool wasAddressChanged = false,
            bool wasContactInfoChanged = false,
            bool wasDescriptionChanged = false)
        {
            using var scope = services.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            string title = $"Актуализирана информация за {school.Name}";
            string content = $"Информацията за училище {school.Name} беше актуализирана.";

            if (wasAddressChanged)
            {
                content += " Променен е адресът.";
            }

            if (wasContactInfoChanged)
            {
                content += " Променена е контактната информация.";
            }

            if (wasDescriptionChanged)
            {
                content += " Актуализирано е описанието.";
            }

            await notificationService.SendNotificationToSchoolObserversAsync(
                unitOfWork,
                school.Id,
                title,
                content,
                1); // Тип 1 = Промяна в училище
        }

        /// <summary>
        /// Генерира известие при добавяне на нов профил към училище
        /// </summary>
        public static async Task OnProfileAddedAsync(
            IServiceProvider services,
            SchoolProfile profile)
        {
            using var scope = services.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var school = await unitOfWork.Schools.GetByIdAsync(profile.SchoolId);

            string title = $"Нов профил в {school.Name}";
            string content = $"Добавен е нов профил \"{profile.Name}\" в училище {school.Name}.";

            if (profile.AvailablePlaces > 0)
            {
                content += $" Брой места: {profile.AvailablePlaces}.";
            }

            await notificationService.SendNotificationToSchoolObserversAsync(
                unitOfWork,
                school.Id,
                title,
                content,
                1); // Тип 1 = Промяна в училище
        }

        /// <summary>
        /// Генерира известие при добавяне на нови данни за класиране
        /// </summary>
        public static async Task OnRankingAddedAsync(
            IServiceProvider services,
            HistoricalRanking ranking)
        {
            using var scope = services.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var school = await unitOfWork.Schools.GetByIdAsync(ranking.SchoolId);

            string title;
            string content;
            int? referenceId;

            if (ranking.ProfileId.HasValue)
            {
                var profile = await unitOfWork.SchoolProfiles.GetByIdAsync(ranking.ProfileId.Value);
                title = $"Ново класиране за {profile.Name} в {school.Name}";
                content = $"Публикувани са резултати от {ranking.Round} класиране за {ranking.Year} г. за профил \"{profile.Name}\" в училище {school.Name}. " +
                          $"Минимален бал: {ranking.MinimumScore}.";
                referenceId = profile.Id;

                await notificationService.SendNotificationToProfileObserversAsync(
                    unitOfWork,
                    profile.Id,
                    title,
                    content,
                    2); // Тип 2 = Ново класиране
            }
            else
            {
                title = $"Ново класиране за {school.Name}";
                content = $"Публикувани са резултати от {ranking.Round} класиране за {ranking.Year} г. за училище {school.Name}. " +
                          $"Минимален бал: {ranking.MinimumScore}.";
                referenceId = school.Id;

                await notificationService.SendNotificationToSchoolObserversAsync(
                    unitOfWork,
                    school.Id,
                    title,
                    content,
                    2); // Тип 2 = Ново класиране
            }
        }

        /// <summary>
        /// Генерира известие при актуализиране на формулата за балообразуване
        /// </summary>
        public static async Task OnAdmissionFormulaUpdatedAsync(
            IServiceProvider services,
            AdmissionFormula formula)
        {
            using var scope = services.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var profile = await unitOfWork.SchoolProfiles.GetByIdAsync(formula.SchoolProfileId);
            var school = await unitOfWork.Schools.GetByIdAsync(profile.SchoolId);

            string title = $"Актуализирана формула за {profile.Name}";
            string content = $"Актуализирана е формулата за балообразуване за профил \"{profile.Name}\" в училище {school.Name} за {formula.Year} г.";

            await notificationService.SendNotificationToProfileObserversAsync(
                unitOfWork,
                profile.Id,
                title,
                content,
                2); // Тип 2 = Ново класиране
        }

        /// <summary>
        /// Генерира системно известие до всички потребители
        /// </summary>
        public static async Task SendSystemAnnouncementAsync(
            IServiceProvider services,
            string title,
            string content)
        {
            using var scope = services.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await notificationService.SendNotificationToAllAsync(
                title,
                content,
                3); // Тип 3 = Системно съобщение
        }
    }
}