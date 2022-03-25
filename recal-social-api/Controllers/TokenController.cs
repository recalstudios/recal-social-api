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


[Route("auth/token")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly IAuthService _authService;
    

    public TokenController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("new")]
    public Task<IActionResult> Post([FromBody] VerifyUserRequest payload)
    {
        var result = _authService.GetToken(payload.Username, payload.Password);
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

    [AllowAnonymous]
    [HttpPost]
    public Task<IActionResult> RefreshToken([FromHeader] string yourmom)
    {
        throw new NotImplementedException();
    }
    
}
