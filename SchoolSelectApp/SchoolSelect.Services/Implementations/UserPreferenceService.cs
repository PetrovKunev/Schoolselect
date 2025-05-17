using System.Text.Json;
using SchoolSelect.Common;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Implementations
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserPreferenceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<UserPreferenceViewModel>> GetUserPreferencesAsync(Guid userId)
        {
            var preferences = await _unitOfWork.UserPreferences.GetPreferencesByUserIdAsync(userId);

            return preferences.Select(p => new UserPreferenceViewModel
            {
                Id = p.Id,
                PreferenceName = p.PreferenceName,
                UserDistrict = p.UserDistrict ?? string.Empty,
                UserLatitude = p.UserLatitude,
                UserLongitude = p.UserLongitude,
                PreferredProfiles = string.IsNullOrEmpty(p.PreferredProfiles)
                    ? new List<string>()
                    : p.PreferredProfiles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(profile => profile.Trim())
                        .ToList(),
                CriteriaWeights = DeserializeCriteriaWeights(p.CriteriaWeights),
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        public async Task<UserPreferenceViewModel?> GetUserPreferenceByIdAsync(int preferenceId)
        {
            var preference = await _unitOfWork.UserPreferences.GetByIdAsync(preferenceId);

            if (preference == null)
            {
                return null;
            }

            var criteriaWeights = DeserializeCriteriaWeights(preference.CriteriaWeights);

            var viewModel = new UserPreferenceViewModel
            {
                Id = preference.Id,
                PreferenceName = preference.PreferenceName,
                UserDistrict = preference.UserDistrict ?? string.Empty,
                UserLatitude = preference.UserLatitude,
                UserLongitude = preference.UserLongitude,
                PreferredProfiles = string.IsNullOrEmpty(preference.PreferredProfiles)
                    ? new List<string>()
                    : preference.PreferredProfiles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(profile => profile.Trim())
                        .ToList(),
                CriteriaWeights = criteriaWeights,
                CreatedAt = preference.CreatedAt
            };

            return viewModel;
        }

        public async Task<int> CreateUserPreferenceAsync(UserPreferenceInputModel model, Guid userId)
        {
            var criteriaWeights = new Dictionary<string, double>
            {
                { "Proximity", model.ProximityWeight },
                { "Rating", model.RatingWeight },
                { "ScoreMatch", model.ScoreMatchWeight },
                { "ProfileMatch", model.ProfileMatchWeight },
                //{ "Facilities", model.FacilitiesWeight },
                { "SearchRadius", model.SearchRadius }
            };

            var userPreference = new UserPreference
            {
                UserId = userId,
                PreferenceName = model.PreferenceName,
                UserDistrict = model.UserDistrict ?? string.Empty,
                UserLatitude = model.UserLatitude,
                UserLongitude = model.UserLongitude,
                PreferredProfiles = string.Join(",", model.PreferredProfiles.Select(p => p.Trim())),
                CriteriaWeights = JsonSerializer.Serialize(criteriaWeights),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserPreferences.AddAsync(userPreference);
            await _unitOfWork.CompleteAsync();

            return userPreference.Id;
        }

        public async Task UpdateUserPreferenceAsync(UserPreferenceInputModel model, int preferenceId)
        {
            var preference = await _unitOfWork.UserPreferences.GetByIdAsync(preferenceId);

            if (preference == null)
            {
                throw new InvalidOperationException($"Не е намерено предпочитание с ID {preferenceId}");
            }

            var criteriaWeights = new Dictionary<string, double>
            {
                { "Proximity", model.ProximityWeight },
                { "Rating", model.RatingWeight },
                { "ScoreMatch", model.ScoreMatchWeight },
                { "ProfileMatch", model.ProfileMatchWeight },
                //{ "Facilities", model.FacilitiesWeight },
                { "SearchRadius", model.SearchRadius }
            };

            preference.PreferenceName = model.PreferenceName;
            preference.UserDistrict = model.UserDistrict;
            preference.UserLatitude = model.UserLatitude;
            preference.UserLongitude = model.UserLongitude;
            preference.PreferredProfiles = string.Join(",", model.PreferredProfiles.Select(p => p.Trim()));
            preference.CriteriaWeights = JsonSerializer.Serialize(criteriaWeights);

            _unitOfWork.UserPreferences.Update(preference);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteUserPreferenceAsync(int preferenceId)
        {
            var preference = await _unitOfWork.UserPreferences.GetByIdAsync(preferenceId);

            if (preference == null)
            {
                throw new InvalidOperationException($"Не е намерено предпочитание с ID {preferenceId}");
            }

            _unitOfWork.UserPreferences.Remove(preference);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<List<string>> GetAllDistrictsAsync()
        {
            var schools = await _unitOfWork.Schools.GetAllAsync();
            return schools.Select(s => s.District).Distinct().OrderBy(d => d).ToList();
        }

        public async Task<Dictionary<string, string>> GetAllProfileTypesAsync()
        {
            // Основен речник с предварително дефинирани профили
            var profileTypesDict = new Dictionary<string, string>
            {
                { ProfileTypes.Mathematics, "Математически профил" },
                { ProfileTypes.NaturalSciences, "Природни науки (Биология, Химия, Физика)" },
                { ProfileTypes.Humanities, "Хуманитарни науки (История, География, Езици)" },
                { ProfileTypes.ForeignLanguages, "Чужди езици (АЕ, НЕ, ФЕ и др.)" },
                { ProfileTypes.ComputerSciences, "Софтуерни и хардуерни науки (STEM)" },
                { ProfileTypes.Entrepreneurship, "Предприемачески профил" },
                { ProfileTypes.Arts, "Изкуства (Музика, Изобразително изкуство)" },
                { ProfileTypes.Sports, "Спортен профил" }
            };

            // Извличаме уникални профили от базата данни
            var schoolProfiles = await _unitOfWork.SchoolProfiles.GetAllAsync();
            var uniqueProfileNames = new HashSet<string>();

            foreach (var profile in schoolProfiles)
            {
                // Извличаме базовия профил от името (например "Математически" от "Математически - АЕ интензивно, ИЕ")
                if (!string.IsNullOrEmpty(profile.Name))
                {
                    var parts = profile.Name.Split('-', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        var baseName = parts[0].Trim();
                        if (!string.IsNullOrEmpty(baseName) && baseName.Length > 5)
                        {
                            uniqueProfileNames.Add(baseName);
                        }
                    }

                    // Проверяваме за ключови думи в имената на профилите
                    string name = profile.Name.ToLower();

                    if (name.Contains("математич") || name.Contains("математик") || name.Contains(" мат "))
                    {
                        uniqueProfileNames.Add("Математически");
                    }
                    else if (name.Contains("природни") || name.Contains("биолог") || name.Contains("химия") ||
                             name.Contains("физика") || name.Contains("бзо") || name.Contains("хоос"))
                    {
                        uniqueProfileNames.Add("Природни науки");
                    }
                    else if (name.Contains("хуманитарен") || name.Contains("история") ||
                             name.Contains("география") || name.Contains("бел и "))
                    {
                        uniqueProfileNames.Add("Хуманитарни науки");
                    }
                    else if (name.Contains("език") || name.Contains(" ае ") || name.Contains("интензивно") ||
                             name.Contains("чужд"))
                    {
                        uniqueProfileNames.Add("Чужди езици");
                    }
                    else if (name.Contains("софтуер") || name.Contains("хардуер") || name.Contains("компютър") ||
                             name.Contains("програмира") || name.Contains("информатика") || name.Contains("stem") ||
                             name.Contains("стем"))
                    {
                        uniqueProfileNames.Add("Софтуерни и хардуерни науки");
                    }
                    else if (name.Contains("предприема") || name.Contains("бизнес") || name.Contains("икономика"))
                    {
                        uniqueProfileNames.Add("Предприемачески");
                    }
                    else if (name.Contains("изкуств") || name.Contains("музика") || name.Contains("изобразително"))
                    {
                        uniqueProfileNames.Add("Изкуства");
                    }
                    else if (name.Contains("спорт") || name.Contains("физическо"))
                    {
                        uniqueProfileNames.Add("Спорт");
                    }
                }

                // Вземаме и типа профил, ако е дефиниран
                if (profile.Type.HasValue)
                {
                    uniqueProfileNames.Add(profile.Type.Value.ToString());
                }

                // Добавяме и специалността, ако е дефинирана и има смисъл като самостоятелен профил
                if (!string.IsNullOrEmpty(profile.Specialty) && profile.Specialty.Length > 5)
                {
                    uniqueProfileNames.Add(profile.Specialty);
                }
            }

            // Добавяме уникалните профили от базата данни към речника
            foreach (var profileName in uniqueProfileNames)
            {
                if (!profileTypesDict.ContainsKey(profileName))
                {
                    profileTypesDict[profileName] = profileName;
                }
            }

            return profileTypesDict;
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
    }
}