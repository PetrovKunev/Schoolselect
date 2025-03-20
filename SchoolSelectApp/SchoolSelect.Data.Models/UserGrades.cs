using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    // Клас за съхраняване на оценки и резултати на ученика
    public class UserGrades
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        // Време на създаване
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Име за запазената конфигурация
        [StringLength(ValidationConstants.UserGrades.ConfigurationNameMaxLength)]
        public string ConfigurationName { get; set; } = string.Empty;

        // Оценки по основни предмети
        [Range(ValidationConstants.Grade.Min, ValidationConstants.Grade.Max,
              ErrorMessage = ValidationMessages.GradeRange)]
        public double BulgarianGrade { get; set; }

        [Range(ValidationConstants.Grade.Min, ValidationConstants.Grade.Max,
              ErrorMessage = ValidationMessages.GradeRange)]
        public double MathGrade { get; set; }

        // Резултати от НВО (точки)
        [Range(ValidationConstants.ExamPoints.Min, ValidationConstants.ExamPoints.Max,
              ErrorMessage = ValidationMessages.ExamPointsRange)]
        public double BulgarianExamPoints { get; set; }

        [Range(ValidationConstants.ExamPoints.Min, ValidationConstants.ExamPoints.Max,
              ErrorMessage = ValidationMessages.ExamPointsRange)]
        public double MathExamPoints { get; set; }

        // Допълнителни оценки
        public virtual ICollection<UserAdditionalGrade> AdditionalGrades { get; set; } = new List<UserAdditionalGrade>();

        // Релации
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }

}
