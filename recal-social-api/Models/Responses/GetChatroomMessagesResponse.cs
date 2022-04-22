namespace recal_social_api.Models.Responses;

public class GetChatroomMessagesResponse
{
    public int RoomId { get; set; }
    public IEnumerable<Message> Messages { get; set; } = null!;
}