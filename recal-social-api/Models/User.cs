namespace recal_social_api.Models;

public class User
{
    public int Id { get; set;}
    public string? Username { get; set;} = null!;
    public string? Password { get; set;} = null!;
    public string? Email { get; set;} = null!;
    public string? Pfp { get; set;} = null!;
    public int AccessLevel { get; set; }
}