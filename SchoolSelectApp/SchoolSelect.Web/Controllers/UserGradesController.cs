using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Common;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Web.Controllers
{
    [Authorize]
    public class UserGradesController : Controller
    {
        private readonly IUserGradesService _userGradesService;
        private readonly ILogger<UserGradesController> _logger;

        public UserGradesController(IUserGradesService userGradesService, ILogger<UserGradesController> logger)
        {
            _userGradesService = userGradesService;
            _logger = logger;
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
                { ComponentTypes.OtherComponent, "Друго" }
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
                { ComponentTypes.OtherComponent, "Друго" }
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
                { ComponentTypes.OtherComponent, "Друго" }
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
                { ComponentTypes.OtherComponent, "Друго" }
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
                var result = await _userGradesService.CalculateChanceAsync(gradesId, schoolId);
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