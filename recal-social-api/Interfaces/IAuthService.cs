using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using recal_social_api.Models;
using recal_social_api.Models.Responses;

namespace recal_social_api.Interfaces;

public interface IAuthService
{
    public string GenerateRefreshToken(int userId);
    public User VerifyCredentials(string username, string pass);
    public string GetNewAuthToken(string username, string pass);
    public string NewRefreshToken(string oldToken);
    public string GetAuthToken(string username);
    public bool UpdatePass(string user, string pass, string newPass);
}