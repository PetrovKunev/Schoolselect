using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Configurations;
using SchoolSelect.Services.Implementations;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.Data;
using SchoolSelect.Web.Infrastructure;


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
                options.SignIn.RequireConfirmedEmail = false;
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
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<ISchoolImportService, SchoolImportService>();
            builder.Services.AddScoped<IAdmissionService, AdmissionService>();
            builder.Services.AddScoped<IChanceCalculator, DefaultChanceCalculator>();
            builder.Services.AddScoped<IScoreCalculationService, ScoreCalculationService>();
            builder.Services.AddScoped<ISchoolRecommendationService, SchoolRecommendationService>();

            // Email services - временно деактивирани
            // builder.Services.AddScoped<IEmailService, EmailService>();
            // builder.Services.AddScoped<IEmailSender, EmailService>();

            // Заместваме с NoOp service за development
            builder.Services.AddTransient<IEmailService, NoOpEmailService>();
            builder.Services.AddTransient<IEmailSender, NoOpEmailService>();


            // Регистриране на HttpClient за GoogleGeocodingService
            builder.Services.AddHttpClient<IGeocodingService, GoogleGeocodingService>();

            // Регистрация на услугите за геокодиране
            builder.Services.AddScoped<IGeocodingService, GoogleGeocodingService>();
            builder.Services.AddScoped<ISchoolGeocodingService, SchoolGeocodingService>();

            // Конфигуриране на опции за ChanceCalculator
            builder.Services.Configure<ChanceCalculatorOptions>(
                builder.Configuration.GetSection("ChanceCalculator"));


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
            })
            .AddFacebook(facebookOptions => // This will now work
            {
                var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
                var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];

                if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
                {
                    facebookOptions.AppId = facebookAppId;
                    facebookOptions.AppSecret = facebookAppSecret;

                    // Add additional permissions
                    facebookOptions.Scope.Add("email");
                    facebookOptions.Scope.Add("public_profile");

                    // Map claims
                    facebookOptions.ClaimActions.MapJsonKey("urn:facebook:name", "name");
                    facebookOptions.ClaimActions.MapJsonKey("urn:facebook:email", "email");
                }
            });
            // Microsoft authentication - временно закоментирано до получаване на одобрение от МОН
            /*.AddMicrosoftAccount(microsoftOptions =>
            {
                var microsoftClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
                var microsoftClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];

                if (!string.IsNullOrEmpty(microsoftClientId) && !string.IsNullOrEmpty(microsoftClientSecret))
                {
                    microsoftOptions.ClientId = microsoftClientId;
                    microsoftOptions.ClientSecret = microsoftClientSecret;

                    // Add scopes
                    microsoftOptions.Scope.Add("https://graph.microsoft.com/user.read");
                    microsoftOptions.Scope.Add("https://graph.microsoft.com/email");
                }
            });*/

            
            builder.Services.AddAuthorization(options =>
            {
                // По подразбиране всички страници са публични
                options.FallbackPolicy = null;

                // Само специфични области изискват логин
                options.AddPolicy("RequireAuth", policy =>
                    policy.RequireAuthenticatedUser());
            });

            // Конфигуриране на JSON форматиране (camelCase за свойствата)
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            });

            builder.Services.AddControllersWithViews(options =>
            {
                options.MaxModelBindingCollectionSize = 100;
            });

            builder.Services.AddHostedService<IdentityDataInitializerHostedService>();

            WebApplication app = builder.Build();   


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
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}