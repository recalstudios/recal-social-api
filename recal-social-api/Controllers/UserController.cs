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
    public GetUserResponse GetUser()
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
            return new GetUserResponse()
            {
                Status = "User not availible or found"
            };
        }

        return retUser;

    }
    
    [HttpPost("create")]
    public bool CreateUser([FromBody] CreateUserRequest payload)
    {
        return _userService.CreateUser(payload.Username, payload.Email,  payload.Pass, payload.Pfp);
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

    [HttpPost("update")]
    public bool UpdateUser([FromBody] UpdateUserRequest payload)
    {
        return _userService.UpdateUser(payload.Token, payload.FirstName, payload.LastName, payload.Email, payload.PhoneNumber, payload.Pfp);
    }
}