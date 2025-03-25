using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Web.Controllers.Api
{
    [Route("api/schools")]
    [ApiController]
    public class SchoolsApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SchoolsApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/schools/search?term=text
        [HttpGet("search")]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
            {
                return Ok(new List<SchoolViewModel>());
            }

            var schools = await _unitOfWork.Schools.SearchSchoolsAsync(term, 10);
            var result = schools.Select(s => new SchoolViewModel
            {
                Id = s.Id,
                Name = s.Name,
                District = s.District,
                City = s.City,
                AverageRating = s.AverageRating,
                ReviewsCount = s.RatingsCount
            }).ToList();

            return Ok(result);
        }

        // GET: api/schools/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(id);
            if (school == null)
            {
                return NotFound();
            }

            var result = new SchoolViewModel
            {
                Id = school.Id,
                Name = school.Name,
                District = school.District,
                City = school.City,
                AverageRating = school.AverageRating,
                ReviewsCount = school.RatingsCount
            };

            return Ok(result);
        }
    }
}