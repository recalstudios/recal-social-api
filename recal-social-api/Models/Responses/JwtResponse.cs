namespace recal_social_api.Models.Responses;

public class JwtResponse
{
    public string AuthToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}