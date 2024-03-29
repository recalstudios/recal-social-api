﻿namespace recal_social_api.Models;

public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime Created { get; set; }
    public string? RevokationDate { get; set; }
    public string? ManuallyRevoked { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int? ReplacesId { get; set; }
    public int ReplacedById { get; set; }
    public int UserId { get; set; }
}