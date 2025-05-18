// SchoolSelect.Web/Areas/Admin/ViewModels/SchoolEditViewModel.cs
using System.ComponentModel.DataAnnotations;
using SchoolSelect.Common;

namespace SchoolSelect.Web.Areas.Admin.ViewModels
{
    public class SchoolEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.School.NameMaxLength, ErrorMessage = ValidationMessages.MaxLengthExceeded)]
        [Display(Name = "Име на училището")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.School.AddressMaxLength)]
        [Display(Name = "Адрес")]
        public string Address { get; set; } = string.Empty;

        [StringLength(ValidationConstants.School.DistrictMaxLength)]
        [Display(Name = "Район")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.School.CityMaxLength)]
        [Display(Name = "Град")]
        public string City { get; set; } = string.Empty;

        [Phone(ErrorMessage = ValidationMessages.InvalidPhone)]
        [StringLength(ValidationConstants.Common.PhoneMaxLength)]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = ValidationMessages.InvalidEmail)]
        [StringLength(ValidationConstants.Common.EmailMaxLength)]
        [Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Url(ErrorMessage = ValidationMessages.InvalidUrl)]
        [StringLength(ValidationConstants.Common.UrlMaxLength)]
        [Display(Name = "Уебсайт")]
        public string Website { get; set; } = string.Empty;

        [StringLength(ValidationConstants.Common.DescriptionMaxLength)]
        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Географска ширина")]
        public double? GeoLatitude { get; set; }

        [Display(Name = "Географска дължина")]
        public double? GeoLongitude { get; set; }

        [StringLength(ValidationConstants.School.MapsFormattedAddressMaxLength)]
        [Display(Name = "Форматиран адрес за Google Maps")]
        public string MapsFormattedAddress { get; set; } = string.Empty;
    }
}