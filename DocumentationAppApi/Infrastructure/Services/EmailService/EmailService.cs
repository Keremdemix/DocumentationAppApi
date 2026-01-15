using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DocumentationAppApi.Infrastructure.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of <see cref="EmailService"/> with the provided configuration used to read SMTP settings.
        /// </summary>
        /// <param name="config">Application configuration containing SMTP settings: Smtp:Host, Smtp:User, Smtp:Password, Smtp:FromName, and Smtp:Port.</param>
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Sends an email message to the specified recipient using SMTP settings read from configuration.
        /// </summary>
        /// <param name="toEmail">Recipient email address.</param>
        /// <param name="subject">Email subject line.</param>
        /// <param name="body">Email body text.</param>
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