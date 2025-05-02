using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    // Потребителски предпочитания
    public class UserPreference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.UserPreference.NameMaxLength)]
        public string PreferenceName { get; set; } = null!;

        // Тегла на различните критерии (съхранявани като JSON)
        [StringLength(ValidationConstants.UserPreference.CriteriaWeightsMaxLength)]
        public string CriteriaWeights { get; set; } = string.Empty;

        // Предпочитан район
        [StringLength(ValidationConstants.School.DistrictMaxLength)]
        public string? UserDistrict { get; set; } = string.Empty;

        // Географски координати на ученика
        [Range(ValidationConstants.GeoCoordinates.MinLatitude,
              ValidationConstants.GeoCoordinates.MaxLatitude)]
        public double? UserLatitude { get; set; }

        [Range(ValidationConstants.GeoCoordinates.MinLongitude,
              ValidationConstants.GeoCoordinates.MaxLongitude)]
        public double? UserLongitude { get; set; }

        // Предпочитани профили (съхранявани като CSV от имена)
        [StringLength(ValidationConstants.UserPreference.PreferredProfilesMaxLength)]
        public string PreferredProfiles { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Релации
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }

}
