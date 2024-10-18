namespace recal_social_api.Models;

public class User
{
    public int Id { get; set;}
    public string? Username { get; set;}
    public string? Password { get; set;}
    public string? Email { get; set;}
    public string? Pfp { get; set;}
    public int AccessLevel { get; set; }
    public int Active { get; set; }
}
