using System.ComponentModel.DataAnnotations;

namespace SchoolSelect.Web.ViewModels
{
    /// <summary>
    /// Модел за създаване на ново известие
    /// </summary>
    public class CreateNotificationModel
    {
        [Required(ErrorMessage = "Заглавието е задължително")]
        [StringLength(100, ErrorMessage = "Заглавието трябва да е между {2} и {1} символа", MinimumLength = 3)]
        public string Title { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Съдържанието не може да надвишава {1} символа")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Типът е задължителен")]
        [Range(1, 3, ErrorMessage = "Неправилен тип известие")]
        public int NotificationType { get; set; }

        public int? ReferenceId { get; set; }

        // Полета за получатели
        public bool SendToAll { get; set; } = false;
        public List<Guid>? UserIds { get; set; }

        // Филтри за потребителски предпочитания
        public string? PreferredDistrict { get; set; }
        public List<string>? PreferredProfiles { get; set; }
    }
}
