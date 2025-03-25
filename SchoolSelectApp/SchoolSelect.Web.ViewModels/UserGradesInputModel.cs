using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolSelect.Common;

namespace SchoolSelect.Web.ViewModels
{
    public class UserGradesInputModel
    {
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.UserGrades.ConfigurationNameMaxLength)]
        [Display(Name = "Име на конфигурацията")]
        public string ConfigurationName { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Range(ValidationConstants.Grade.Min, ValidationConstants.Grade.Max, ErrorMessage = ValidationMessages.GradeRange)]
        [Display(Name = "Годишна оценка по Български език и литература")]
        public double BulgarianGrade { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Range(ValidationConstants.Grade.Min, ValidationConstants.Grade.Max, ErrorMessage = ValidationMessages.GradeRange)]
        [Display(Name = "Годишна оценка по Математика")]
        public double MathGrade { get; set; }

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
