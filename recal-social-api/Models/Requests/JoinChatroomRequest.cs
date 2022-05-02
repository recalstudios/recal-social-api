namespace recal_social_api.Models.Requests;

public class JoinChatroomRequest
{
    public string Code { get; set; } = null!;
    public string Pass { get; set; } = null!;
}