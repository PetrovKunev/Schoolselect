using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Implementations;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.Data;
using SchoolSelect.Web.TestData;

namespace SchoolSelect.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


            // Registering DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Registering Identity with our ApplicationUser
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireLowercase = true;
            })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Registering the Repository Pattern
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
            builder.Services.AddScoped<ISchoolProfileRepository, SchoolProfileRepository>();
            builder.Services.AddScoped<IHistoricalRankingRepository, HistoricalRankingRepository>();
            builder.Services.AddScoped<ISchoolFacilityRepository, SchoolFacilityRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
            builder.Services.AddScoped<IAdmissionFormulaRepository, AdmissionFormulaRepository>();
            builder.Services.AddScoped<IUserGradesRepository, UserGradesRepository>();
            builder.Services.AddScoped<IComparisonSetRepository, ComparisonSetRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

            // Register application services
            builder.Services.AddScoped<IHomeService, HomeService>();
            builder.Services.AddScoped<IUserGradesService, UserGradesService>();
            builder.Services.AddScoped<IUserPreferenceService, UserPreferenceService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IComparisonService, ComparisonService>();

            // Регистриране на Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddAuthentication()
            .AddGoogle(googleOptions =>
            {
                var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
                var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

                if (string.IsNullOrEmpty(googleClientId) || string.IsNullOrEmpty(googleClientSecret))
                {
                    throw new InvalidOperationException("Google authentication is missing.");
                }

                googleOptions.ClientId = googleClientId;
                googleOptions.ClientSecret = googleClientSecret;
            });

            // Конфигуриране на JSON форматиране (camelCase за свойствата)
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            });

            builder.Services.AddControllersWithViews();

            WebApplication app = builder.Build();

            // Зареждане на тестови данни
            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<ApplicationDbContext>();
                        SeedData.Initialize(context);
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Грешка при инициализиране на тестовата база данни.");
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}