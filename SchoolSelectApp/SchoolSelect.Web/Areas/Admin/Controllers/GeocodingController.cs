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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSchool(int schoolId)
        {
            bool isSuccess = await _schoolGeocodingService.UpdateSchoolCoordinatesAsync(schoolId);

            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Координатите са обновени успешно.";
            }
            else
            {
                TempData["ErrorMessage"] = "Не успяхме да обновим координатите на училището.";
            }

            return RedirectToAction("Edit", "Schools", new { id = schoolId, area = "Admin" });
        }
    }
}