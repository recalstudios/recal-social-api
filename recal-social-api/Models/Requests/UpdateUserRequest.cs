namespace recal_social_api.Models.Requests;

public class UpdateUserRequest
{
    public string Token { get; set; } = null!;
    public string? FirstName { get; set; } = null!;
    public string? LastName { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public int? PhoneNumber { get; set; }
    public string? Pfp { get; set; } = null!;
}