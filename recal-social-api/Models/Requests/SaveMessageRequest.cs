namespace recal_social_api.Models.Requests;

public class SaveMessageRequest
{
    public int Room { get; set; }
    public MessageContent Content { get; set; } = null!;
}