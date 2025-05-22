using System.ComponentModel.DataAnnotations;

namespace SchoolSelect.Web.ViewModels
{
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Полето е задължително")]
        [Display(Name = "Име")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето е задължително")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес")]
        [Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето е задължително")]
        [Display(Name = "Тема")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето е задължително")]
        [Display(Name = "Съобщение")]
        public string Message { get; set; } = string.Empty;
    }
}