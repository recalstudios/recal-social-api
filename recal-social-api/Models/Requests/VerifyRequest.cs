namespace recal_social_api.Models.Requests;

public class VerifyRequest
{
    public string Username { get; set; } = null!;
    public string Pass { get; set; } = null!;
}