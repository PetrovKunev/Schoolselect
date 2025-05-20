using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    public class EmailService : IEmailService, IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                // Взимаме настройките от appsettings.json
                var host = _configuration["EmailSettings:Host"];
                var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"] ?? "SchoolSelect";
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                _logger.LogInformation("Email настройки - Host: {Host}, Port: {Port}, User: {Username}, EnableSsl: {EnableSsl}",
                    host, port, username, enableSsl);

                // Проверяваме дали всички настройки са налични
                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) ||
                    string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogWarning("Email конфигурацията не е пълна. Пропускаме изпращането на имейл.");
                    return;
                }

                using var client = new SmtpClient(host, port);

                // Специални настройки за порт 465 (SSL)
                if (port == 465)
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(username, password);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                }
                else
                {
                    // За порт 587 (STARTTLS)
                    client.EnableSsl = enableSsl;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(username, password);
                }

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                _logger.LogInformation("Опит за изпращане на имейл до {Email} чрез {Host}:{Port}", email, host, port);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Успешно изпратен имейл до {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ГРЕШКА при изпращане на имейл до {Email}: {Message}", email, ex.Message);

                // В development среда не хвърляме грешка, а само логваме
                if (_configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    _logger.LogInformation("Development среда: Имейл грешка симулирана за {Email}: {Subject}",
                        email, subject);
                }
                else
                {
                    // В production можем да хвърлим грешка или да използваме fallback метод
                    throw;
                }
            }
        }
    }
}