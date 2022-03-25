using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using recal_social_api.Interfaces;
using recal_social_api.Models;
using recal_social_api.Models.Requests;
using recal_social_api.Services;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace recal_social_api.Controllers;


[Route("auth/token/new")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly IAuthService _authService;
    

    public TokenController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost]
    public Task<IActionResult> Post([FromBody] VerifyUserRequest payload)
    {
        string result = _authService.GetToken(payload.Username, payload.Password);
        if(result == "BadRequest")
        {
            return Task.FromResult<IActionResult>(BadRequest("Bad request"));
        }

        if (result == "Invalid credentials")
        {
            return Task.FromResult<IActionResult>(BadRequest("Invalid credentials"));
        }

        return Task.FromResult<IActionResult>(Ok(result));

    }
    
}
