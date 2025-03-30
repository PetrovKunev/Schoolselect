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
            // Get admin credentials from configuration
            var adminEmail = _configuration["AdminUser:Email"];
            var adminPassword = _configuration["AdminUser:Password"];

            // Validate configuration
            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                _logger.LogWarning("Admin user credentials not found in configuration. Using default values for development only.");
                adminEmail = "admin@schoolselect.com";
                adminPassword = "Admin!@123";
            }

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

                // Reset password only if explicitly configured to do so
                var resetPassword = _configuration.GetValue<bool>("AdminUser:ResetPasswordOnStartup");
                if (resetPassword)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                    var resetResult = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);

                    if (!resetResult.Succeeded)
                    {
                        _logger.LogWarning("Password reset failed. Errors: {Errors}",
                            string.Join(", ", resetResult.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        _logger.LogInformation("Admin password reset successfully");
                    }
                }
            }
        }
    }
}