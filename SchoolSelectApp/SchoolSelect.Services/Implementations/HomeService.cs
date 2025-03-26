// SchoolSelect.Services/Implementations/HomeService.cs
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Implementations
{
    public class HomeService : IHomeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<HomeViewModel> GetHomePageDataAsync()
        {
            var model = new HomeViewModel();

            // Get total counts
            model.TotalSchoolsCount = await _unitOfWork.Schools.CountAsync();
            model.TotalProfilesCount = await _unitOfWork.SchoolProfiles.CountAsync();

            // Get top rated schools (само ако има записи)
            if (model.TotalSchoolsCount > 0)
            {
                var topRatedSchools = await _unitOfWork.Schools.GetTopRatedSchoolsAsync(6);

                // Convert to view models
                foreach (var school in topRatedSchools)
                {
                    var profiles = await _unitOfWork.SchoolProfiles.GetProfilesBySchoolIdAsync(school.Id);
                    var latestRankings = await _unitOfWork.HistoricalRankings.GetRankingsBySchoolIdAsync(school.Id);

                    var profileTypes = profiles
                        .Select(p => p.Name)
                        .Distinct()
                        .ToList();

                    var minimumScore = latestRankings.Any()
                        ? latestRankings.OrderByDescending(r => r.Year).FirstOrDefault()?.MinimumScore
                        : null;

                    var schoolViewModel = new SchoolViewModel
                    {
                        Id = school.Id,
                        Name = school.Name,
                        District = school.District,
                        City = school.City,
                        AverageRating = school.AverageRating,
                        ReviewsCount = school.RatingsCount,
                        ProfileTypes = profileTypes,
                        MinimumScore = minimumScore
                    };

                    model.TopRatedSchools.Add(schoolViewModel);
                }
            }

            return model;
        }
        
    }
}