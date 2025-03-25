using System.ComponentModel.DataAnnotations;
using SchoolSelect.Common;

namespace SchoolSelect.Web.ViewModels
{
    public class UserPreferenceInputModel
    {
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.UserPreference.NameMaxLength)]
        [Display(Name = "Име на предпочитанието")]
        public string PreferenceName { get; set; } = string.Empty;

        // Район на ученика
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Display(Name = "Предпочитан район")]
        public string UserDistrict { get; set; } = string.Empty;

        // Географски координати
        [Display(Name = "Географска ширина")]
        [Range(ValidationConstants.GeoCoordinates.MinLatitude,
               ValidationConstants.GeoCoordinates.MaxLatitude,
               ErrorMessage = ValidationMessages.LatitudeRange)]
        public double? UserLatitude { get; set; }

        [Display(Name = "Географска дължина")]
        [Range(ValidationConstants.GeoCoordinates.MinLongitude,
               ValidationConstants.GeoCoordinates.MaxLongitude,
               ErrorMessage = ValidationMessages.LongitudeRange)]
        public double? UserLongitude { get; set; }

        // Предпочитани профили
        [Display(Name = "Предпочитани профили")]
        public List<string> PreferredProfiles { get; set; } = new List<string>();

        // Тегла на критериите
        [Display(Name = "Близост")]
        [Range(0, 5, ErrorMessage = ValidationMessages.ValueRange)]
        public int ProximityWeight { get; set; } = 3;

        [Display(Name = "Рейтинг")]
        [Range(0, 5, ErrorMessage = ValidationMessages.ValueRange)]
        public int RatingWeight { get; set; } = 3;

        [Display(Name = "Съответствие с бал")]
        [Range(0, 5, ErrorMessage = ValidationMessages.ValueRange)]
        public int ScoreMatchWeight { get; set; } = 4;

        [Display(Name = "Профил")]
        [Range(0, 5, ErrorMessage = ValidationMessages.ValueRange)]
        public int ProfileMatchWeight { get; set; } = 5;

        [Display(Name = "Допълнителни възможности")]
        [Range(0, 5, ErrorMessage = ValidationMessages.ValueRange)]
        public int FacilitiesWeight { get; set; } = 2;
    }
}