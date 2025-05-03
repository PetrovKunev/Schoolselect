
namespace SchoolSelect.Web.ViewModels
{
    public class SchoolRecommendationViewModel
    {
        public SchoolViewModel School { get; set; } = new SchoolViewModel();
        public double TotalScore { get; set; }
        public List<CriterionScoreViewModel> CriteriaScores { get; set; } = new List<CriterionScoreViewModel>();
        public List<string> Profiles { get; set; } = new List<string>();
    }
}