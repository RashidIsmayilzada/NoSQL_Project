using System.Net;
using System.Net.Mail;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _config;

        public EmailSenderService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var host = _config["Email:Host"];
            var port = int.Parse(_config["Email:Port"] ?? "587");
            var username = _config["Email:UserName"];
            var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            var from = _config["Email:From"] ?? username;
            var enableSsl = bool.Parse(_config["Email:EnableSsl"] ?? "true");

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Email settings are not configured correctly.");
            }

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            using var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}
