
namespace SchoolSelect.Web.ViewModels
{
    public class SchoolRecommendationViewModel
    {
        public SchoolViewModel School { get; set; } = new SchoolViewModel();
        public double TotalScore { get; set; }
        public List<CriterionScoreViewModel> CriteriaScores { get; set; } = new List<CriterionScoreViewModel>();
        public List<string> Profiles { get; set; } = new List<string>();
        public double? Distance { get; set; }

        // Допълнителни свойства за съответствие с бал
        public double? CalculatedUserScore { get; set; } // Изчислен бал на ученика
        public double? MinimumSchoolScore { get; set; } // Минимален бал на училището
        public double? ScoreDifference { get; set; } // Разлика между баловете
        public int? UsedGradesId { get; set; } // ID на използвания набор от оценки
    }
}