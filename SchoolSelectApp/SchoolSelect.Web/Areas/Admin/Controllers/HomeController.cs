using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class HomeController : Controller
    {
        private readonly IReviewService _reviewService;

        public HomeController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index()
        {
            var pendingReviews = await _reviewService.GetPendingReviewsAsync();
            ViewBag.PendingReviewsCount = pendingReviews.Count();

            return View();
        }
    }
}