using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.ViewModels;
using System.Security.Claims;

namespace SchoolSelect.Web.ViewComponents
{
    public class AddToComparisonViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddToComparisonViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync(int schoolId, int? profileId = null)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Content(string.Empty);
            }

            var claimsPrincipal = User as ClaimsPrincipal;
            var userId = Guid.Parse(claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
            if (userId == Guid.Empty)
            {
                return Content(string.Empty);
            }

            var userComparisonSets = await _unitOfWork.ComparisonSets.GetComparisonSetsByUserIdAsync(userId);

            var viewModel = new AddToComparisonViewModel
            {
                SchoolId = schoolId,
                ProfileId = profileId,
                UserComparisonSets = userComparisonSets
            };

            return View(viewModel);
        }
    }
}