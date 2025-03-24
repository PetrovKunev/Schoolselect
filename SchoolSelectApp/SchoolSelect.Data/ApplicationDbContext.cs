using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;

namespace SchoolSelect.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Основни модели
        public DbSet<School> Schools { get; set; }
        public DbSet<SchoolProfile> SchoolProfiles { get; set; }
        public DbSet<SchoolFacility> SchoolFacilities { get; set; }
        public DbSet<HistoricalRanking> HistoricalRankings { get; set; }

        // Балообразуване
        public DbSet<AdmissionFormula> AdmissionFormulas { get; set; }
        public DbSet<FormulaComponent> FormulaComponents { get; set; }

        // Потребителски данни
        public DbSet<Review> Reviews { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<UserGrades> UserGrades { get; set; }
        public DbSet<UserAdditionalGrade> UserAdditionalGrades { get; set; }

        // Сравнения и известия
        public DbSet<ComparisonSet> ComparisonSets { get; set; }
        public DbSet<ComparisonItem> ComparisonItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Важно за Identity таблиците

            // Конфигурации за School
            builder.Entity<School>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Каскадно изтриване
                entity.HasMany(s => s.Profiles)
                      .WithOne(p => p.School)
                      .HasForeignKey(p => p.SchoolId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(s => s.Facilities)
                      .WithOne(f => f.School)
                      .HasForeignKey(f => f.SchoolId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурации за SchoolProfile
            builder.Entity<SchoolProfile>(entity =>
            {
                entity.HasIndex(e => new { e.SchoolId, e.Name }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasMany(p => p.AdmissionFormulas)
                      .WithOne(f => f.SchoolProfile)
                      .HasForeignKey(f => f.SchoolProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурации за AdmissionFormula
            builder.Entity<AdmissionFormula>(entity =>
            {
                entity.HasIndex(e => new { e.SchoolProfileId, e.Year }).IsUnique();

                entity.HasMany(f => f.Components)
                      .WithOne(c => c.Formula)
                      .HasForeignKey(c => c.AdmissionFormulaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурации за UserGrades
            builder.Entity<UserGrades>(entity =>
            {
                entity.HasMany(g => g.AdditionalGrades)
                      .WithOne(a => a.UserGrades)
                      .HasForeignKey(a => a.UserGradesId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурации за ComparisonSet
            builder.Entity<ComparisonSet>(entity =>
            {
                entity.HasMany(c => c.Items)
                      .WithOne(i => i.ComparisonSet)
                      .HasForeignKey(i => i.ComparisonSetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TransportFacility>().ToTable("SchoolFacilities");

        }
    }
}