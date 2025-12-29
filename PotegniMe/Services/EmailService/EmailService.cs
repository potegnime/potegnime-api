using SendGrid.Helpers.Mail;
using SendGrid;
using System.Net;

namespace PotegniMe.Services.EmailService
{
    public class EmailService : IEmailService
    {
        // Fields
        private readonly IConfiguration _configuration;
        private readonly string _sendGridApiKey;
        private readonly string _sendGridPasswordResetTemplateId;
        private readonly string _sendGridSenderEmail;
        private readonly string _sendGridSenderName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _sendGridApiKey = Environment.GetEnvironmentVariable("POTEGNIME_SENDGRID_KEY") ??
                throw new Exception("Cannot get inernal PotegniMe keys");
            _sendGridPasswordResetTemplateId = _configuration["SendGrid:PasswordResetTemplateId"] ??
                throw new Exception("SendGrid Password Reset Template ID not configured");
            _sendGridSenderEmail = _configuration["SendGrid:SenderEmail"] ??
                throw new Exception("SendGrid Sender Email not configured");
            _sendGridSenderName = _configuration["SendGrid:SenderName"] ??
                throw new Exception("SendGrid Sender Name not configured");

            if (
                string.IsNullOrEmpty(_sendGridApiKey) ||
                string.IsNullOrEmpty(_sendGridPasswordResetTemplateId) ||
                string.IsNullOrEmpty(_sendGridSenderEmail) ||
                string.IsNullOrEmpty(_sendGridSenderName)
                )
            {
                throw new ArgumentException("SendGrid settings not configured");
            }
        }

        public async Task SendEmailAsync(string userEmail, Dictionary<string, string> templateData)
        {
            SendGridClient client = new SendGridClient(_sendGridApiKey);
            EmailAddress from = new EmailAddress(_sendGridSenderEmail, _sendGridSenderName);
            EmailAddress to = new EmailAddress(userEmail);
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, _sendGridPasswordResetTemplateId, templateData);

            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                // Check if rate limit exceeded (403 Forbidden)
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new SendGridLimitException();
                }

                // General error
                throw new Exception($"Failed to send email: {response.StatusCode}");
            }
        }
    }
}
