namespace PotegniMe.Services.EmailService;

/// <summary>
/// Sends a single test message through the configured transport, so delivery can be
/// verified without triggering a real app flow. Invoked via scripts/test-mail.
/// </summary>
public static class TestMailCommand
{
    public const string Flag = "--send-test-mail";

    public static async Task<int> RunAsync(string[] args)
    {
        string? recipient = args.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(recipient))
        {
            Console.Error.WriteLine($"Usage: dotnet run -- {Flag} <recipient@example.com>");
            return 1;
        }

        try
        {
            var mailConfig = new MailConfig();
            var emailService = new EmailService(mailConfig);

            Console.WriteLine($"Relay:    {mailConfig.Host}:{mailConfig.Port} (STARTTLS)");
            Console.WriteLine($"User:     {mailConfig.Username}");
            Console.WriteLine($"From:     {mailConfig.FromAddress}");
            Console.WriteLine($"Reply-To: {mailConfig.ReplyTo}");
            Console.WriteLine($"To:       {recipient}");

            string timestamp = DateTime.UtcNow.ToString("u");
            await emailService.SendRawEmailAsync(
                recipient,
                "potegni.me SMTP test",
                $"<p>Test message from the potegni.me API mail transport.</p><p>Sent at {timestamp}.</p>");

            Console.WriteLine("Accepted by the relay.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{Constants.Constants.InternalErrorCode} test mail failed: {ex.Message}");
            return 1;
        }
    }
}
