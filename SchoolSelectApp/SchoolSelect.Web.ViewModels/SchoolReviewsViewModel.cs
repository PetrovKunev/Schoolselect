using SchoolSelect.Data.Models;

namespace SchoolSelect.Web.ViewModels
{
    public class SchoolReviewsViewModel
    {
        public School School { get; set; } = null!;
        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}
