namespace recal_social_api.Models;

public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public string Token { get; set; }
    public DateTime Created { get; set; }
    public DateTime RevokationDate { get; set; }
    public bool ManuallyRevoked { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int ReplacesId { get; set; }
    public int ReplacedById { get; set; }
    public int UserID { get; set; }
}