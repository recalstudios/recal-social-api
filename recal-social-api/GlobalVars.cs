namespace recal_social_api;

public static class GlobalVars
{
    public const int RefreshTokenAgeDays = 5;
    public const int AuthTokenAgeMinutes = 7200; // AuthService line 344

    // Common options
    public static readonly int ApiPort = Convert.ToInt32(Environment.GetEnvironmentVariable("RECAL_SOCIAL_API_PORT") ?? "80");
    public static readonly string DatabaseConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? throw new ArgumentNullException();
    public static readonly bool EnableMailService = Convert.ToBoolean(Environment.GetEnvironmentVariable("ENABLE_MAIL_SERVICE") ?? throw new ArgumentNullException());

    // Initialize these mail options in the constructor instead of here to allow for conditional statements
    public static readonly string? MailServer;
    public static readonly int MailServerPort;
    public static readonly string? MailSenderName;
    public static readonly string? MailSenderEmail;
    public static readonly string? MailUsername;
    public static readonly string? MailPassword;

    static GlobalVars()
    {
        // Don't initialise mail options if the mail service is disabled
        if (!EnableMailService) return;

        MailServer = Environment.GetEnvironmentVariable("MAIL_SERVER") ?? throw new ArgumentNullException();
        MailServerPort = Convert.ToInt32(Environment.GetEnvironmentVariable("MAIL_SERVER_PORT") ?? "587");
        MailSenderName = Environment.GetEnvironmentVariable("MAIL_SENDER_NAME") ?? "Recal Social";
        MailSenderEmail = Environment.GetEnvironmentVariable("MAIL_SENDER_EMAIL") ?? throw new ArgumentNullException();
        MailUsername = Environment.GetEnvironmentVariable("MAIL_USERNAME") ?? throw new ArgumentNullException();
        MailPassword = Environment.GetEnvironmentVariable("MAIL_PASSWORD") ?? throw new ArgumentNullException();
    }
}
