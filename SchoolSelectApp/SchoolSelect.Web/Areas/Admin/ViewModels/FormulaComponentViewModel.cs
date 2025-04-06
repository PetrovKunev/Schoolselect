using System.ComponentModel.DataAnnotations;

namespace SchoolSelect.Web.Areas.Admin.ViewModels
{
    public class FormulaComponentViewModel
    {
        public int Id { get; set; }
        public int AdmissionFormulaId { get; set; }

        [Required(ErrorMessage = "Кодът на предмета е задължителен")]
        [Display(Name = "Код на предмет")]
        public string SubjectCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Името на предмета е задължително")]
        [Display(Name = "Име на предмет")]
        public string SubjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Типът е задължителен")]
        [Display(Name = "Тип")]
        public int ComponentType { get; set; }

        [Required(ErrorMessage = "Коефициентът е задължителен")]
        [Range(0.1, 10, ErrorMessage = "Коефициентът трябва да бъде между 0.1 и 10")]
        [Display(Name = "Коефициент")]
        public double Multiplier { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;
    }
}
