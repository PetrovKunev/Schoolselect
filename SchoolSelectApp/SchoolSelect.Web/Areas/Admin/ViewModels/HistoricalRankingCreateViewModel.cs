using System.ComponentModel.DataAnnotations;

namespace SchoolSelect.Web.Areas.Admin.ViewModels
{
    public class HistoricalRankingCreateViewModel
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;

        public int? ProfileId { get; set; }

        [Required(ErrorMessage = "Годината е задължителна")]
        [Range(2000, 2100, ErrorMessage = "Годината трябва да бъде между 2000 и 2100")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Минималният бал е задължителен")]
        [Range(0, 100, ErrorMessage = "Минималният бал трябва да бъде между 0 и 100")]
        public double MinimumScore { get; set; }

        [Range(1, 10, ErrorMessage = "Кръгът трябва да бъде между 1 и 10")]
        public int Round { get; set; } = 1;

        [Range(0, 1000, ErrorMessage = "Броят приети ученици трябва да бъде между 0 и 1000")]
        public int StudentsAdmitted { get; set; }
    }

}
