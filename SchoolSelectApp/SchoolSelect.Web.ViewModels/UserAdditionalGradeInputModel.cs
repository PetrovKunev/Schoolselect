using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolSelect.Common;

namespace SchoolSelect.Web.ViewModels
{
    public class UserAdditionalGradeInputModel
    {
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Display(Name = "Предмет")]
        public string SubjectCode { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Display(Name = "Име на предмета")]
        public string SubjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Display(Name = "Тип компонент")]
        public int ComponentType { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Range(0, 100, ErrorMessage = ValidationMessages.ValueRange)]
        [Display(Name = "Стойност")]
        public double Value { get; set; }
    }
}
