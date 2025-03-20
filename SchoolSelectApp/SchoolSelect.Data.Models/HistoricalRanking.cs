using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    // Исторически данни за класирания от предходни години
    public class HistoricalRanking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }

        public int? ProfileId { get; set; }

        [Required]
        [Range(ValidationConstants.Year.Min, ValidationConstants.Year.Max,
              ErrorMessage = ValidationMessages.YearRange)]
        public int Year { get; set; }

        [Required]
        [Range(ValidationConstants.Score.Min, ValidationConstants.Score.Max,
              ErrorMessage = ValidationMessages.ScoreRange)]
        public double MinimumScore { get; set; }

        [Range(1, ValidationConstants.Ranking.MaxRound,
              ErrorMessage = ValidationMessages.RoundRange)]
        public int Round { get; set; } = 1;

        [Range(0, ValidationConstants.SchoolProfile.MaxPlaces,
              ErrorMessage = ValidationMessages.StudentsRange)]
        public int StudentsAdmitted { get; set; }

        // Релации
        [ForeignKey(nameof(SchoolId))]
        public virtual School? School { get; set; }

        [ForeignKey(nameof(ProfileId))]
        public virtual SchoolProfile? Profile { get; set; }

        // Одит информация
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
