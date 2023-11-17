using recal_social_api.Models;
using recal_social_api.Models.Responses;

namespace recal_social_api.Interfaces;

public interface IUserService
{
    public User GetUser(string username);
    public User GetUserById(int userId);
    public PublicGetUserResponse PublicGetUser(int userId);
    public bool CreateUser(string username, string email, string pass);
    public bool DeleteUser(string username);
    // UpdateUser wasn't public before, but i think it should be?
    public bool UpdateUser(int payloadUserId, string? payloadUsername, string? payloadEmail, string? payloadPfp);
    public bool SendPassphraseResetEmail(string emailAddress);
    public bool ResetUserPassphraseUsingResetToken(string resetToken, string newPassphrase);
    public IEnumerable<GetUserChatroomsResponse> GetUserChatrooms(int userId);
}
