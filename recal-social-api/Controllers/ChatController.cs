using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recal_social_api.Interfaces;

namespace recal_social_api.Controllers;
[ApiController]
[Route("chat")]
public class ChatController : Controller
{ 
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public ChatController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }
    
    [Authorize]
    [HttpPost("")]
    public Task<IActionResult> GetUser()
    {
        /*
        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];
        
        //  Cuts out the Bearer part of the header
        var stream = authHeader.Substring("Bearer ".Length).Trim();
        
        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;
        
        //  Sets the variable username to the username from the token
        var username = tokenS!.Claims.First(claim => claim.Type == "Username").Value;
        var retUser = _userService.GetUser(username);

        if (string.IsNullOrEmpty(retUser.Username))
        {
            return Task.FromResult<IActionResult>(NotFound("Username does not exist"));
        }

        return Task.FromResult<IActionResult>(Ok(retUser));
        */
        throw new NotImplementedException();
    }
}