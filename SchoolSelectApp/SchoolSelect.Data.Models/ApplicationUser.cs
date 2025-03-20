using Microsoft.AspNetCore.Identity;

namespace SchoolSelect.Data.Models
{
        public class ApplicationUser : IdentityUser<Guid>
        {
            public ApplicationUser()
            {
                this.Id = Guid.NewGuid();
                this.FirstName = string.Empty;
                this.LastName = string.Empty;
            }

            // Лични данни
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? LastLogin { get; set; }

            // Релации специфични за SchoolSelect
            public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
            public virtual ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();

            public virtual ICollection<ComparisonSet> ComparisonSets { get; set; } = new List<ComparisonSet>();

            public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

            public virtual ICollection<UserGrades> Grades { get; set; } = new List<UserGrades>();

        }
}
