using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace PotegniMe.Services.EmailService;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpServer;
    private readonly ushort _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpServer = _configuration["Email:SmtpServer"] ?? throw new Exception($"{Constants.Constants.AppSettingsErrorCode} SmtpServer");
        _smtpUsername = _configuration["Email:Username"] ?? throw new Exception($"{Constants.Constants.AppSettingsErrorCode} SmtpUsername");
        _smtpPassword = _configuration["Email:Password"] ?? throw new Exception($"{Constants.Constants.AppSettingsErrorCode} SmtpPassword");
        var res = ushort.TryParse(_configuration["Email:SmtpPort"], out _smtpPort);

        if (
            string.IsNullOrEmpty(_smtpServer) ||
            string.IsNullOrEmpty(_smtpUsername) ||
            string.IsNullOrEmpty(_smtpPassword) ||
            res == false
            )
        {
            throw new ArgumentException("Email settings not configured");
        }
    }

    public async Task SendEmailAsync(string userEmail, Dictionary<string, string> templateData)
    {
        try
        {
            var smtpClient = new SmtpClient()
            {
                Host = _smtpServer,
                Port = _smtpPort,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true,
                Timeout = 5000
            };

            string templatePath = Path.Combine(AppContext.BaseDirectory, "email-templates", "reset-password.html");
            if (!File.Exists(templatePath)) throw new FileNotFoundException("email-templates/reset-password.html file not found", templatePath);

            string msg = File.ReadAllText(templatePath);
            msg = FormatText(msg, templateData);

            var mail = new MailMessage()
            {
                From = new MailAddress(_smtpUsername),
                Subject = "Ponastavitev Gesla",
                Body = msg,
                IsBodyHtml = true
            };

            mail.To.Add(userEmail);
            await smtpClient.SendMailAsync(mail);
        }
        catch (SmtpException)    
        {
            throw;
        }
    }

    private string FormatText(string text, Dictionary<string, string> templateData)
    {
        return Regex.Replace(text, @"\{\{\s*(.*?)\s*\}\}", m =>
        {
            var key = m.Groups[1].Value;
            return templateData.TryGetValue(key, out var value) ? value : m.Value;
        });
    }
}
