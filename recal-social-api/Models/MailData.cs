namespace recal_social_api.Models;

public class MailData
{
    public string RecipientEmail { get; set; } = null!;
    public string RecipientName { get; set; } = null!;
    public string EmailSubject { get; set; } = null!;
    public string EmailBody { get; set; } = null!;
}
