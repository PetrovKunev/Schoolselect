// SchoolProfilesController.cs в Admin областта
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Common;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Areas.Admin.ViewModels;

namespace SchoolSelect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class SchoolProfilesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SchoolProfilesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Admin/SchoolProfiles/Index
        public async Task<IActionResult> Index()
        {
            var schools = await _unitOfWork.Schools.GetAllAsync();
            return View(schools);
        }

        // GET: /Admin/SchoolProfiles/Manage/5
        public async Task<IActionResult> Manage(int id)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(id);
            if (school == null)
            {
                return NotFound();
            }

            var profiles = await _unitOfWork.SchoolProfiles.GetProfilesBySchoolIdAsync(id);

            var viewModel = new SchoolProfilesManageViewModel
            {
                School = school,
                Profiles = profiles.ToList()
            };

            return View(viewModel);
        }

        // GET: /Admin/SchoolProfiles/Create/5
        public async Task<IActionResult> Create(int schoolId)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school == null)
            {
                return NotFound();
            }

            var viewModel = new SchoolProfileCreateViewModel
            {
                SchoolId = schoolId,
                SchoolName = school.Name
            };

            return View(viewModel);
        }

        // POST: /Admin/SchoolProfiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SchoolProfileCreateViewModel model)
        {
            if (model.Type == ProfileType.Професионална)
            {
                if (string.IsNullOrEmpty(model.Specialty))
                {
                    ModelState.AddModelError("Specialty", "Полето Специалност е задължително за професионалните паралелки.");
                }
                if (string.IsNullOrEmpty(model.ProfessionalQualification))
                {
                    ModelState.AddModelError("ProfessionalQualification", "Полето Професионална квалификация е задължително за професионалните паралелки.");
                }
            }

            if (ModelState.IsValid)
            {
                var profile = new SchoolProfile
                {
                    SchoolId = model.SchoolId,
                    Name = model.Name,
                    Code = model.Code,
                    Description = string.IsNullOrEmpty(model.Description) ? string.Empty : model.Description,
                    Subjects = string.IsNullOrEmpty(model.Subjects) ? string.Empty : model.Subjects,
                    AvailablePlaces = model.AvailablePlaces,
                    Type = model.Type,
                    Specialty = model.Specialty,
                    ProfessionalQualification = model.ProfessionalQualification
                };

                await _unitOfWork.SchoolProfiles.AddAsync(profile);
                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] = "Профилът беше успешно създаден.";
                return RedirectToAction(nameof(Manage), new { id = model.SchoolId });
            }

            var school = await _unitOfWork.Schools.GetByIdAsync(model.SchoolId);
            model.SchoolName = school?.Name ?? "Неизвестно училище";

            return View(model);
        }

        // GET: /Admin/SchoolProfiles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var profile = await _unitOfWork.SchoolProfiles.GetByIdAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            var school = await _unitOfWork.Schools.GetByIdAsync(profile.SchoolId);

            var viewModel = new SchoolProfileEditViewModel
            {
                Id = profile.Id,
                SchoolId = profile.SchoolId,
                SchoolName = school?.Name ?? "Неизвестно училище",
                Name = profile.Name,
                Code = profile.Code,
                Description = profile.Description,
                Subjects = profile.Subjects,
                AvailablePlaces = profile.AvailablePlaces,
                Type = profile.Type,
                Specialty = profile.Specialty ?? string.Empty,
                ProfessionalQualification = profile.ProfessionalQualification ?? string.Empty
            };

            return View(viewModel);
        }

        // POST: /Admin/SchoolProfiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SchoolProfileEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var profile = await _unitOfWork.SchoolProfiles.GetByIdAsync(model.Id);
                if (profile == null)
                {
                    return NotFound();
                }

                profile.Name = model.Name;
                profile.Code = model.Code;
                profile.Description = string.IsNullOrEmpty(model.Description) ? string.Empty : model.Description;
                profile.Subjects = string.IsNullOrEmpty(model.Subjects) ? string.Empty : model.Subjects;
                profile.AvailablePlaces = model.AvailablePlaces;
                profile.Type = model.Type;
                profile.Specialty = model.Specialty;
                profile.ProfessionalQualification = model.ProfessionalQualification;
                profile.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.SchoolProfiles.Update(profile);
                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] = "Профилът беше успешно обновен.";
                return RedirectToAction(nameof(Manage), new { id = profile.SchoolId });
            }

            var school = await _unitOfWork.Schools.GetByIdAsync(model.SchoolId);
            model.SchoolName = school?.Name ?? "Неизвестно училище";

            return View(model);
        }

        // POST: /Admin/SchoolProfiles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var profile = await _unitOfWork.SchoolProfiles.GetByIdAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            var schoolId = profile.SchoolId;

            _unitOfWork.SchoolProfiles.Remove(profile);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "Профилът беше успешно изтрит.";
            return RedirectToAction(nameof(Manage), new { id = schoolId });
        }
    }
}