using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    // Паралелки в училище (профилирани или професионални)
    public class SchoolProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.SchoolProfile.NameMaxLength)]
        public string Name { get; set; } = null!;

        [StringLength(ValidationConstants.SchoolProfile.CodeMaxLength)]
        public string Code { get; set; } = string.Empty;

        [StringLength(ValidationConstants.Common.DescriptionMaxLength)]
        public string Description { get; set; } = string.Empty;

        [StringLength(ValidationConstants.SchoolProfile.SubjectsMaxLength)]
        public string Subjects { get; set; } = string.Empty;

        [Range(0, ValidationConstants.SchoolProfile.MaxPlaces,
              ErrorMessage = ValidationMessages.PlacesRange)]
        public int AvailablePlaces { get; set; }

        // Тип на паралелката
        public ProfileType? Type { get; set; }

        // Поле за професионалните паралелки
        [StringLength(ValidationConstants.SchoolProfile.SpecialtyMaxLength)]
        public string? Specialty { get; set; }

        // Професионална квалификация
        [StringLength(ValidationConstants.SchoolProfile.QualificationMaxLength)]
        public string? ProfessionalQualification { get; set; }

        // Релации
        [ForeignKey(nameof(SchoolId))]
        public virtual School? School { get; set; }
        public virtual ICollection<HistoricalRanking> Rankings { get; set; } = new List<HistoricalRanking>();
        public virtual ICollection<AdmissionFormula> AdmissionFormulas { get; set; } = new List<AdmissionFormula>();

        // Одит информация
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}