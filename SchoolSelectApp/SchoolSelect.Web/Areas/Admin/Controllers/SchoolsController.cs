// SchoolSelect.Web/Areas/Admin/Controllers/SchoolsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Areas.Admin.ViewModels;
using System.Threading.Tasks;

namespace SchoolSelect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SchoolsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SchoolsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Admin/Schools
        public async Task<IActionResult> Index()
        {
            var schools = await _unitOfWork.Schools.GetAllAsync();
            return View(schools);
        }

        // GET: Admin/Schools/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var school = await _unitOfWork.Schools.GetSchoolWithDetailsAsync(id);
            if (school == null)
            {
                return NotFound();
            }

            return View(school);
        }

        // GET: Admin/Schools/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Schools/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SchoolEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var school = new School
                {
                    Name = model.Name,
                    Address = model.Address,
                    District = model.District,
                    City = model.City,
                    Phone = model.Phone,
                    Email = model.Email,
                    Website = model.Website,
                    Description = model.Description
                };

                await _unitOfWork.Schools.AddAsync(school);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/Schools/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(id);
            if (school == null)
            {
                return NotFound();
            }

            var viewModel = new SchoolEditViewModel
            {
                Id = school.Id,
                Name = school.Name,
                Address = school.Address,
                District = school.District,
                City = school.City,
                Phone = school.Phone,
                Email = school.Email,
                Website = school.Website,
                Description = school.Description,
                GeoLatitude = school.GeoLatitude,
                GeoLongitude = school.GeoLongitude,
                MapsFormattedAddress = school.MapsFormattedAddress
            };

            return View(viewModel);
        }

        // POST: Admin/Schools/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SchoolEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var school = await _unitOfWork.Schools.GetByIdAsync(id);
                    if (school == null)
                    {
                        return NotFound();
                    }

                    school.Name = model.Name;
                    school.Address = model.Address;
                    school.District = model.District;
                    school.City = model.City;
                    school.Phone = model.Phone;
                    school.Email = model.Email;
                    school.Website = model.Website;
                    school.Description = model.Description;
                    school.GeoLatitude = model.GeoLatitude;
                    school.GeoLongitude = model.GeoLongitude;
                    school.MapsFormattedAddress = model.MapsFormattedAddress;
                    school.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.Schools.Update(school);
                    await _unitOfWork.CompleteAsync();

                    TempData["SuccessMessage"] = "Училището е успешно обновено.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Възникна грешка при обновяване на училището: {ex.Message}");
                }
            }
            return View(model);
        }

        // GET: Admin/Schools/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(id);
            if (school == null)
            {
                return NotFound();
            }

            return View(school);
        }

        // POST: Admin/Schools/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(id);
            if (school != null)
            {
                _unitOfWork.Schools.Remove(school);
                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] = "Училището е успешно изтрито.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}