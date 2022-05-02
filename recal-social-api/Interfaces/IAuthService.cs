using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using recal_social_api.Models;
using recal_social_api.Models.Responses;

namespace recal_social_api.Interfaces;

public interface IAuthService
{
    public string GenerateRefreshToken(int userId);
    public GetRefreshTokenResponse GetRefreshToken(string token);
    public User VerifyCredentials(string username, string pass);

    public bool UpdateCredentials(int userId, string pass, string newPass);
    public string GetNewAuthToken(string username, string pass);
    public string NewRefreshToken(string oldToken);
    public string GetAuthToken(string username);
    public string LogOut(string token);
    public string LogOutAll(string userId);
}