using System.ComponentModel.DataAnnotations;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    // Основна информация за училищата
    public class School
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.School.NameMaxLength,
            ErrorMessage = ValidationMessages.MaxLengthExceeded)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.School.AddressMaxLength)]
        public string Address { get; set; } = null!;

        [StringLength(ValidationConstants.School.DistrictMaxLength)]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.School.CityMaxLength)]
        public string City { get; set; } = null!;

        [Phone(ErrorMessage = ValidationMessages.InvalidPhone)]
        [StringLength(ValidationConstants.Common.PhoneMaxLength)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = ValidationMessages.InvalidEmail)]
        [StringLength(ValidationConstants.Common.EmailMaxLength)]
        public string Email { get; set; } = string.Empty;

        [Url(ErrorMessage = ValidationMessages.InvalidUrl)]
        [StringLength(ValidationConstants.Common.UrlMaxLength)]
        public string Website { get; set; } = string.Empty;

        [StringLength(ValidationConstants.Common.DescriptionMaxLength)]
        public string? Description { get; set; } = string.Empty;

        // Географски координати за изчисляване на разстояние
        [Range(ValidationConstants.GeoCoordinates.MinLatitude,
              ValidationConstants.GeoCoordinates.MaxLatitude,
              ErrorMessage = ValidationMessages.LatitudeRange)]
        public double? GeoLatitude { get; set; }

        [Range(ValidationConstants.GeoCoordinates.MinLongitude,
              ValidationConstants.GeoCoordinates.MaxLongitude,
              ErrorMessage = ValidationMessages.LongitudeRange)]
        public double? GeoLongitude { get; set; }

        // Поле за форматиран адрес за Google Maps API
        [StringLength(ValidationConstants.School.MapsFormattedAddressMaxLength)]
        public string MapsFormattedAddress { get; set; } = string.Empty;

        // Рейтинг, изчислен от потребителски отзиви
        [Range(ValidationConstants.Rating.MinValue, ValidationConstants.Rating.MaxValue,
              ErrorMessage = ValidationMessages.RatingRange)]
        public double AverageRating { get; set; } = 0;

        public int RatingsCount { get; set; } = 0;

        // Релации
        public virtual ICollection<SchoolProfile> Profiles { get; set; } = new List<SchoolProfile>();
        public virtual ICollection<HistoricalRanking> HistoricalRankings { get; set; } = new List<HistoricalRanking>();
        public virtual ICollection<SchoolFacility> Facilities { get; set; } = new List<SchoolFacility>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Одит информация
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = ValidationConstants.Common.DefaultCreator;
        public string UpdatedBy { get; set; } = ValidationConstants.Common.DefaultUpdater;
    }

}
