namespace recal_social_api.Models.Requests;

public class UpdateUserRequest
{
    public string? Username { get; set; } = null!;
    public string? Password { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string? Pfp { get; set; } = null!;

}