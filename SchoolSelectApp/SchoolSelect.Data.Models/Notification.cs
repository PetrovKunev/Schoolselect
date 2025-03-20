using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.Notification.TitleMaxLength)]
        public string Title { get; set; } = null!;

        [StringLength(ValidationConstants.Notification.ContentMaxLength)]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Тип нотификация: 1 = Промяна в училище, 2 = Ново класиране, 3 = Системно съобщение
        public int NotificationType { get; set; }

        // ID на референциран обект (училище, профил и т.н.)
        public int? ReferenceId { get; set; }

        // Релации
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }
}
