namespace recal_social_api.Models.Requests;

public class CreateChatroomRequest
{
    public string Name { get; set; } = null!;
    public string Pass { get; set; } = null!;
}