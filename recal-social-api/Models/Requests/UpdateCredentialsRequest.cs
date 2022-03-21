namespace recal_social_api.Models.Requests;

public class UpdateCredentialsRequest
{
    public string Token { get; set; } = null!;
    public string Pass { get; set; } = null!;
    public string NewPass { get; set; } = null!;
}