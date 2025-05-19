using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Common;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.ViewModels;
using FormulaComponentViewModel = SchoolSelect.Web.ViewModels.FormulaComponentViewModel;

namespace SchoolSelect.Web.Controllers
{
    public class SchoolsController : Controller
    {
        private readonly ILogger<SchoolsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public SchoolsController(ILogger<SchoolsController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string searchTerm = "", string district = "", string profileType = "")
        {
            var schools = string.IsNullOrEmpty(searchTerm)
                ? await _unitOfWork.Schools.GetAllAsync()
                : await _unitOfWork.Schools.SearchSchoolsAsync(searchTerm);

            // Filter by district if provided
            if (!string.IsNullOrEmpty(district))
            {
                schools = schools.Where(s => s.District.Equals(district, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by profile type if provided
            if (!string.IsNullOrEmpty(profileType))
            {
                // This will need to join with profiles data
                schools = await _unitOfWork.Schools.GetSchoolsByProfileTypeAsync(profileType);
            }

            // Convert to view models
            var schoolViewModels = schools.Select(s => new SchoolViewModel
            {
                Id = s.Id,
                Name = s.Name,
                District = s.District,
                City = s.City,
                AverageRating = s.AverageRating,
                ReviewsCount = s.RatingsCount
            }).ToList();

            // Get all districts for the filter dropdown
            var allDistricts = schools.Select(s => s.District).Distinct().OrderBy(d => d).ToList();

            var model = new SchoolsListViewModel
            {
                Schools = schoolViewModels,
                SearchTerm = searchTerm,
                SelectedDistrict = district,
                SelectedProfileType = profileType,
                Districts = allDistricts
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var school = await _unitOfWork.Schools.GetSchoolWithDetailsAsync(id);

            if (school == null)
            {
                return NotFound();
            }

            var profiles = await _unitOfWork.SchoolProfiles.GetProfilesBySchoolIdAsync(school.Id);
            var facilities = await _unitOfWork.SchoolFacilities.GetFacilitiesBySchoolIdAsync(school.Id);
            var transportFacilities = await _unitOfWork.SchoolFacilities.GetTransportFacilitiesBySchoolIdAsync(school.Id);
            var reviews = await _unitOfWork.Reviews.GetReviewsBySchoolIdAsync(school.Id);
            var rankings = await _unitOfWork.HistoricalRankings.GetRankingsBySchoolIdAsync(school.Id);

            var schoolViewModel = new SchoolDetailsViewModel
            {
                Id = school.Id,
                Name = school.Name,
                Address = school.Address,
                District = school.District,
                City = school.City,
                Phone = school.Phone,
                Email = school.Email,
                Website = school.Website,
                Description = school.Description ?? string.Empty,
                AverageRating = school.AverageRating,
                ReviewsCount = school.RatingsCount,
                Latitude = school.GeoLatitude,
                Longitude = school.GeoLongitude,
                Profiles = profiles.ToList(),
                Facilities = facilities.ToList(),
                TransportFacilities = transportFacilities.ToList(),
                Reviews = reviews.ToList(),
                Rankings = rankings.OrderByDescending(r => r.Year).ThenBy(r => r.Round).ToList()
            };

            // Добавете тук проверката за автентикация и подготовката на модела за сравнение
            if (User.Identity?.IsAuthenticated == true)
            {
                ViewBag.AddToComparisonViewModel = await PrepareAddToComparisonViewModel(id);
            }

            return View(schoolViewModel);
        }

        
        [HttpGet]
        public async Task<IActionResult> ViewFormula(int profileId)
        {
            var profile = await _unitOfWork.SchoolProfiles.GetProfileWithAdmissionFormulasAsync(profileId);

            if (profile == null)
            {
                return NotFound();
            }

            var currentFormula = profile.AdmissionFormulas
                .OrderByDescending(f => f.Year)
                .FirstOrDefault();

            if (currentFormula == null)
            {
                return View("NoFormula", profile);
            }

            var viewModel = new FormulaDisplayViewModel
            {
                ProfileId = profile.Id,
                ProfileName = profile.Name,
                SchoolName = profile.School?.Name ?? "Unknown School",
                Year = currentFormula.Year,
                FormulaExpression = currentFormula.FormulaExpression,
                FormulaDescription = currentFormula.FormulaDescription,
                Components = currentFormula.Components.Select(c => new FormulaComponentViewModel
                {
                    SubjectCode = c.SubjectCode,
                    SubjectName = c.SubjectName,
                    ComponentType = c.ComponentType,
                    Multiplier = c.Multiplier,
                    Description = c.Description
                }).ToList()
            };

            return View(viewModel);
        }

        private async Task<SchoolSelect.ViewModels.AddToComparisonViewModel> PrepareAddToComparisonViewModel(int schoolId, int? profileId = null)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var userComparisonSets = await _unitOfWork.ComparisonSets.GetComparisonSetsByUserIdAsync(userId);

            ViewBag.MaxComparisonItems = ValidationConstants.Comparison.MaxItems;

            return new SchoolSelect.ViewModels.AddToComparisonViewModel
            {
                SchoolId = schoolId,
                ProfileId = profileId,
                UserComparisonSets = userComparisonSets
            };
        }
    }
}
