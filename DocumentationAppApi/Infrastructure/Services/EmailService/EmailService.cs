using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DocumentationAppApi.Infrastructure.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _config.GetValue<string>("Smtp:Host");
            var smtpUser = _config.GetValue<string>("Smtp:User");
            var smtpPassword = _config.GetValue<string>("Smtp:Password");
            var fromName = _config.GetValue<string>("Smtp:FromName");
            var port = _config.GetValue<int>("Smtp:Port");

            using var smtpClient = new SmtpClient(smtpHost)
            {
                Port = port,
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
