namespace PotegniMe.Services.EmailService;

/// <summary>
/// Single source of truth for outgoing mail. Every value comes from the environment,
/// nothing mail related lives in appsettings.json or in code.
/// </summary>
public class MailConfig
{
    public const string FromDisplayName = "potegni.me";

    public string Host { get; }
    public int Port { get; }
    public string Username { get; }
    public string Password { get; }
    public string FromAddress { get; }
    public string ReplyTo { get; }

    public MailConfig()
    {
        Host = RequiredEnv("SMTP_HOST");
        Username = RequiredEnv("SMTP_USER");
        Password = RequiredEnv("SMTP_PASS");
        FromAddress = RequiredEnv("MAIL_FROM");
        ReplyTo = RequiredEnv("MAIL_REPLY_TO");

        string port = RequiredEnv("SMTP_PORT");
        if (!int.TryParse(port, out int parsedPort) || parsedPort <= 0 || parsedPort > 65535)
        {
            throw new ArgumentException($"{Constants.Constants.DotEnvErrorCode} SMTP_PORT is not a valid port: {port}");
        }
        Port = parsedPort;
    }

    private static string RequiredEnv(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value)) throw new Exception($"{Constants.Constants.DotEnvErrorCode} {name}");
        return value;
    }
}
