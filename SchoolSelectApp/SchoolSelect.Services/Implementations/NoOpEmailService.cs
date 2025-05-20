using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    /// <summary>
    /// Email service който само логва, без да изпраща истински имейли
    /// Използва се за development и когато не искаме email функционалност
    /// </summary>
    public class NoOpEmailService : IEmailService, IEmailSender
    {
        private readonly ILogger<NoOpEmailService> _logger;

        public NoOpEmailService(ILogger<NoOpEmailService> logger)
        {
            _logger = logger;
        }

        // Твоя интерфейс
        public Task SendEmailAsync(string email, string subject, string message)
        {
            LogEmailSimulation(email, subject, message);
            return Task.CompletedTask;
        }

        // ASP.NET Core Identity интерфейс
        Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage)
        {
            LogEmailSimulation(email, subject, htmlMessage);
            return Task.CompletedTask;
        }

        private void LogEmailSimulation(string email, string subject, string message)
        {
            _logger.LogInformation("📧 EMAIL СИМУЛАЦИЯ:");
            _logger.LogInformation("   До: {Email}", email);
            _logger.LogInformation("   Тема: {Subject}", subject);
            _logger.LogInformation("   Съдържание: {Message}",
                message.Length > 100 ? message.Substring(0, 100) + "..." : message);
            _logger.LogInformation("   [В production това би било истински email]");
        }
    }
}