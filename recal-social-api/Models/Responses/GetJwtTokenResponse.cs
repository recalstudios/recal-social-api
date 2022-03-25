namespace recal_social_api.Models.Responses;

public class GetJwtTokenResponse
{
    public string AuthToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}