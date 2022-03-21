using Microsoft.AspNetCore.Mvc;
using recal_social_api.Interfaces;
using recal_social_api.Models;
using recal_social_api.Models.Requests;

namespace recal_social_api.Controllers;


[ApiController]
[Route("auth")]

public class AuthController : Controller
{

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    public string VerifyCredentials([FromBody] VerifyRequest payload)
    {
        return _authService.VerifyCredentials(payload.User, payload.Pass);
    }

    [HttpPost("update")]
    public bool UpdatePass([FromBody] UpdateCredentialsRequest payload)
    {
        return _authService.UpdatePass(payload.Token, payload.Pass, payload.NewPass);
    }
    
}