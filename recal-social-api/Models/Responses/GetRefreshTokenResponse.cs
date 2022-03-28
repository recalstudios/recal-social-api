namespace recal_social_api.Models.Responses;

public class GetRefreshTokenResponse
{
    public int RefreshTokenId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime Created { get; set; }
    public string? RevokationDate { get; set; }
    public string? ManuallyRevoked { get; set; }
    public string? ExpiresAt { get; set; }
    public int ReplacedById { get; set; }
    public int UserId { get; set; }
}