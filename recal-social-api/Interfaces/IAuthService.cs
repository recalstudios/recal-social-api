using recal_social_api.Models;

namespace recal_social_api.Interfaces;

public interface IAuthService
{
    public User VerifyCredentials(string username, string pass);
    public bool UpdatePass(string user, string pass, string newPass);
}