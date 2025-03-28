// ReviewsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;
using System.Security.Claims;

namespace SchoolSelect.Web.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly ISchoolRepository _schoolRepository;

        public ReviewsController(IReviewService reviewService, ISchoolRepository schoolRepository)
        {
            _reviewService = reviewService;
            _schoolRepository = schoolRepository;
        }

        // GET: /Reviews/School/{schoolId}
        public async Task<IActionResult> School(int schoolId)
        {
            var school = await _schoolRepository.GetByIdAsync(schoolId);
            if (school == null)
            {
                return NotFound();
            }

            var reviews = await _reviewService.GetReviewsBySchoolIdAsync(schoolId);

            var viewModel = new SchoolReviewsViewModel
            {
                School = school,
                Reviews = reviews.ToList()
            };

            return View(viewModel);
        }

        // GET: /Reviews/Create/{schoolId}
        [Authorize]
        public async Task<IActionResult> Create(int schoolId)
        {
            var school = await _schoolRepository.GetByIdAsync(schoolId);
            if (school == null)
            {
                return NotFound();
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Проверка дали потребителят вече е оставил отзив
            if (await _reviewService.HasUserReviewedSchoolAsync(userId, schoolId))
            {
                TempData["ErrorMessage"] = "Вече сте публикували отзив за това училище.";
                return RedirectToAction("School", new { schoolId });
            }

            var viewModel = new CreateReviewViewModel
            {
                SchoolId = schoolId,
                SchoolName = school.Name
            };

            return View(viewModel);
        }

        // POST: /Reviews/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var review = new Review
                {
                    SchoolId = model.SchoolId,
                    UserId = userId,
                    Content = model.Content,
                    Rating = model.Rating
                };

                await _reviewService.CreateReviewAsync(review);

                TempData["SuccessMessage"] = "Вашият отзив беше изпратен за одобрение. Ще бъде публикуван след преглед от модератор.";
                return RedirectToAction("School", new { schoolId = model.SchoolId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // GET: /Reviews/Edit/{id}
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
            var review = reviews.FirstOrDefault(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            var school = await _schoolRepository.GetByIdAsync(review.SchoolId);

            var viewModel = new EditReviewViewModel
            {
                Id = review.Id,
                SchoolId = review.SchoolId,
                SchoolName = school.Name,
                Content = review.Content,
                Rating = review.Rating
            };

            return View(viewModel);
        }

        // POST: /Reviews/Edit
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
            var review = reviews.FirstOrDefault(r => r.Id == model.Id);

            if (review == null)
            {
                return NotFound();
            }

            review.Content = model.Content;
            review.Rating = model.Rating;

            await _reviewService.UpdateReviewAsync(review);

            TempData["SuccessMessage"] = "Вашият отзив беше актуализиран и изпратен за повторно одобрение.";
            return RedirectToAction("MyReviews");
        }

        // GET: /Reviews/Delete/{id}
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
            var review = reviews.FirstOrDefault(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            var school = await _schoolRepository.GetByIdAsync(review.SchoolId);

            var viewModel = new DeleteReviewViewModel
            {
                Id = review.Id,
                SchoolName = school.Name,
                Content = review.Content,
                Rating = review.Rating,
                CreatedAt = review.CreatedAt
            };

            return View(viewModel);
        }

        // POST: /Reviews/DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
            var review = reviews.FirstOrDefault(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            await _reviewService.DeleteReviewAsync(id);

            TempData["SuccessMessage"] = "Вашият отзив беше изтрит успешно.";
            return RedirectToAction("MyReviews");
        }

        // GET: /Reviews/MyReviews
        [Authorize]
        public async Task<IActionResult> MyReviews()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);

            return View(reviews);
        }
    }
}