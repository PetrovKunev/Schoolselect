using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.ViewModels;

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
                Description = school.Description,
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

            return View(schoolViewModel);
        }
    }
}
