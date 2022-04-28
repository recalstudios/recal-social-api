namespace recal_social_api.Models.Requests;

public class SaveMessageRequest
{
    public string Data { get; set; } = null!;
    public int ChatroomId { get; set; }
}