using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using recal_social_api.Interfaces;
using recal_social_api.Models.Requests;
using recal_social_api.Models.Responses;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace recal_social_api.Controllers;


[Route("auth/token")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public TokenController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("new")]
    public Task<IActionResult> Post([FromBody] VerifyUserRequest payload)
    {
        var response = new GetJwtTokenResponse();
        
        var result = _authService.GetToken(payload.Username, payload.Password);
        // Returns error if anything goes wrong
            if(result == "BadRequest")
            { return Task.FromResult<IActionResult>(BadRequest("Bad request")); }
            if (result == "Invalid credentials")
            { return Task.FromResult<IActionResult>(BadRequest("Invalid credentials")); }
        
        // Inserts the authtoken into the response
            response.AuthToken = result;
            var user = _userService.GetUser(payload.Username);
        
        // Inserts the refreshtoken into the response
            var responsetoken = _authService.GenerateRefreshToken(user.Id) ?? throw new InvalidOperationException();
            response.RefreshToken = responsetoken;
            return Task.FromResult<IActionResult>(Ok(response));

    }

    [AllowAnonymous]
    [HttpPost]
    public Task<IActionResult> RefreshToken([FromHeader] string yourmom)
    {
        throw new NotImplementedException();
    }
    
}
