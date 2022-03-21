namespace recal_social_api.Models;

public class Credentials
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Token { get; set; } = null!;
    public int AccessLevel { get; set; }
}