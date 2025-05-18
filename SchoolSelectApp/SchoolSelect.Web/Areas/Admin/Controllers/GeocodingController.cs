// SchoolSelect.Web/Areas/Admin/Controllers/GeocodingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Services.Interfaces;
using System.Threading.Tasks;

namespace SchoolSelect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GeocodingController : Controller
    {
        private readonly ISchoolGeocodingService _schoolGeocodingService;

        public GeocodingController(ISchoolGeocodingService schoolGeocodingService)
        {
            _schoolGeocodingService = schoolGeocodingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAllSchools()
        {
            int updatedCount = await _schoolGeocodingService.UpdateAllSchoolCoordinatesAsync();
            TempData["SuccessMessage"] = $"Успешно обновени координати на {updatedCount} училища.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateSchool(int schoolId, string returnUrl)
        {
            try
            {
                var success = await _schoolGeocodingService.UpdateSchoolCoordinatesAsync(schoolId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Координатите бяха успешно обновени.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Възникна грешка при обновяване на координатите. Моля, опитайте отново.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Възникна грешка: {ex.Message}";
            }

            // Ако имаме returnUrl, пренасочваме към него
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // В противен случай пренасочваме към списъка с училища
            return RedirectToAction("Index", "Schools");
        }
    }
}