using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Repositories;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using System.Security.Claims;

namespace SchoolSelect.Web.Controllers
{
    [Authorize]
    public class ComparisonController : Controller
    {
        private readonly IComparisonService _comparisonService;
        private readonly ISchoolRepository _schoolRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ComparisonController(
            IComparisonService comparisonService,
            ISchoolRepository schoolRepository,
            IUnitOfWork unitOfWork)
        {
            _comparisonService = comparisonService;
            _schoolRepository = schoolRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Показва списък с всички набори за сравнение на потребителя
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ??
                                    User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ??
                                    string.Empty);

            if (userId == Guid.Empty)
                return Unauthorized();

            var comparisonSets = await _comparisonService.GetComparisonSetsByUserIdAsync(userId);
            return View(comparisonSets);
        }

        /// <summary>
        /// Показва детайли за конкретен набор за сравнение
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ??
                                    User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ??
                                    string.Empty);

            if (userId == Guid.Empty)
                return Unauthorized();

            var comparisonViewModel = await _comparisonService.GetComparisonDetailsAsync(id, userId);

            if (comparisonViewModel == null)
                return NotFound();

            return View(comparisonViewModel);
        }

        /// <summary>
        /// Връща изглед за създаване на нов набор за сравнение
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Обработва формата за създаване на нов набор за сравнение
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Моля, въведете име на набора за сравнение");
                return View();
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value ??
                                    User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ??
                                    string.Empty);

            if (userId == Guid.Empty)
                return Unauthorized();

            var setId = await _comparisonService.CreateComparisonSetAsync(name, userId);

            return RedirectToAction(nameof(Details), new { id = setId });
        }

        /// <summary>
        /// AJAX метод за добавяне на училище към набор за сравнение
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int comparisonSetId, int schoolId, int? profileId)
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ??
                                    User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ??
                                    string.Empty);

            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _comparisonService.AddItemToComparisonAsync(comparisonSetId, schoolId, profileId, userId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = result });
            }

            if (result)
                return RedirectToAction(nameof(Details), new { id = comparisonSetId });
            else
                return NotFound();
        }

        /// <summary>
        /// Изтрива набор за сравнение
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ??
                                    User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ??
                                    string.Empty);

            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _comparisonService.DeleteComparisonSetAsync(id, userId);

            if (result)
                return RedirectToAction(nameof(Index));
            else
                return NotFound();
        }

        /// <summary>
        /// Премахва елемент от набор за сравнение
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int itemId, int comparisonSetId)
        {
            

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);

            // Взимаме сравнението с неговите елементи
            var comparisonSet = await _unitOfWork.ComparisonSets.GetComparisonSetWithItemsAsync(comparisonSetId);

            // Проверяваме дали сравнението съществува и принадлежи на текущия потребител
            if (comparisonSet == null || comparisonSet.UserId != userId)
            {
                
                return NotFound();
            }

            // Намираме елемента, който трябва да премахнем
            var itemToRemove = comparisonSet.Items.FirstOrDefault(i => i.Id == itemId);
            if (itemToRemove == null)
            {
               
                return NotFound();
            }

            // Премахваме елемента от колекцията
            comparisonSet.Items.Remove(itemToRemove);

            // Запазваме промените
            await _unitOfWork.CompleteAsync();

            // Пренасочваме обратно към страницата с детайли
            return RedirectToAction(nameof(Details), new { id = comparisonSetId });
        }
    }
}