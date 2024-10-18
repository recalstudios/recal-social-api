namespace recal_social_api.Models.Requests;

public class ResetPassphraseRequest
{
    public string ResetToken { get; set; } = null!;
    public string NewPassphrase { get; set; } = null!;
}
