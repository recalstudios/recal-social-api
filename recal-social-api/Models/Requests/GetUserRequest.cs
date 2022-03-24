namespace recal_social_api.Models.Requests;

public class GetUserRequest
{
    public string Username { get; set;} = null!;
    public string Password { get; set;} = null!;
}