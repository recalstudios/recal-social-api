namespace recal_social_api.Models.Requests;

public class VerifyRequest
{
    public string User { get; set; } = null!;
    public string Pass { get; set; } = null!;
}