namespace recal_social_api.Models.Requests;

public class ResetPassphraseRequest
{
    public string ResetToken { get; set; }
    public string NewPassphrase { get; set; }
}
