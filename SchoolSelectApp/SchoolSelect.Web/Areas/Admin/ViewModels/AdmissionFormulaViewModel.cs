using System.ComponentModel.DataAnnotations;

namespace SchoolSelect.Web.Areas.Admin.ViewModels
{
    public class AdmissionFormulaViewModel
    {
        public int Id { get; set; }
        public int SchoolProfileId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Годината е задължителна")]
        [Range(2020, 2030, ErrorMessage = "Годината трябва да бъде между 2020 и 2030")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Формулата е задължителна")]
        [Display(Name = "Формула")]
        public string FormulaExpression { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string FormulaDescription { get; set; } = string.Empty;

        public List<FormulaComponentViewModel> Components { get; set; } = new List<FormulaComponentViewModel>();
    }

}