﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Common;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Web.Controllers
{
    [Authorize]
    public class UserGradesController : Controller
    {
        private readonly IUserGradesService _userGradesService;
        private readonly ILogger<UserGradesController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdmissionService _admissionService;


        public UserGradesController(IUserGradesService userGradesService, ILogger<UserGradesController> logger, IUnitOfWork unitOfWork, IAdmissionService admissionService)
        {
            _userGradesService = userGradesService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _admissionService = admissionService;
        }

        // GET: UserGrades
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var userGrades = await _userGradesService.GetUserGradesAsync(userId);
            return View(userGrades);
        }

        // GET: UserGrades/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var grades = await _userGradesService.GetUserGradeByIdAsync(id);
            if (grades == null)
            {
                return NotFound();
            }

            return View(grades);
        }

        // GET: UserGrades/Create
        public IActionResult Create()
        {
            ViewBag.SubjectCodes = SubjectCodes.SubjectNames;
            ViewBag.ComponentTypes = new Dictionary<int, string>
            {
                { ComponentTypes.YearlyGrade, "Годишна оценка" },
                { ComponentTypes.NationalExam, "НВО" },
                { ComponentTypes.EntranceExam, "Приемен изпит" },
                { ComponentTypes.PhysicsOlympiad60Percent, "Олимпиада по физика (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив физик" },
                { ComponentTypes.BiologyOlympiad60Percent, "Олимпиада по биология (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив биолог" },
                { ComponentTypes.ChemistryOlympiad60Percent, "Олимпиада по химия (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив химик" },
                { ComponentTypes.ChakalovTalentedBiologist, "Акад. Л. Чакалов - модул Талантлив биолог" },
                { ComponentTypes.ChakalovTalentedChemist, "Акад. Л. Чакалов - модул Талантлив химик" },
                { ComponentTypes.GeographyOlympiad60Percent, "Олимпиада по география (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив географ" }
            };
            return View(new UserGradesInputModel());
        }

        // POST: UserGrades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserGradesInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    await _userGradesService.CreateUserGradesAsync(model, userId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Грешка при създаване на набор от оценки");
                    ModelState.AddModelError("", "Възникна грешка при запазване на оценките. Моля опитайте отново.");
                }
            }

            ViewBag.SubjectCodes = SubjectCodes.SubjectNames;
            ViewBag.ComponentTypes = new Dictionary<int, string>
            {
                { ComponentTypes.YearlyGrade, "Годишна оценка" },
                { ComponentTypes.NationalExam, "НВО" },
                { ComponentTypes.EntranceExam, "Приемен изпит" },
                { ComponentTypes.PhysicsOlympiad60Percent, "Олимпиада по физика (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив физик" },
                { ComponentTypes.BiologyOlympiad60Percent, "Олимпиада по биология (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив биолог" },
                { ComponentTypes.ChemistryOlympiad60Percent, "Олимпиада по химия (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив химик" },
                { ComponentTypes.ChakalovTalentedBiologist, "Акад. Л. Чакалов - модул Талантлив биолог" },
                { ComponentTypes.ChakalovTalentedChemist, "Акад. Л. Чакалов - модул Талантлив химик" },
                { ComponentTypes.GeographyOlympiad60Percent, "Олимпиада по география (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив географ" }
            };
            return View(model);
        }

        // GET: UserGrades/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var grades = await _userGradesService.GetUserGradeByIdAsync(id);
            if (grades == null)
            {
                return NotFound();
            }

            var model = new UserGradesInputModel
            {
                ConfigurationName = grades.ConfigurationName,
                BulgarianGrade = grades.BulgarianGrade,
                MathGrade = grades.MathGrade,
                BulgarianExamPoints = grades.BulgarianExamPoints,
                MathExamPoints = grades.MathExamPoints,
                AdditionalGrades = grades.AdditionalGrades.Select(ag => new UserAdditionalGradeInputModel
                {
                    SubjectCode = ag.SubjectCode,
                    SubjectName = ag.SubjectName,
                    ComponentType = ag.ComponentType,
                    Value = ag.Value
                }).ToList()
            };

            ViewBag.SubjectCodes = SubjectCodes.SubjectNames;
            ViewBag.ComponentTypes = new Dictionary<int, string>
            {
                { ComponentTypes.YearlyGrade, "Годишна оценка" },
                { ComponentTypes.NationalExam, "НВО" },
                { ComponentTypes.EntranceExam, "Приемен изпит" },
                { ComponentTypes.PhysicsOlympiad60Percent, "Олимпиада по физика (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив физик" },
                { ComponentTypes.BiologyOlympiad60Percent, "Олимпиада по биология (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив биолог" },
                { ComponentTypes.ChemistryOlympiad60Percent, "Олимпиада по химия (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив химик" },
                { ComponentTypes.ChakalovTalentedBiologist, "Акад. Л. Чакалов - модул Талантлив биолог" },
                { ComponentTypes.ChakalovTalentedChemist, "Акад. Л. Чакалов - модул Талантлив химик" },
                { ComponentTypes.GeographyOlympiad60Percent, "Олимпиада по география (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив географ" }
            };
            return View(model);
        }

        // POST: UserGrades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserGradesInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userGradesService.UpdateUserGradesAsync(model, id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Грешка при редактиране на набор от оценки");
                    ModelState.AddModelError("", "Възникна грешка при запазване на оценките. Моля опитайте отново.");
                }
            }

            ViewBag.SubjectCodes = SubjectCodes.SubjectNames;
            ViewBag.ComponentTypes = new Dictionary<int, string>
            {
                { ComponentTypes.YearlyGrade, "Годишна оценка" },
                { ComponentTypes.NationalExam, "НВО" },
                { ComponentTypes.EntranceExam, "Приемен изпит" },
                { ComponentTypes.PhysicsOlympiad60Percent, "Олимпиада по физика (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив физик" },
                { ComponentTypes.BiologyOlympiad60Percent, "Олимпиада по биология (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив биолог" },
                { ComponentTypes.ChemistryOlympiad60Percent, "Олимпиада по химия (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив химик" },
                { ComponentTypes.ChakalovTalentedBiologist, "Акад. Л. Чакалов - модул Талантлив биолог" },
                { ComponentTypes.ChakalovTalentedChemist, "Акад. Л. Чакалов - модул Талантлив химик" },
                { ComponentTypes.GeographyOlympiad60Percent, "Олимпиада по география (областен кръг 60%+) / Акад. Л. Чакалов - Талантлив географ" }
            };
            return View(model);
        }

        // GET: UserGrades/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var grades = await _userGradesService.GetUserGradeByIdAsync(id);
            if (grades == null)
            {
                return NotFound();
            }

            return View(grades);
        }

        // POST: UserGrades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _userGradesService.DeleteUserGradesAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: UserGrades/Calculate
        public async Task<IActionResult> Calculate(int? schoolId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var userGrades = await _userGradesService.GetUserGradesAsync(userId);

            if (!userGrades.Any())
            {
                TempData["Message"] = "Първо трябва да въведете вашите оценки.";
                return RedirectToAction(nameof(Create));
            }

            ViewBag.UserGrades = userGrades;

            if (schoolId.HasValue)
            {
                ViewBag.SchoolId = schoolId.Value;
            }

            return View();
        }

        // POST: UserGrades/CalculateChance
        [HttpPost]
        public async Task<IActionResult> CalculateChance(int gradesId, int schoolId)
        {
            try
            {
                // Проверка дали schoolId е валидно
                if (schoolId <= 0)
                {
                    TempData["ErrorMessage"] = "Моля, изберете валидно училище.";
                    return RedirectToAction(nameof(Calculate));
                }

                // Проверка дали училището съществува
                var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);

                // Ако не съществува, отново редирект с подходящо съобщение
                if (school == null)
                {
                    TempData["ErrorMessage"] = "Избраното училище не е намерено. Моля, изберете друго.";
                    return RedirectToAction(nameof(Calculate));
                }

                var result = await _admissionService.CalculateChanceAsync(gradesId, schoolId);
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при изчисляване на шанс за прием");
                TempData["ErrorMessage"] = "Възникна грешка при изчисляване на шансовете. Моля опитайте отново.";
                return RedirectToAction(nameof(Calculate), new { schoolId });
            }
        }
        
    }
}