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
                : preference.PreferredProfiles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToList();

            // Get all schools
            var allSchools = await _unitOfWork.Schools.GetSchoolsWithProfilesAsync();

            // Добавяме дебъг информация за разстоянията
            var debugInfo = new List<string>();

            // Calculate scores for each school
            var scoredSchools = new List<SchoolRecommendationViewModel>();
            foreach (var school in allSchools)
            {
                double schoolScore = 0;
                List<CriterionScoreViewModel> criteriaScores = new List<CriterionScoreViewModel>();

                // Изчисляване на разстоянието, ако имаме координати
                double? distance = null;

                if (preference.UserLatitude.HasValue && preference.UserLongitude.HasValue &&
                    school.GeoLatitude.HasValue && school.GeoLongitude.HasValue)
                {
                    distance = GeoHelper.CalculateDistance(
                        preference.UserLatitude.Value, preference.UserLongitude.Value,
                        school.GeoLatitude.Value, school.GeoLongitude.Value);

                    // Дебъг информация за разстоянието
                    debugInfo.Add($"Училище: {school.Name}, Разстояние: {Math.Round(distance.Value, 2)} км");
                }

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

                // Calculate facilities score is commented out
                /*double facilitiesScore = await CalculateFacilitiesScoreAsync(school.Id);
                schoolScore += facilitiesScore * GetWeightValue(criteriaWeights, "Facilities");
                criteriaScores.Add(new CriterionScoreViewModel
                {
                    Name = "Допълнителни възможности",
                    Score = facilitiesScore,
                    Weight = GetWeightValue(criteriaWeights, "Facilities")
                });*/

                // Calculate total weighted score and normalize it to 0-100 scale
                double maxPossibleScore = GetWeightValue(criteriaWeights, "Proximity") +
                                          GetWeightValue(criteriaWeights, "Rating") +
                                          GetWeightValue(criteriaWeights, "ProfileMatch");

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
                    Profiles = school.Profiles.Select(p => p.Name).ToList(),
                    Distance = distance // Добавяме разстоянието в модела
                });
            }

            // Запис на дебъг информацията в лога
            foreach (var info in debugInfo.OrderBy(i => i))
            {
                _logger.LogDebug(info);
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
                    //{ "Facilities", 2 },
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
                //"Facilities" => 2,
                _ => 3
            };
        }

        private double CalculateProximityScore(School school, UserPreference preference, Dictionary<string, double> weights)
        {
            // 1. Проверка дали имаме валидни координати за потребителя
            if (!preference.UserLatitude.HasValue || !preference.UserLongitude.HasValue)
            {
                // Ако имаме само район, проверяваме съвпадение по район
                if (!string.IsNullOrEmpty(preference.UserDistrict) && !string.IsNullOrEmpty(school.District))
                {
                    return string.Equals(school.District, preference.UserDistrict, StringComparison.OrdinalIgnoreCase)
                        ? 1.0  // 100% съвпадение за същия район
                        : 0.2; // 20% съвпадение за различен район
                }

                // Ако нямаме район, връщаме неутрален резултат
                return 0.5;
            }

            // 2. Проверка дали имаме валидни координати за училището
            if (!school.GeoLatitude.HasValue || !school.GeoLongitude.HasValue)
            {
                // Ако имаме само район, проверяваме съвпадение по район
                if (!string.IsNullOrEmpty(preference.UserDistrict) && !string.IsNullOrEmpty(school.District))
                {
                    return string.Equals(school.District, preference.UserDistrict, StringComparison.OrdinalIgnoreCase)
                        ? 1.0  // 100% съвпадение за същия район
                        : 0.2; // 20% съвпадение за различен район
                }

                // Ако нямаме район, връщаме неутрален резултат
                return 0.5;
            }

            // 3. На този етап знаем, че и двете координати са валидни
            // Запазваме координатите в локални променливи, за да избегнем nullable предупрежденията
            double userLat = preference.UserLatitude.Value; // Безопасно, защото вече проверихме HasValue
            double userLon = preference.UserLongitude.Value;
            double schoolLat = school.GeoLatitude.Value;
            double schoolLon = school.GeoLongitude.Value;

            // 4. Изчисляваме разстоянието
            double distance = GeoHelper.CalculateDistance(userLat, userLon, schoolLat, schoolLon);

            // 5. Вземаме радиуса на търсене
            double searchRadius = 5.0; // км по подразбиране
            if (weights.TryGetValue("SearchRadius", out double radius) && radius > 0)
            {
                searchRadius = radius;
            }

            // 6. Логаритмична функция за по-плавно намаляване
            if (distance <= 0.5) // Много близко (в рамките на 500 метра)
            {
                return 1.0; // 100% съвпадение
            }
            else if (distance <= searchRadius)
            {
                // Логаритмично намаляване
                double logarithmicScore = 1.0 - (Math.Log10(distance + 1) / Math.Log10(searchRadius + 1));
                return Math.Max(0.1, logarithmicScore);
            }
            else if (distance <= searchRadius * 1.5)
            {
                // За училища малко извън радиуса
                return 0.1;
            }

            // За училища далеч извън радиуса
            return 0.05;
        }

        private double CalculateRatingScore(School school)
        {
            // No ratings means neutral score
            if (school.RatingsCount == 0)
                return 0.5;

            // Convert 1-5 scale to 0-1 scale
            return (school.AverageRating - 1) / 4.0;
        }

        // Добавям само новия метод, който трябва да замени съществуващия в SchoolRecommendationService.cs
        private double CalculateProfileMatchScore(School school, List<string> preferredProfiles)
        {
            // Ако няма предпочитания или училището няма профили, връщаме неутрален резултат
            if (!preferredProfiles.Any() || !school.Profiles.Any())
                return 0.5;

            double totalMatch = 0;
            double maxPossibleMatch = preferredProfiles.Count;

            // Създаваме речник с ключови думи за всеки предпочитан профил
            var profileKeywords = new Dictionary<string, List<string>>();

            foreach (var profile in preferredProfiles)
            {
                // Добавяме ключови думи за всеки тип профил на базата на известните категории
                string profileKey = profile.ToLower();

                if (profileKey.Contains("математич") || profileKey.Contains("математик") || profileKey == "математически")
                {
                    profileKeywords[profile] = new List<string> { "математич", "математик", "мат", "математически" };
                }
                else if (profileKey.Contains("природн") || profileKey.Contains("биолог") || profileKey.Contains("химия") ||
                         profileKey.Contains("физика") || profileKey.Contains("бзо") || profileKey.Contains("хоос"))
                {
                    profileKeywords[profile] = new List<string> { "природни", "биолог", "химия", "физика", "бзо", "хоос", "фа" };
                }
                else if (profileKey.Contains("хуманитар") || profileKey.Contains("истор") || profileKey.Contains("географ") ||
                         profileKey.Contains("бел"))
                {
                    profileKeywords[profile] = new List<string> { "хуманитарен", "хуманитарни", "история", "география", "бел", "ези" };
                }
                else if (profileKey.Contains("език") || profileKey.Contains("ае") || profileKey.Contains("интензивно") ||
                         profileKey.Contains("чужд"))
                {
                    profileKeywords[profile] = new List<string> { "език", "езици", "ае", "ие", "не", "ре", "фе", "интензивно" };
                }
                else if (profileKey.Contains("софтуер") || profileKey.Contains("хардуер") || profileKey.Contains("компютър") ||
                         profileKey.Contains("програмир") || profileKey.Contains("информатик") || profileKey.Contains("stem") ||
                         profileKey.Contains("стем"))
                {
                    profileKeywords[profile] = new List<string> { "софтуер", "хардуер", "компютър", "програмира", "информатика", "кодиране", "stem", "стем" };
                }
                else if (profileKey.Contains("предприема") || profileKey.Contains("бизнес") || profileKey.Contains("икономика"))
                {
                    profileKeywords[profile] = new List<string> { "предприема", "бизнес", "икономика", "мениджмънт" };
                }
                else if (profileKey.Contains("изкуств") || profileKey.Contains("музика") || profileKey.Contains("изобразит"))
                {
                    profileKeywords[profile] = new List<string> { "изкуств", "музика", "изобразително", "рисуване", "театрал" };
                }
                else if (profileKey.Contains("спорт") || profileKey.Contains("физическ"))
                {
                    profileKeywords[profile] = new List<string> { "спорт", "физическо" };
                }
                else
                {
                    // За неразпознати профили използваме самото име като ключова дума
                    // и добавяме части от думата за по-гъвкаво съвпадение
                    var words = profile.ToLower().Split(new[] { ' ', '-', ',', '(', ')', '.' }, StringSplitOptions.RemoveEmptyEntries);
                    profileKeywords[profile] = new List<string>();

                    foreach (var word in words)
                    {
                        if (word.Length > 3)
                        {
                            profileKeywords[profile].Add(word);
                            // Добавяме и началото на думата за по-гъвкаво съвпадение
                            if (word.Length > 5)
                            {
                                profileKeywords[profile].Add(word.Substring(0, Math.Min(5, word.Length)));
                            }
                        }
                    }

                    // Ако няма подходящи ключови думи, използваме целия низ
                    if (!profileKeywords[profile].Any())
                    {
                        profileKeywords[profile].Add(profile.ToLower());
                    }
                }
            }

            // За всеки профил в училището
            foreach (var schoolProfile in school.Profiles)
            {
                string profileName = schoolProfile.Name?.ToLower() ?? string.Empty;
                string profileType = schoolProfile.Type?.ToString()?.ToLower() ?? string.Empty;
                string specialty = schoolProfile.Specialty?.ToLower() ?? string.Empty;

                // Създаваме комбиниран низ за търсене
                string searchText = $"{profileName} {profileType} {specialty}";

                // Изчисляване на частично съвпадение за всеки предпочитан профил
                foreach (var preferredProfile in preferredProfiles)
                {
                    double bestMatch = 0;

                    // Ако има точно съвпадение, даваме максимален резултат
                    if (searchText.Contains(preferredProfile.ToLower()))
                    {
                        bestMatch = 1.0;
                    }
                    else
                    {
                        // Иначе търсим ключови думи
                        int foundKeywords = 0;

                        // Ако имаме ключови думи за този профил
                        if (profileKeywords.ContainsKey(preferredProfile) && profileKeywords[preferredProfile].Any())
                        {
                            foreach (var keyword in profileKeywords[preferredProfile])
                            {
                                if (searchText.Contains(keyword))
                                {
                                    foundKeywords++;
                                }
                            }

                            // Изчисляваме частичното съвпадение на базата на намерените ключови думи
                            double keywordMatch = (double)foundKeywords / profileKeywords[preferredProfile].Count;

                            // Не даваме повече от 0.9 за съвпадение по ключови думи
                            bestMatch = Math.Min(keywordMatch, 0.9);
                        }
                    }

                    // Добавяме най-доброто съвпадение за този предпочитан профил
                    totalMatch += bestMatch;
                }
            }

            // Нормализиране на общия резултат
            // Но не повече от 1.0, дори ако има няколко профила, които съвпадат с предпочитанията
            return Math.Min(totalMatch / maxPossibleMatch, 1.0);
        }

        // Uncomment if you want to implement facilities score calculation
        //private async Task<double> CalculateFacilitiesScoreAsync(int schoolId)
        //{
        //    // Get facilities count
        //    var facilities = await _unitOfWork.SchoolFacilities.GetFacilitiesBySchoolIdAsync(schoolId);
        //    int facilitiesCount = facilities.Count();

        //    // Simple score based on number of facilities (0-10 scale normalized to 0-1)
        //    return Math.Min(facilitiesCount, 10) / 10.0;
        //}
    }
}