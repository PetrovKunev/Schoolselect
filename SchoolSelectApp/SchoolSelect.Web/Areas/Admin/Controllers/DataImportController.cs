using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class DataImportController : Controller
    {
        private readonly ISchoolImportService _schoolImportService;

        public DataImportController(ISchoolImportService schoolImportService)
        {
            _schoolImportService = schoolImportService;
        }

        // GET: /Admin/DataImport
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/DataImport/ImportSchools
        public IActionResult ImportSchools()
        {
            return View();
        }

        // POST: /Admin/DataImport/ImportSchoolsFromCsv
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportSchoolsFromCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Моля, изберете файл за импортиране.";
                return RedirectToAction(nameof(ImportSchools));
            }

            string fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
            if (fileExtension != ".csv")
            {
                TempData["ErrorMessage"] = "Моля, изберете CSV файл.";
                return RedirectToAction(nameof(ImportSchools));
            }

            var result = await _schoolImportService.ImportSchoolsFromCsvAsync(file);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Успешно импортирани {result.SuccessCount} училища. " +
                    (result.FailureCount > 0 ? $"Грешки: {result.FailureCount}" : "");

                if (result.Errors.Any())
                {
                    TempData["ImportErrors"] = result.Errors;
                }
            }
            else
            {
                TempData["ErrorMessage"] = $"Грешка при импортиране: {result.ErrorMessage}";

                if (result.Errors.Any())
                {
                    TempData["ImportErrors"] = result.Errors;
                }
            }

            return RedirectToAction(nameof(ImportSchools));
        }

        // POST: /Admin/DataImport/ImportSchoolsFromExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportSchoolsFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Моля, изберете файл за импортиране.";
                return RedirectToAction(nameof(ImportSchools));
            }

            string fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
            if (fileExtension != ".xlsx" && fileExtension != ".xls")
            {
                TempData["ErrorMessage"] = "Моля, изберете Excel файл (.xlsx или .xls).";
                return RedirectToAction(nameof(ImportSchools));
            }

            var result = await _schoolImportService.ImportSchoolsFromExcelAsync(file);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Успешно импортирани {result.SuccessCount} училища. " +
                    (result.FailureCount > 0 ? $"Грешки: {result.FailureCount}" : "");

                if (result.Errors.Any())
                {
                    TempData["ImportErrors"] = result.Errors;
                }
            }
            else
            {
                TempData["ErrorMessage"] = $"Грешка при импортиране: {result.ErrorMessage}";

                if (result.Errors.Any())
                {
                    TempData["ImportErrors"] = result.Errors;
                }
            }

            return RedirectToAction(nameof(ImportSchools));
        }
    }
}