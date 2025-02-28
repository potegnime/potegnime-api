namespace API.Services.EmailService
{
    public interface IEmailService
    {
        Task SendEmailAsync(string userEmail, Dictionary<string, string> templateData);
    }
}
