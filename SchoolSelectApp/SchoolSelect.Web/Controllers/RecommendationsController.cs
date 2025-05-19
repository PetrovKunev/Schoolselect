using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Web.Controllers
{
    [Authorize]
    public class RecommendationsController : Controller
    {
        private readonly ISchoolRecommendationService _recommendationService;
        private readonly ILogger<RecommendationsController> _logger;

        public RecommendationsController(
            ISchoolRecommendationService recommendationService,
            ILogger<RecommendationsController> logger)
        {
            _recommendationService = recommendationService;
            _logger = logger;
        }

        // GET: Recommendations
        [HttpGet]
        public async Task<IActionResult> Index(int preferenceId, int? gradesId = null)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                var recommendations = await _recommendationService.GetRecommendationsAsync(preferenceId, userId, gradesId);
                return View(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for preference {PreferenceId}", preferenceId);
                TempData["ErrorMessage"] = "Възникна грешка при изчисляване на препоръки. Моля, опитайте отново.";
                return RedirectToAction("Index", "UserPreferences");
            }
        }
    }
}