namespace recal_social_api.Models.Requests;

public class SaveMessageRequest
{
    public int ChatroomId { get; set; }
    public MessageContent Content { get; set; } = null!;
}