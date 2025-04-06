using SchoolSelect.Common;
using System.ComponentModel.DataAnnotations;

namespace SchoolSelect.Web.Areas.Admin.ViewModels
{
    public class SchoolProfileCreateViewModel
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Името е задължително")]
        [StringLength(100, ErrorMessage = "Името не може да бъде по-дълго от 100 символа")]
        public string Name { get; set; } = null!;

        [StringLength(20)]
        public string Code { get; set; } = string.Empty; 

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Subjects { get; set; }

        [Range(0, 100, ErrorMessage = "Броят места трябва да бъде между 0 и 100")]
        public int AvailablePlaces { get; set; }

        public ProfileType? Type { get; set; }

        [StringLength(ValidationConstants.SchoolProfile.SpecialtyMaxLength)]
        [Display(Name = "Специалност")]
        public string? Specialty { get; set; }

        [StringLength(ValidationConstants.SchoolProfile.QualificationMaxLength)]
        [Display(Name = "Професионална квалификация")]
        public string? ProfessionalQualification { get; set; }
    }
}
