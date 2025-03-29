using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Data.Models;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class ReviewModerationController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewModerationController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: /Admin/ReviewModeration
        public async Task<IActionResult> Index()
        {
            var pendingReviews = await _reviewService.GetPendingReviewsAsync();
            return View(pendingReviews);
        }

        // GET: /Admin/ReviewModeration/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var allPendingReviews = await _reviewService.GetPendingReviewsAsync();
            var review = allPendingReviews.FirstOrDefault(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: /Admin/ReviewModeration/Approve/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            await _reviewService.ApproveReviewAsync(id);
            TempData["SuccessMessage"] = "Отзивът беше одобрен успешно.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/ReviewModeration/Reject/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            await _reviewService.RejectReviewAsync(id);
            TempData["SuccessMessage"] = "Отзивът беше отхвърлен успешно.";
            return RedirectToAction(nameof(Index));
        }
    }
}