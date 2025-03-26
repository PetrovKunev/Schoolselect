using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolSelect.Web.ViewModels
{
    /// <summary>
    /// Модел за визуализация на известие в изгледа
    /// </summary>
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int NotificationType { get; set; }
        public int? ReferenceId { get; set; }

        // Помощни свойства за изгледа
        public string TypeIcon => GetTypeIcon();
        public string TypeClass => GetTypeClass();
        public string RelativeTime => GetRelativeTime();

        private string GetTypeIcon()
        {
            return NotificationType switch
            {
                1 => "fa-school", // Промяна в училище
                2 => "fa-chart-line", // Ново класиране
                3 => "fa-bullhorn", // Системно съобщение
                _ => "fa-bell"
            };
        }

        private string GetTypeClass()
        {
            return NotificationType switch
            {
                1 => "bg-primary", // Промяна в училище
                2 => "bg-success", // Ново класиране
                3 => "bg-warning", // Системно съобщение
                _ => "bg-secondary"
            };
        }

        private string GetRelativeTime()
        {
            var timeSpan = DateTime.UtcNow - CreatedAt;

            if (timeSpan.TotalMinutes < 1)
                return "преди няколко секунди";
            if (timeSpan.TotalMinutes < 60)
                return $"преди {(int)timeSpan.TotalMinutes} мин";
            if (timeSpan.TotalHours < 24)
                return $"преди {(int)timeSpan.TotalHours} ч";
            if (timeSpan.TotalDays < 7)
                return $"преди {(int)timeSpan.TotalDays} дни";

            return CreatedAt.ToString("dd.MM.yyyy");
        }
    }

}
