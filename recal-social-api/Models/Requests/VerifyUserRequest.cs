namespace recal_social_api.Models.Requests;

public class VerifyUserRequest
{
    public string Username { get; set;} = null!;
    public string Password { get; set;} = null!;
}