using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using recal_social_api.Models;
using recal_social_api.Models.Responses;

namespace recal_social_api.Interfaces;

public interface IAuthService
{
    public User VerifyCredentials(string username, string pass);
    public string GetToken(string username, string pass);
    public bool UpdatePass(string user, string pass, string newPass);
}