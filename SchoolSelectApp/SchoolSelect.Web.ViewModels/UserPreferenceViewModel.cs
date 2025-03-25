using System.ComponentModel.DataAnnotations;
using SchoolSelect.Common;

namespace SchoolSelect.Web.ViewModels
{
    public class UserPreferenceViewModel
    {
        public int Id { get; set; }

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
        public double? UserLatitude { get; set; }

        [Display(Name = "Географска дължина")]
        public double? UserLongitude { get; set; }

        // Предпочитани профили (ще се съхраняват като CSV)
        [Display(Name = "Предпочитани профили")]
        public List<string> PreferredProfiles { get; set; } = new List<string>();

        // Тегла на критериите (ще се съхраняват като JSON)
        [Display(Name = "Тегла на критериите")]
        public Dictionary<string, double> CriteriaWeights { get; set; } = new Dictionary<string, double>();

        public DateTime CreatedAt { get; set; }
    }
}