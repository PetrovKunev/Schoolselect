using System.ComponentModel.DataAnnotations;
using SchoolSelect.Common;

namespace SchoolSelect.Web.ViewModels
{
    public class EditReviewViewModel
    {
        public int Id { get; set; }

        public int SchoolId { get; set; }

        public string SchoolName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Моля, въведете съдържание на отзива.")]
        [StringLength(ValidationConstants.Review.ContentMaxLength,
                     ErrorMessage = ValidationMessages.ReviewLengthExceeded)]
        [Display(Name = "Съдържание на отзива")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Моля, изберете оценка.")]
        [Range(ValidationConstants.Rating.MinValue, ValidationConstants.Rating.MaxValue,
              ErrorMessage = ValidationMessages.RatingRange)]
        [Display(Name = "Оценка (от 1 до 5)")]
        public int Rating { get; set; }
    }
}
