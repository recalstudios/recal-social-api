namespace recal_social_api.Models.Requests;

public class UpdateChatroomRequest
{
    public int ChatroomId { get; set; }
    public string? Name { get; set; } = null!;
    public string? Image { get; set; } = null!;
    public string? Pass { get; set; } = null!;
}