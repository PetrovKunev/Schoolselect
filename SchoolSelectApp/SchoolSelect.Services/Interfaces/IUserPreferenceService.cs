using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Interfaces
{
    public interface IUserPreferenceService
    {
        /// <summary>
        /// Получава всички предпочитания на потребителя
        /// </summary>
        Task<List<UserPreferenceViewModel>> GetUserPreferencesAsync(Guid userId);

        /// <summary>
        /// Получава конкретно предпочитание
        /// </summary>
        Task<UserPreferenceViewModel?> GetUserPreferenceByIdAsync(int preferenceId);

        /// <summary>
        /// Създава ново предпочитание
        /// </summary>
        Task<int> CreateUserPreferenceAsync(UserPreferenceInputModel model, Guid userId);

        /// <summary>
        /// Обновява съществуващо предпочитание
        /// </summary>
        Task UpdateUserPreferenceAsync(UserPreferenceInputModel model, int preferenceId);

        /// <summary>
        /// Изтрива предпочитание
        /// </summary>
        Task DeleteUserPreferenceAsync(int preferenceId);

        /// <summary>
        /// Получава списък с всички възможни райони
        /// </summary>
        Task<List<string>> GetAllDistrictsAsync();

        /// <summary>
        /// Получава списък с всички възможни профили
        /// </summary>
        Task<Dictionary<string, string>> GetAllProfileTypesAsync();
    }
}