namespace recal_social_api.Models.Requests;

public class UpdateCredentialsRequest
{
    public string Pass { get; set; } = null!;
    public string NewPass { get; set; } = null!;
}