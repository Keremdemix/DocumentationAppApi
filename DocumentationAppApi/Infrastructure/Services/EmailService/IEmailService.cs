namespace DocumentationAppApi.Infrastructure.Services.EmailService
{
    public interface IEmailService
    {
        /// <summary>
/// Sends an email to the specified recipient.
/// </summary>
/// <param name="toEmail">The recipient's email address.</param>
/// <param name="subject">The email subject line.</param>
/// <param name="body">The email body content.</param>
/// <returns>A Task representing the send operation.</returns>
Task SendEmailAsync(string toEmail, string subject, string body);
    }
}