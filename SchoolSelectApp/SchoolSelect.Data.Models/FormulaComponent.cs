using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    /// <summary>
    /// Компонент от формулата за балообразуване (предмет и коефициент)
    /// </summary>
    public class FormulaComponent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AdmissionFormulaId { get; set; }

        // Предмет/компонент (напр. "БЕЛ", "МАТ", "ЧЕз", "КМИТ")
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.FormulaComponent.SubjectCodeMaxLength)]
        public string SubjectCode { get; set; } = null!;

        // Наименование на предмета
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.FormulaComponent.SubjectNameMaxLength)]
        public string SubjectName { get; set; } = null!;

        // Тип компонент: 1 = Годишна оценка, 2 = НВО, 3 = Друго
        public int ComponentType { get; set; } = ComponentTypes.YearlyGrade;

        // Коефициент (множител)
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Range(ValidationConstants.FormulaComponent.MinMultiplier,
              ValidationConstants.FormulaComponent.MaxMultiplier)]
        public double Multiplier { get; set; }

        // Описание за потребителя
        [StringLength(ValidationConstants.FormulaComponent.DescriptionMaxLength)]
        public string Description { get; set; } = string.Empty;

        // Релация
        [ForeignKey(nameof(AdmissionFormulaId))]
        public virtual AdmissionFormula? Formula { get; set; }
    }
}
