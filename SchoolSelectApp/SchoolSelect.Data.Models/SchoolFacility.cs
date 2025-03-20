using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    // Допълнителни възможности (спортна база, извънкласни дейности и т.н.)
    public class SchoolFacility
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.SchoolFacility.TypeMaxLength)]
        public string FacilityType { get; set; } = null!;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.SchoolFacility.NameMaxLength)]
        public string Name { get; set; } = null!;

        [StringLength(ValidationConstants.Common.DescriptionMaxLength)]
        public string Description { get; set; } = string.Empty;

        // Релации
        [ForeignKey(nameof(SchoolId))]
        public virtual School? School { get; set; }

        // Одит информация
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
