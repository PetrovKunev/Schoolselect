using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Interfaces
{
    public interface ISchoolRecommendationService
    {
        /// <summary>
        /// Gets school recommendations based on user preferences
        /// </summary>
        /// <param name="preferenceId">ID of the user preference</param>
        /// <param name="userId">User ID for verification</param>
        /// <param name="gradesId">Optional ID of specific user grades set to use</param>
        /// <returns>List of recommended schools with scores</returns>
        Task<SchoolRecommendationsViewModel> GetRecommendationsAsync(int preferenceId, Guid userId, int? gradesId = null);
    }
}