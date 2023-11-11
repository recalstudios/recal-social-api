using recal_social_api.Models;
using recal_social_api.Models.Responses;

namespace recal_social_api.Interfaces;

public interface IChatService
{
    // Message part of the interface
    public GetChatroomMessagesResponse GetChatroomMessages(int chatroomId, int userId, int start, int lenght);
    public Message SaveChatMessage(int userId, int roomId, MessageContent content);
    public bool DeleteChatMessage(int messageId, int userId);

    // Room part of the interface
    public bool CreateChatroom(string name, string pass, int userId);

    public Chatroom DetailsChatroom(int userId, int chatroomId);

    public bool UpdateChatroom(int userId, int chatroomId, string? payloadName, string? payloadImage, string? payloadPass);

    public bool DeleteChatroom(int userId, int chatroomId);

    public bool JoinChatroom(string code, string pass, int userId);

    public bool LeaveChatroom(int userId, int chatroomId);
}
