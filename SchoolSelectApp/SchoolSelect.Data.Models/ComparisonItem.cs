using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSelect.Data.Models
{
    public class ComparisonItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ComparisonSetId { get; set; }

        [Required]
        public int SchoolId { get; set; }

        public int? ProfileId { get; set; }

        // Релации
        [ForeignKey(nameof(ComparisonSetId))]
        public virtual ComparisonSet? ComparisonSet { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public virtual School? School { get; set; }

        [ForeignKey(nameof(ProfileId))]
        public virtual SchoolProfile? Profile { get; set; }
    }

}
