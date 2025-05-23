﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Web.Controllers
{
    [Authorize]
    public class UserPreferencesController : Controller
    {
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly ILogger<UserPreferencesController> _logger;

        public UserPreferencesController(IUserPreferenceService userPreferenceService, ILogger<UserPreferencesController> logger)
        {
            _userPreferenceService = userPreferenceService;
            _logger = logger;
        }

        // GET: UserPreferences
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var preferences = await _userPreferenceService.GetUserPreferencesAsync(userId);
            return View(preferences);
        }

        // GET: UserPreferences/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var preference = await _userPreferenceService.GetUserPreferenceByIdAsync(id);

            if (preference == null)
            {
                return NotFound();
            }

            return View(preference);
        }

        // GET: UserPreferences/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Districts = await _userPreferenceService.GetAllDistrictsAsync();
            ViewBag.ProfileTypes = await _userPreferenceService.GetAllProfileTypesAsync();

            return View(new UserPreferenceInputModel());
        }

        // POST: UserPreferences/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserPreferenceInputModel model)
        {
            // Проверяваме дали има поне един от двата начина за определяне на местоположение
            bool hasLocation = !string.IsNullOrEmpty(model.UserDistrict) ||
                               (model.UserLatitude.HasValue && model.UserLongitude.HasValue);

            // Ако нямаме никакво местоположение, добавяме грешка
            if (!hasLocation)
            {
                // Добавяме ръчно грешка за местоположението
                ModelState.AddModelError("UserDistrict", "Моля, изберете район или задайте точно местоположение на картата.");

                ViewBag.Districts = await _userPreferenceService.GetAllDistrictsAsync();
                ViewBag.ProfileTypes = await _userPreferenceService.GetAllProfileTypesAsync();

                return View(model);
            }

            // ВАЖНО: Валидиране на координатите
            if (model.UserLatitude.HasValue && model.UserLongitude.HasValue)
            {
                // Проверка за валидни граници на координатите
                if (Math.Abs(model.UserLatitude.Value) > 90 || Math.Abs(model.UserLongitude.Value) > 180)
                {
                    ModelState.AddModelError("", "Невалидни географски координати. Моля, задайте правилно местоположение на картата.");

                    ViewBag.Districts = await _userPreferenceService.GetAllDistrictsAsync();
                    ViewBag.ProfileTypes = await _userPreferenceService.GetAllProfileTypesAsync();

                    return View(model);
                }

                // Проверка дали координатите са в разумни граници за България
                bool isInBulgaria = model.UserLatitude.Value >= 41.0 && model.UserLatitude.Value <= 44.5 &&
                                   model.UserLongitude.Value >= 22.0 && model.UserLongitude.Value <= 29.0;

                if (!isInBulgaria)
                {
                    _logger.LogWarning("Координатите ({Lat}, {Lon}) изглежда са извън България!",
                        model.UserLatitude.Value, model.UserLongitude.Value);
                }
            }

            // Ако имаме координати, но радиусът не е зададен или е невалиден, задаваме стойност по подразбиране
            if (model.UserLatitude.HasValue && model.UserLongitude.HasValue &&
               (model.SearchRadius <= 0 || model.SearchRadius > 50))
            {
                model.SearchRadius = 5; // Стойност по подразбиране в километри
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    await _userPreferenceService.CreateUserPreferenceAsync(model, userId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Грешка при създаване на предпочитание");
                    ModelState.AddModelError("", "Възникна грешка при запазване на предпочитанието. Моля опитайте отново.");
                }
            }

            ViewBag.Districts = await _userPreferenceService.GetAllDistrictsAsync();
            ViewBag.ProfileTypes = await _userPreferenceService.GetAllProfileTypesAsync();
            return View(model);
        }

        // GET: UserPreferences/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var preference = await _userPreferenceService.GetUserPreferenceByIdAsync(id);

            if (preference == null)
            {
                return NotFound();
            }

            var criteriaWeights = preference.CriteriaWeights;

            var searchRadius = 5; // стойност по подразбиране
            if (criteriaWeights.TryGetValue("SearchRadius", out double radiusValue))
            {
                searchRadius = (int)radiusValue;
            }

            var model = new UserPreferenceInputModel
            {
                PreferenceName = preference.PreferenceName,
                UserDistrict = preference.UserDistrict,
                UserLatitude = preference.UserLatitude,
                UserLongitude = preference.UserLongitude,
                PreferredProfiles = preference.PreferredProfiles,
                ProximityWeight = GetWeightValue(criteriaWeights, "Proximity"),
                RatingWeight = GetWeightValue(criteriaWeights, "Rating"),
                ScoreMatchWeight = GetWeightValue(criteriaWeights, "ScoreMatch"),
                ProfileMatchWeight = GetWeightValue(criteriaWeights, "ProfileMatch"),
                //FacilitiesWeight = GetWeightValue(criteriaWeights, "Facilities"),
                SearchRadius = searchRadius // Добавяме радиуса
            };

            ViewBag.Districts = await _userPreferenceService.GetAllDistrictsAsync();
            ViewBag.ProfileTypes = await _userPreferenceService.GetAllProfileTypesAsync();

            return View(model);
        }

        // POST: UserPreferences/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserPreferenceInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userPreferenceService.UpdateUserPreferenceAsync(model, id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Грешка при редактиране на предпочитание");
                    ModelState.AddModelError("", "Възникна грешка при запазване на промените. Моля опитайте отново.");
                }
            }

            ViewBag.Districts = await _userPreferenceService.GetAllDistrictsAsync();
            ViewBag.ProfileTypes = await _userPreferenceService.GetAllProfileTypesAsync();

            return View(model);
        }

        // GET: UserPreferences/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var preference = await _userPreferenceService.GetUserPreferenceByIdAsync(id);

            if (preference == null)
            {
                return NotFound();
            }

            return View(preference);
        }

        // POST: UserPreferences/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _userPreferenceService.DeleteUserPreferenceAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Helper method to get weight values
        private int GetWeightValue(Dictionary<string, double> weights, string key)
        {
            if (weights.TryGetValue(key, out double value))
            {
                return (int)value;
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
    }
}