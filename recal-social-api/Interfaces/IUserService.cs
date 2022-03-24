using recal_social_api.Models;
using recal_social_api.Models.Responses;

namespace recal_social_api.Interfaces;

public interface IUserService
{
    public GetUserResponse GetUser(string username);
    public bool CreateUser(string username, string email, string pass, string pfp);
    public bool DeleteUser(string username);
    bool UpdateUser(string payloadToken, string? payloadFirstName, string? payloadLastName, string? payloadEmail, int? payloadPhoneNumber, string? payloadPfp);
}