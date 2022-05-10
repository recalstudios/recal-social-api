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
    private readonly IAuthService _authService;

    public UserController(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    [Authorize]
    [HttpPost("user")]
    // Get user with the username in jwt token
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

        //  Runs the service
        return string.IsNullOrEmpty(retUser.Username) ? Task.FromResult<IActionResult>(NotFound("Username does not exist")) : Task.FromResult<IActionResult>(Ok(retUser));
    }
    
    [AllowAnonymous]
    [HttpPost("create")]
    // Create a user using username, email and password
    public bool CreateUser([FromBody] CreateUserRequest payload)
    {
        // Runs the service
        return _userService.CreateUser(payload.Username, payload.Email, payload.Pass);
    }

    [AllowAnonymous]
    [HttpPost("user/public")]
    // Gets the public part of a user using the userid
    public PublicGetUserResponse PublicGetUser([FromBody]PublicGetUserRequest payload)
    {
        return _userService.PublicGetUser(payload.UserId);
    }

    [Authorize]
    [HttpDelete("delete")]
    // Deletes user using userid from auth token
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
        var userId = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
        _userService.DeleteUser(username);

        // Logs out all refreshtokens
        var logout = _authService.LogOutAll(userId);
        
        // Returns true if logged out
        return logout == "Success";
    }

    [Authorize]
    [HttpPost("update")]
    // Update the user. User id is from auth token and rest is from request body
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
        
        // Runs the service
        return _userService.UpdateUser( int.Parse(userId),  payload.Username, payload.Email, payload.Pfp);
    }

    [Authorize]
    [HttpGet("rooms")]
    // Get user rooms with user id from authtoken
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

        //  Runs the service
        return _userService.GetUserChatrooms(int.Parse(userId));
    }
}