// RankingsController.cs в Admin областта
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Areas.Admin.ViewModels;

namespace SchoolSelect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class RankingsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public RankingsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Admin/Rankings/Index/5
        public async Task<IActionResult> Index(int schoolId)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school == null)
            {
                return NotFound();
            }

            var rankings = await _unitOfWork.HistoricalRankings.GetRankingsBySchoolIdAsync(schoolId);
            var profiles = await _unitOfWork.SchoolProfiles.GetProfilesBySchoolIdAsync(schoolId);

            var viewModel = new HistoricalRankingsViewModel
            {
                School = school,
                Rankings = rankings.ToList(),
                Profiles = profiles.ToList()
            };

            return View(viewModel);
        }

        // GET: /Admin/Rankings/Create/5
        public async Task<IActionResult> Create(int schoolId)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school == null)
            {
                return NotFound();
            }

            var profiles = await _unitOfWork.SchoolProfiles.GetProfilesBySchoolIdAsync(schoolId);

            ViewBag.Profiles = new SelectList(profiles, "Id", "Name");

            var viewModel = new HistoricalRankingCreateViewModel
            {
                SchoolId = schoolId,
                SchoolName = school.Name,
                Year = DateTime.Now.Year
            };

            return View(viewModel);
        }

        // POST: /Admin/Rankings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HistoricalRankingCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var ranking = new HistoricalRanking
                {
                    SchoolId = model.SchoolId,
                    ProfileId = model.ProfileId,
                    Year = model.Year,
                    MinimumScore = model.MinimumScore,
                    Round = model.Round,
                    StudentsAdmitted = model.StudentsAdmitted
                };

                await _unitOfWork.HistoricalRankings.AddAsync(ranking);
                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] = "Класирането беше успешно добавено.";
                return RedirectToAction(nameof(Index), new { schoolId = model.SchoolId });
            }

            var school = await _unitOfWork.Schools.GetByIdAsync(model.SchoolId);
            var profiles = await _unitOfWork.SchoolProfiles.GetProfilesBySchoolIdAsync(model.SchoolId);

            ViewBag.Profiles = new SelectList(profiles, "Id", "Name");
            model.SchoolName = school?.Name ?? "Неизвестно училище";

            return View(model);
        }

        // GET: /Admin/Rankings/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var ranking = await _unitOfWork.HistoricalRankings.GetByIdAsync(id);
            if (ranking == null)
            {
                return NotFound();
            }

            var school = await _unitOfWork.Schools.GetByIdAsync(ranking.SchoolId);
            var profiles = await _unitOfWork.SchoolProfiles.GetProfilesBySchoolIdAsync(ranking.SchoolId);

            ViewBag.Profiles = new SelectList(profiles, "Id", "Name", ranking.ProfileId);

            var viewModel = new HistoricalRankingEditViewModel
            {
                Id = ranking.Id,
                SchoolId = ranking.SchoolId,
                SchoolName = school?.Name ?? "Неизвестно училище",
                ProfileId = ranking.ProfileId,
                Year = ranking.Year,
                MinimumScore = ranking.MinimumScore,
                Round = ranking.Round,
                StudentsAdmitted = ranking.StudentsAdmitted
            };

            return View(viewModel);
        }

        // POST: /Admin/Rankings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HistoricalRankingEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var ranking = await _unitOfWork.HistoricalRankings.GetByIdAsync(model.Id);
                if (ranking == null)
                {
                    return NotFound();
                }

                ranking.ProfileId = model.ProfileId;
                ranking.Year = model.Year;
                ranking.MinimumScore = model.MinimumScore;
                ranking.Round = model.Round;
                ranking.StudentsAdmitted = model.StudentsAdmitted;

                _unitOfWork.HistoricalRankings.Update(ranking);
                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] = "Класирането беше успешно обновено.";
                return RedirectToAction(nameof(Index), new { schoolId = ranking.SchoolId });
            }

            var school = await _unitOfWork.Schools.GetByIdAsync(model.SchoolId);
            var profiles = await _unitOfWork.SchoolProfiles.GetProfilesBySchoolIdAsync(model.SchoolId);

            ViewBag.Profiles = new SelectList(profiles, "Id", "Name", model.ProfileId);
            model.SchoolName = school?.Name ?? "Неизвестно училище";

            return View(model);
        }

        // POST: /Admin/Rankings/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ranking = await _unitOfWork.HistoricalRankings.GetByIdAsync(id);
            if (ranking == null)
            {
                return NotFound();
            }

            var schoolId = ranking.SchoolId;

            _unitOfWork.HistoricalRankings.Remove(ranking);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "Класирането беше успешно изтрито.";
            return RedirectToAction(nameof(Index), new { schoolId });
        }
    }
}