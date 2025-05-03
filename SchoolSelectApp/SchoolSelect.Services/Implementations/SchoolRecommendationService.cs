using Microsoft.Extensions.Logging;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Helpers;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;
using System.Text.Json;

namespace SchoolSelect.Services.Implementations
{
    public class SchoolRecommendationService : ISchoolRecommendationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SchoolRecommendationService> _logger;

        public SchoolRecommendationService(
            IUnitOfWork unitOfWork,
            ILogger<SchoolRecommendationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<SchoolRecommendationsViewModel> GetRecommendationsAsync(int preferenceId, Guid userId)
        {
            // Get the user preference
            var preference = await _unitOfWork.UserPreferences.GetByIdAsync(preferenceId);
            if (preference == null || preference.UserId != userId)
            {
                throw new InvalidOperationException($"Preference with ID {preferenceId} not found or doesn't belong to the user");
            }

            // Parse the criteria weights
            var criteriaWeights = DeserializeCriteriaWeights(preference.CriteriaWeights);

            // Get desired profiles
            var preferredProfiles = string.IsNullOrEmpty(preference.PreferredProfiles)
                ? new List<string>()
                : preference.PreferredProfiles.Split(',').ToList();

            // Get all schools
            var allSchools = await _unitOfWork.Schools.GetSchoolsWithProfilesAsync();

            // Calculate scores for each school
            var scoredSchools = new List<SchoolRecommendationViewModel>();
            foreach (var school in allSchools)
            {
                double schoolScore = 0;
                List<CriterionScoreViewModel> criteriaScores = new List<CriterionScoreViewModel>();

                // Calculate proximity score
                double proximityScore = CalculateProximityScore(school, preference, criteriaWeights);
                schoolScore += proximityScore * GetWeightValue(criteriaWeights, "Proximity");
                criteriaScores.Add(new CriterionScoreViewModel
                {
                    Name = "Близост",
                    Score = proximityScore,
                    Weight = GetWeightValue(criteriaWeights, "Proximity")
                });

                // Calculate rating score
                double ratingScore = CalculateRatingScore(school);
                schoolScore += ratingScore * GetWeightValue(criteriaWeights, "Rating");
                criteriaScores.Add(new CriterionScoreViewModel
                {
                    Name = "Рейтинг",
                    Score = ratingScore,
                    Weight = GetWeightValue(criteriaWeights, "Rating")
                });

                // Calculate profile match score
                double profileMatchScore = CalculateProfileMatchScore(school, preferredProfiles);
                schoolScore += profileMatchScore * GetWeightValue(criteriaWeights, "ProfileMatch");
                criteriaScores.Add(new CriterionScoreViewModel
                {
                    Name = "Профил",
                    Score = profileMatchScore,
                    Weight = GetWeightValue(criteriaWeights, "ProfileMatch")
                });

                // Calculate facilities score
                double facilitiesScore = await CalculateFacilitiesScoreAsync(school.Id);
                schoolScore += facilitiesScore * GetWeightValue(criteriaWeights, "Facilities");
                criteriaScores.Add(new CriterionScoreViewModel
                {
                    Name = "Допълнителни възможности",
                    Score = facilitiesScore,
                    Weight = GetWeightValue(criteriaWeights, "Facilities")
                });

                // Calculate total weighted score and normalize it to 0-100 scale
                double maxPossibleScore = GetWeightValue(criteriaWeights, "Proximity") +
                                          GetWeightValue(criteriaWeights, "Rating") +
                                          GetWeightValue(criteriaWeights, "ProfileMatch") +
                                          GetWeightValue(criteriaWeights, "Facilities");

                double normalizedScore = (schoolScore / maxPossibleScore) * 100;

                // Add to results
                scoredSchools.Add(new SchoolRecommendationViewModel
                {
                    School = new SchoolViewModel
                    {
                        Id = school.Id,
                        Name = school.Name,
                        District = school.District,
                        City = school.City,
                        AverageRating = school.AverageRating,
                        ReviewsCount = school.RatingsCount
                    },
                    TotalScore = Math.Round(normalizedScore, 1),
                    CriteriaScores = criteriaScores,
                    Profiles = school.Profiles.Select(p => p.Name).ToList()
                });
            }

            // Sort by total score descending
            scoredSchools = scoredSchools.OrderByDescending(s => s.TotalScore).ToList();

            // Create the view model
            var viewModel = new SchoolRecommendationsViewModel
            {
                PreferenceId = preferenceId,
                PreferenceName = preference.PreferenceName,
                UserDistrict = preference.UserDistrict ?? string.Empty,
                UserCoordinates = preference.UserLatitude.HasValue && preference.UserLongitude.HasValue
                    ? $"{preference.UserLatitude.Value}, {preference.UserLongitude.Value}"
                    : string.Empty,
                PreferredProfiles = preferredProfiles,
                RecommendedSchools = scoredSchools
            };

            return viewModel;
        }

        private Dictionary<string, double> DeserializeCriteriaWeights(string criteriaWeightsJson)
        {
            if (string.IsNullOrEmpty(criteriaWeightsJson))
            {
                return new Dictionary<string, double>
                {
                    { "Proximity", 3 },
                    { "Rating", 3 },
                    { "ScoreMatch", 4 },
                    { "ProfileMatch", 5 },
                    { "Facilities", 2 },
                    { "SearchRadius", 5 }
                };
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, double>>(criteriaWeightsJson)
                    ?? new Dictionary<string, double>();
            }
            catch
            {
                return new Dictionary<string, double>();
            }
        }

        private double GetWeightValue(Dictionary<string, double> weights, string key)
        {
            if (weights.TryGetValue(key, out double value))
            {
                return value;
            }

            // Default weights
            return key switch
            {
                "Proximity" => 3,
                "Rating" => 3,
                "ScoreMatch" => 4,
                "ProfileMatch" => 5,
                "Facilities" => 2,
                _ => 3
            };
        }

        private double CalculateProximityScore(School school, UserPreference preference, Dictionary<string, double> weights)
        {
            // If no coordinates, use district match
            if (!preference.UserLatitude.HasValue || !preference.UserLongitude.HasValue ||
                !school.GeoLatitude.HasValue || !school.GeoLongitude.HasValue)
            {
                return string.Equals(school.District, preference.UserDistrict, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.2;
            }

            // Calculate distance in km
            double distance = GeoHelper.CalculateDistance(
                preference.UserLatitude.Value, preference.UserLongitude.Value,
                school.GeoLatitude.Value, school.GeoLongitude.Value);

            // Get search radius or use default
            double searchRadius = weights.TryGetValue("SearchRadius", out double radius) ? radius : 5.0;

            // Score decreases linearly with distance up to the search radius
            if (distance <= searchRadius)
            {
                return 1.0 - (distance / searchRadius);
            }

            return 0.0;
        }

        private double CalculateRatingScore(School school)
        {
            // No ratings means neutral score
            if (school.RatingsCount == 0)
                return 0.5;

            // Convert 1-5 scale to 0-1 scale
            return (school.AverageRating - 1) / 4.0;
        }

        private double CalculateProfileMatchScore(School school, List<string> preferredProfiles)
        {
            // If no preferences, return neutral score
            if (!preferredProfiles.Any())
                return 0.5;

            // Count matching profiles
            int matchCount = school.Profiles.Count(p =>
                (p.Type.HasValue && preferredProfiles.Contains(p.Type.Value.ToString())) || // Ensure Type is not null
                (!string.IsNullOrEmpty(p.Specialty) && preferredProfiles.Contains(p.Specialty))); // Ensure Specialty is not null

            // If school has profiles, calculate match percentage
            if (school.Profiles.Any())
            {
                double matchPercentage = (double)matchCount / preferredProfiles.Count;
                return matchPercentage;
            }

            return 0;
        }

        private async Task<double> CalculateFacilitiesScoreAsync(int schoolId)
        {
            // Get facilities count
            var facilities = await _unitOfWork.SchoolFacilities.GetFacilitiesBySchoolIdAsync(schoolId);
            int facilitiesCount = facilities.Count();

            // Simple score based on number of facilities (0-10 scale normalized to 0-1)
            return Math.Min(facilitiesCount, 10) / 10.0;
        }
    }
}