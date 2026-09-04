using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace PotegniMe.Services.EmailService;

public class EmailService(MailConfig mailConfig) : IEmailService
{
    private readonly MailConfig _mailConfig = mailConfig;

    public async Task SendEmailAsync(string userEmail, Dictionary<string, string> templateData)
    {
        string templatePath = Path.Combine(AppContext.BaseDirectory, "email-templates", "reset-password.html");
        if (!File.Exists(templatePath)) throw new FileNotFoundException("email-templates/reset-password.html file not found", templatePath);

        string msg = await File.ReadAllTextAsync(templatePath);
        msg = FormatText(msg, templateData);

        await SendRawEmailAsync(userEmail, "Ponastavi geslo - potegni.me", msg);
    }

    public async Task SendRawEmailAsync(string recipient, string subject, string htmlBody)
    {
        using var smtpClient = new SmtpClient
        {
            Host = _mailConfig.Host,
            Port = _mailConfig.Port,
            Credentials = new NetworkCredential(_mailConfig.Username, _mailConfig.Password),
            EnableSsl = true, // STARTTLS upgrade on the submission port
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 10000
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(_mailConfig.FromAddress, MailConfig.FromDisplayName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        mail.To.Add(recipient);

        // The From mailbox is not monitored, replies go elsewhere
        mail.ReplyToList.Add(new MailAddress(_mailConfig.ReplyTo));

        await smtpClient.SendMailAsync(mail);
    }

    private static string FormatText(string text, Dictionary<string, string> templateData)
    {
        return Regex.Replace(text, @"\{\{\s*(.*?)\s*\}\}", m =>
        {
            var key = m.Groups[1].Value;
            return templateData.TryGetValue(key, out var value) ? value : m.Value;
        });
    }
}
