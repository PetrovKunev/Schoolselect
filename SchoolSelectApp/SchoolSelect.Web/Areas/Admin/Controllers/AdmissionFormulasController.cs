using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSelect.Common;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Areas.Admin.ViewModels;

namespace SchoolSelect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class AdmissionFormulasController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdmissionFormulasController> _logger;

        public AdmissionFormulasController(IUnitOfWork unitOfWork, ILogger<AdmissionFormulasController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: /Admin/AdmissionFormulas
        public async Task<IActionResult> Index()
        {
            var schools = await _unitOfWork.Schools.GetSchoolsWithProfilesAsync();
            return View(schools);
        }

        // GET: /Admin/AdmissionFormulas/Profile/5
        public async Task<IActionResult> Profile(int id)
        {
            var profile = await _unitOfWork.SchoolProfiles.GetProfileWithAdmissionFormulasAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            var school = await _unitOfWork.Schools.GetByIdAsync(profile.SchoolId);

            var viewModel = new ProfileFormulasViewModel
            {
                ProfileId = profile.Id,
                ProfileName = profile.Name,
                SchoolId = school.Id,
                SchoolName = school.Name,
                Formulas = profile.AdmissionFormulas.Select(f => new AdmissionFormulaViewModel
                {
                    Id = f.Id,
                    SchoolProfileId = f.SchoolProfileId,
                    Year = f.Year,
                    FormulaExpression = f.FormulaExpression,
                    FormulaDescription = f.FormulaDescription,
                    Components = f.Components.Select(c => new FormulaComponentViewModel
                    {
                        Id = c.Id,
                        AdmissionFormulaId = c.AdmissionFormulaId,
                        SubjectCode = c.SubjectCode,
                        SubjectName = c.SubjectName,
                        ComponentType = c.ComponentType,
                        Multiplier = c.Multiplier,
                        Description = c.Description
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: /Admin/AdmissionFormulas/Create/5
        public async Task<IActionResult> Create(int profileId)
        {
            var profile = await _unitOfWork.SchoolProfiles.GetByIdAsync(profileId);
            if (profile == null)
            {
                return NotFound();
            }

            var school = await _unitOfWork.Schools.GetByIdAsync(profile.SchoolId);

            var viewModel = new AdmissionFormulaViewModel
            {
                SchoolProfileId = profileId,
                SchoolName = school.Name,
                ProfileName = profile.Name,
                Year = DateTime.Now.Year,
                Components = new List<FormulaComponentViewModel>()
            };

            // Add default components for BEL and MAT
            viewModel.Components.Add(new FormulaComponentViewModel
            {
                SubjectCode = "БЕЛ",
                SubjectName = "Български език и литература",
                ComponentType = ComponentTypes.YearlyGrade,
                Multiplier = 1,
                Description = "Годишна оценка по БЕЛ от 7-ми клас"
            });

            viewModel.Components.Add(new FormulaComponentViewModel
            {
                SubjectCode = "БЕЛ",
                SubjectName = "Български език и литература",
                ComponentType = ComponentTypes.NationalExam,
                Multiplier = 1,
                Description = "Национално външно оценяване по БЕЛ"
            });

            viewModel.Components.Add(new FormulaComponentViewModel
            {
                SubjectCode = "МАТ",
                SubjectName = "Математика",
                ComponentType = ComponentTypes.YearlyGrade,
                Multiplier = 1,
                Description = "Годишна оценка по математика от 7-ми клас"
            });

            viewModel.Components.Add(new FormulaComponentViewModel
            {
                SubjectCode = "МАТ",
                SubjectName = "Математика",
                ComponentType = ComponentTypes.NationalExam,
                Multiplier = 1,
                Description = "Национално външно оценяване по математика"
            });

            // Define the common subjects for selection
            ViewBag.ComponentTypes = new SelectList(new[]
            {
                new { Value = ComponentTypes.YearlyGrade, Text = "Годишна оценка" },
                new { Value = ComponentTypes.NationalExam, Text = "НВО" },
                new { Value = ComponentTypes.EntranceExam, Text = "Приемен изпит" },
                new { Value = ComponentTypes.YearlyGradeAsPoints, Text = "Годишна оценка, преобразувана в точки" }
            }, "Value", "Text");

            ViewBag.SubjectCodes = SubjectCodes.SubjectNames;

            return View(viewModel);
        }

        // POST: /Admin/AdmissionFormulas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdmissionFormulaViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create the formula
                    var formula = new AdmissionFormula
                    {
                        SchoolProfileId = model.SchoolProfileId,
                        Year = model.Year,
                        FormulaExpression = model.FormulaExpression,
                        FormulaDescription = model.FormulaDescription,
                        HasComponents = model.Components != null && model.Components.Any() // Задаваме HasComponents на true ако има компоненти
                    };

                    // Add all components
                    if (model.Components != null) // Ensure Components is not null
                    {
                        foreach (var componentModel in model.Components)
                        {
                            formula.Components.Add(new FormulaComponent
                            {
                                SubjectCode = componentModel.SubjectCode,
                                SubjectName = componentModel.SubjectName,
                                ComponentType = componentModel.ComponentType,
                                Multiplier = componentModel.Multiplier,
                                Description = componentModel.Description
                            });
                        }
                    }

                    await _unitOfWork.AdmissionFormulas.AddAsync(formula);
                    await _unitOfWork.CompleteAsync();

                    TempData["SuccessMessage"] = "Формулата беше успешно създадена.";
                    return RedirectToAction(nameof(Profile), new { id = model.SchoolProfileId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating admission formula");
                    ModelState.AddModelError("", "Възникна грешка при създаване на формулата. Моля опитайте отново.");
                }
            }

            // If we got here, something failed; redisplay form
            var profile = await _unitOfWork.SchoolProfiles.GetByIdAsync(model.SchoolProfileId);
            var school = await _unitOfWork.Schools.GetByIdAsync(profile.SchoolId);
            model.SchoolName = school.Name;
            model.ProfileName = profile.Name;

            ViewBag.ComponentTypes = new SelectList(new[]
            {
                new { Value = ComponentTypes.YearlyGrade, Text = "Годишна оценка" },
                new { Value = ComponentTypes.NationalExam, Text = "НВО" },
                new { Value = ComponentTypes.EntranceExam, Text = "Приемен изпит" },
                new { Value = ComponentTypes.YearlyGradeAsPoints, Text = "Годишна оценка, преобразувана в точки" }
            }, "Value", "Text");

            ViewBag.SubjectCodes = SubjectCodes.SubjectNames;

            return View(model);
        }

        // GET: /Admin/AdmissionFormulas/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var formula = await _unitOfWork.AdmissionFormulas.GetFormulaWithComponentsAsync(id);
            if (formula == null)
            {
                return NotFound();
            }

            var profile = await _unitOfWork.SchoolProfiles.GetByIdAsync(formula.SchoolProfileId);
            var school = await _unitOfWork.Schools.GetByIdAsync(profile.SchoolId);

            var viewModel = new AdmissionFormulaViewModel
            {
                Id = formula.Id,
                SchoolProfileId = formula.SchoolProfileId,
                SchoolName = school.Name,
                ProfileName = profile.Name,
                Year = formula.Year,
                FormulaExpression = formula.FormulaExpression,
                FormulaDescription = formula.FormulaDescription,
                Components = formula.Components.Select(c => new FormulaComponentViewModel
                {
                    Id = c.Id,
                    AdmissionFormulaId = c.AdmissionFormulaId,
                    SubjectCode = c.SubjectCode,
                    SubjectName = c.SubjectName,
                    ComponentType = c.ComponentType,
                    Multiplier = c.Multiplier,
                    Description = c.Description
                }).ToList()
            };

            ViewBag.ComponentTypes = new SelectList(new[]
            {
                new { Value = ComponentTypes.YearlyGrade, Text = "Годишна оценка" },
                new { Value = ComponentTypes.NationalExam, Text = "НВО" },
                new { Value = ComponentTypes.EntranceExam, Text = "Приемен изпит" },
                new { Value = ComponentTypes.YearlyGradeAsPoints, Text = "Годишна оценка, преобразувана в точки" }
            }, "Value", "Text");

            ViewBag.SubjectCodes = SubjectCodes.SubjectNames;

            return View(viewModel);
        }

        // POST: /Admin/AdmissionFormulas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdmissionFormulaViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var formula = await _unitOfWork.AdmissionFormulas.GetFormulaWithComponentsAsync(id);
                    if (formula == null)
                    {
                        return NotFound();
                    }

                    // Update formula properties
                    formula.Year = model.Year;
                    formula.FormulaExpression = model.FormulaExpression;
                    formula.FormulaDescription = model.FormulaDescription;
                    formula.HasComponents = model.Components != null && model.Components.Any(); // Обновяваме и HasComponents

                    // Remove all existing components
                    formula.Components.Clear();

                    // Add all components from the model
                    if (model.Components != null)
                    {     // Ensure Components is not null
                        foreach (var componentModel in model.Components)
                        {
                            formula.Components.Add(new FormulaComponent
                            {
                                SubjectCode = componentModel.SubjectCode,
                                SubjectName = componentModel.SubjectName,
                                ComponentType = componentModel.ComponentType,
                                Multiplier = componentModel.Multiplier,
                                Description = componentModel.Description
                            });
                        }
                    }

                    _unitOfWork.AdmissionFormulas.Update(formula);
                    await _unitOfWork.CompleteAsync();

                    TempData["SuccessMessage"] = "Формулата беше успешно обновена.";
                    return RedirectToAction(nameof(Profile), new { id = model.SchoolProfileId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating admission formula");
                    ModelState.AddModelError("", "Възникна грешка при обновяване на формулата. Моля опитайте отново.");
                }
            }

            // If we got here, something failed; redisplay form
            var profile = await _unitOfWork.SchoolProfiles.GetByIdAsync(model.SchoolProfileId);
            var school = await _unitOfWork.Schools.GetByIdAsync(profile.SchoolId);
            model.SchoolName = school.Name;
            model.ProfileName = profile.Name;

            ViewBag.ComponentTypes = new SelectList(new[]
            {
                new { Value = ComponentTypes.YearlyGrade, Text = "Годишна оценка" },
                new { Value = ComponentTypes.NationalExam, Text = "НВО" },
                new { Value = ComponentTypes.EntranceExam, Text = "Приемен изпит" },
                new { Value = ComponentTypes.YearlyGradeAsPoints, Text = "Годишна оценка, преобразувана в точки" }
            }, "Value", "Text");

            ViewBag.SubjectCodes = SubjectCodes.SubjectNames;

            return View(model);
        }

        // POST: /Admin/AdmissionFormulas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var formula = await _unitOfWork.AdmissionFormulas.GetByIdAsync(id);
            if (formula == null)
            {
                return NotFound();
            }

            var profileId = formula.SchoolProfileId;

            _unitOfWork.AdmissionFormulas.Remove(formula);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "Формулата беше успешно изтрита.";
            return RedirectToAction(nameof(Profile), new { id = profileId });
        }

        // This action will handle adding more components via AJAX
        [HttpPost]
        public IActionResult AddComponent()
        {
            ViewBag.ComponentTypes = new SelectList(new[]
            {
                new { Value = ComponentTypes.YearlyGrade, Text = "Годишна оценка" },
                new { Value = ComponentTypes.NationalExam, Text = "НВО" },
                new { Value = ComponentTypes.EntranceExam, Text = "Приемен изпит" },
                new { Value = ComponentTypes.YearlyGradeAsPoints , Text = "Годишна оценка, преобразувана в точки" }
            }, "Value", "Text");

            ViewBag.SubjectCodes = SubjectCodes.SubjectNames;

            // Get the index from the request - needed for proper form field names
            if (Request.Form.TryGetValue("index", out var indexValue) && int.TryParse(indexValue, out int index))
            {
                ViewBag.Index = index;
            }
            else
            {
                ViewBag.Index = 0;
            }

            // Create a new component with default values
            var componentModel = new FormulaComponentViewModel
            {
                Multiplier = 1.0, // Default multiplier
                ComponentType = ComponentTypes.YearlyGrade, // Default to yearly grade
                SubjectCode = "БЕЛ", // Default subject
                SubjectName = "Български език и литература" // Default subject name
            };

            return PartialView("_FormulaComponentPartial", componentModel);
        }
    }
}