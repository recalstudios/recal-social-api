namespace recal_social_api.Models.Requests;

public class GetChatroomMessagesRequests
{
    public int ChatroomId { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
}