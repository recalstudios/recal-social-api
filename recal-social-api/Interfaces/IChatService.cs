using recal_social_api.Models.Responses;

namespace recal_social_api.Interfaces;

public interface IChatService
{ 
    public GetChatroomMessagesResponse GetChatroomMessages(int ChatroomId, int UserId, int Start, int Lenght);
    public int SaveChatMessage(int UserId, string Data, int RoomId);
}