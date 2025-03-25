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
                UserDistrict = p.UserDistrict,
                UserLatitude = p.UserLatitude,
                UserLongitude = p.UserLongitude,
                PreferredProfiles = string.IsNullOrEmpty(p.PreferredProfiles)
                    ? new List<string>()
                    : p.PreferredProfiles.Split(',').ToList(),
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

            return new UserPreferenceViewModel
            {
                Id = preference.Id,
                PreferenceName = preference.PreferenceName,
                UserDistrict = preference.UserDistrict,
                UserLatitude = preference.UserLatitude,
                UserLongitude = preference.UserLongitude,
                PreferredProfiles = string.IsNullOrEmpty(preference.PreferredProfiles)
                    ? new List<string>()
                    : preference.PreferredProfiles.Split(',').ToList(),
                CriteriaWeights = DeserializeCriteriaWeights(preference.CriteriaWeights),
                CreatedAt = preference.CreatedAt
            };
        }

        public async Task<int> CreateUserPreferenceAsync(UserPreferenceInputModel model, Guid userId)
        {
            var criteriaWeights = new Dictionary<string, double>
            {
                { "Proximity", model.ProximityWeight },
                { "Rating", model.RatingWeight },
                { "ScoreMatch", model.ScoreMatchWeight },
                { "ProfileMatch", model.ProfileMatchWeight },
                { "Facilities", model.FacilitiesWeight }
            };

            var userPreference = new UserPreference
            {
                UserId = userId,
                PreferenceName = model.PreferenceName,
                UserDistrict = model.UserDistrict,
                UserLatitude = model.UserLatitude,
                UserLongitude = model.UserLongitude,
                PreferredProfiles = string.Join(",", model.PreferredProfiles),
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
                { "Facilities", model.FacilitiesWeight }
            };

            preference.PreferenceName = model.PreferenceName;
            preference.UserDistrict = model.UserDistrict;
            preference.UserLatitude = model.UserLatitude;
            preference.UserLongitude = model.UserLongitude;
            preference.PreferredProfiles = string.Join(",", model.PreferredProfiles);
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
            // Връщаме предефинираните профили от ProfileTypes
            return new Dictionary<string, string>
            {
                { ProfileTypes.Mathematics, ProfileTypes.Mathematics },
                { ProfileTypes.NaturalSciences, ProfileTypes.NaturalSciences },
                { ProfileTypes.Humanities, ProfileTypes.Humanities },
                { ProfileTypes.ForeignLanguages, ProfileTypes.ForeignLanguages },
                { ProfileTypes.ComputerSciences, ProfileTypes.ComputerSciences },
                { ProfileTypes.Entrepreneurship, ProfileTypes.Entrepreneurship },
                { ProfileTypes.Arts, ProfileTypes.Arts },
                { ProfileTypes.Sports, ProfileTypes.Sports }
            };
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
                    { "Facilities", 2 }
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