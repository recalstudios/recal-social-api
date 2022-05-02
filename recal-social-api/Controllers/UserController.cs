using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recal_social_api.Interfaces;
using recal_social_api.Models;
using recal_social_api.Models.Requests;
using recal_social_api.Models.Responses;

namespace recal_social_api.Controllers;

[ApiController]
[Route("user")]

public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpPost("user")]
    public Task<IActionResult> GetUser()
    {
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

    }
    
    [AllowAnonymous]
    [HttpPost("create")]
    public bool CreateUser([FromBody] CreateUserRequest payload)
    {
        return _userService.CreateUser(payload.Username, payload.Email, payload.Pass);
    }

    [AllowAnonymous]
    [HttpPost("user/public")]
    public PublicGetUserResponse PublicGetUser([FromBody]PublicGetUserRequest payload)
    {
        return _userService.PublicGetUser(payload.UserId);
    }

    [Authorize]
    [HttpDelete("delete")]
    public bool DeleteUser()
    {
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
        var username = tokenS.Claims.First(claim => claim.Type == "Username").Value;
        return _userService.DeleteUser(username);
    }

    [Authorize]
    [HttpPost("update")]
    public bool UpdateUser([FromBody] UpdateUserRequest payload)
    {
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
        var userId = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
        
        return _userService.UpdateUser( int.Parse(userId),  payload.Username, payload.Email, payload.Pfp);
    }

    [Authorize]
    [HttpGet("rooms")]
    public IEnumerable<GetUserChatroomsResponse> GetUsersChatrooms()
    {
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
        var userId = tokenS.Claims.First(claim => claim.Type == "UserId").Value;

        return _userService.GetUserChatrooms(int.Parse(userId));
    }
}