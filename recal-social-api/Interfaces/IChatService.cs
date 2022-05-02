using recal_social_api.Models.Responses;

namespace recal_social_api.Interfaces;

public interface IChatService
{ 
// Message part of the interface
    public GetChatroomMessagesResponse GetChatroomMessages(int ChatroomId, int UserId, int Start, int Lenght);
    public int SaveChatMessage(int UserId, string Data, int RoomId);
    public bool DeleteChatMessage(int MessageId, int UserId);

// Room part of the interface
    public bool CreateChatroom(string Name, string Pass, int UserId);
}