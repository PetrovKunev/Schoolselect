using System.ComponentModel.DataAnnotations;
using SchoolSelect.Common;

namespace SchoolSelect.Web.ViewModels
{
    public class UserGradesInputModel
    {
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.UserGrades.ConfigurationNameMaxLength)]
        [Display(Name = "Име на конфигурацията")]
        public string ConfigurationName { get; set; } = string.Empty;


        [Range(2, 6, ErrorMessage = "Оценката трябва да бъде между 2 и 6")]
        [DisplayFormat(NullDisplayText = "")]
        [Display(Name = "Годишна оценка по Български език и литература")]
        public double? BulgarianGrade { get; set; }

        [Range(2, 6, ErrorMessage = "Оценката трябва да бъде между 2 и 6")]
        [DisplayFormat(NullDisplayText = "")]
        [Display(Name = "Годишна оценка по Математика")]
        public double? MathGrade { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Range(ValidationConstants.ExamPoints.Min, ValidationConstants.ExamPoints.Max, ErrorMessage = ValidationMessages.ExamPointsRange)]
        [Display(Name = "Точки от НВО по Български език и литература")]
        public double BulgarianExamPoints { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Range(ValidationConstants.ExamPoints.Min, ValidationConstants.ExamPoints.Max, ErrorMessage = ValidationMessages.ExamPointsRange)]
        [Display(Name = "Точки от НВО по Математика")]
        public double MathExamPoints { get; set; }

        [Display(Name = "Допълнителни оценки")]
        public List<UserAdditionalGradeInputModel> AdditionalGrades { get; set; } = new List<UserAdditionalGradeInputModel>();
    }
}
