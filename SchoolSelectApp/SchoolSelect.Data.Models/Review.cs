using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    // Отзиви от потребители
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(ValidationConstants.Review.ContentMaxLength,
                     ErrorMessage = ValidationMessages.ReviewLengthExceeded)]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Range(ValidationConstants.Rating.MinValue, ValidationConstants.Rating.MaxValue,
              ErrorMessage = ValidationMessages.RatingRange)]
        public int Rating { get; set; }

        // Статус за модерация на отзива
        public bool IsApproved { get; set; } = false;

        // Релации
        [ForeignKey(nameof(SchoolId))]
        public virtual School? School { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }

}
