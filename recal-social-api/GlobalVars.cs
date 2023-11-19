namespace recal_social_api;

public static class GlobalVars
{
    public const int RefreshTokenAgeDays = 5;
    public const int AuthTokenAgeMinutes = 7200; // AuthService line 344

    // Database settings
    public static readonly string DatabaseConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? throw new ArgumentNullException();

    // Mail settings
    public static readonly string MailServer = Environment.GetEnvironmentVariable("MAIL_SERVER") ?? throw new ArgumentNullException();
    public static readonly int MailServerPort = Convert.ToInt32(Environment.GetEnvironmentVariable("MAIL_SERVER_PORT") ?? "587");
    public static readonly string MailSenderName = Environment.GetEnvironmentVariable("MAIL_SENDER_NAME") ?? "Recal Social";
    public static readonly string MailSenderEmail = Environment.GetEnvironmentVariable("MAIL_SENDER_EMAIL") ?? throw new ArgumentNullException();
    public static readonly string MailUsername = Environment.GetEnvironmentVariable("MAIL_USERNAME") ?? throw new ArgumentNullException();
    public static readonly string MailPassword = Environment.GetEnvironmentVariable("MAIL_PASSWORD") ?? throw new ArgumentNullException();
}
