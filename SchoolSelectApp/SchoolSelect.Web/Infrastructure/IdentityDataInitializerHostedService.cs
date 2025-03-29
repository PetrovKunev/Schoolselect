using Microsoft.AspNetCore.Identity;
using SchoolSelect.Data.Models;

namespace SchoolSelect.Web.Infrastructure
{
    public class IdentityDataInitializerHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IdentityDataInitializerHostedService> _logger;

        public IdentityDataInitializerHostedService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<IdentityDataInitializerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Identity Data Initializer");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                await SeedRolesAsync(roleManager);
                await SeedAdminUserAsync(userManager);

                _logger.LogInformation("Identity Data Initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing identity data");
            }

            return;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            string[] roleNames = { "Admin", "Moderator", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Created role: {RoleName}", roleName);
                    }
                }
            }
        }

        private async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@schoolselect.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var adminPassword = "Admin!@123";
                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin user created successfully");
                    var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to assign Admin role. Errors: {Errors}",
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    _logger.LogError("Failed to create admin user. Errors: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                var isAdmin = await userManager.IsInRoleAsync(adminUser, "Admin");
                if (!isAdmin)
                {
                    var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to assign Admin role. Errors: {Errors}",
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }

                // Ресет на паролата при всяко стартиране
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                var resetResult = await userManager.ResetPasswordAsync(adminUser, token, "Admin!@123");

                if (!resetResult.Succeeded)
                {
                    _logger.LogWarning("Password reset failed. Errors: {Errors}",
                        string.Join(", ", resetResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}