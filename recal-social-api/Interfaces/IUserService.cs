using recal_social_api.Models;
using recal_social_api.Models.Responses;

namespace recal_social_api.Interfaces;

public interface IUserService
{
    public GetUserResponse GetUser(string username);
    public GetUserResponse GetUserById(int userId);
    public PublicGetUserResponse PublicGetUser(int userId);
    public bool CreateUser(string username, string email, string pass, string pfp);
    public bool DeleteUser(string username);
    bool UpdateUser(int payloadUserId, string? payloadUsername,string? payloadPassword, string? payloadEmail, string? payloadPfp);
    public IEnumerable<GetUserChatroomsResponse> GetUserChatrooms(int userId);
}