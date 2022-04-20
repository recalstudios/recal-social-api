namespace recal_social_api.Models.Requests;

public class CreateUserRequest
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Pass { get; set; } = null!;
}