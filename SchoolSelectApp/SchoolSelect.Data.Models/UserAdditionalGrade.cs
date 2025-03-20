using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    // Клас за допълнителни оценки на ученика
    public class UserAdditionalGrade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserGradesId { get; set; }

        // Код на предмета
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.FormulaComponent.SubjectCodeMaxLength)]
        public string SubjectCode { get; set; } = null!;

        // Име на предмета
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.FormulaComponent.SubjectNameMaxLength)]
        public string SubjectName { get; set; } = null!;

        // Тип оценка/точки
        public int ComponentType { get; set; } = ComponentTypes.YearlyGrade;

        // Стойност (оценка или точки)
        [Range(0, ValidationConstants.ExamPoints.Max,
              ErrorMessage = ValidationMessages.ValueRange)]
        public double Value { get; set; }

        // Релация
        [ForeignKey(nameof(UserGradesId))]
        public virtual UserGrades? UserGrades { get; set; }
    }
}
