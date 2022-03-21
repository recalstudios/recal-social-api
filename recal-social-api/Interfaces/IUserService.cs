using recal_social_api.Models;

namespace recal_social_api.Interfaces;

public interface IUserService
{
    public User GetUser(string token);
    public bool CreateUser(string firstName, string lastName, string username, string email, int phoneNumber, string pass, string pfp);
    public bool DeleteUser(string username);
    bool UpdateUser(string payloadToken, string? payloadFirstName, string? payloadLastName, string? payloadEmail, int? payloadPhoneNumber, string? payloadPfp);
}