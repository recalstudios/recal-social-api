using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recal_social_api.Interfaces;
using recal_social_api.Models.Requests;
using recal_social_api.Models.Responses;

namespace recal_social_api.Controllers;

[ApiController]
[Route("v1/user")]

public class UserController(IUserService userService, IAuthService authService) : Controller
{
    [Authorize]
    [HttpPost("user")]
    // Get user with the username in jwt token
    public Task<IActionResult> GetUser()
    {
        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var username = tokenS!.Claims.First(claim => claim.Type == "Username").Value;
        var retUser = userService.GetUser(username);

        //  Runs the service
        return string.IsNullOrEmpty(retUser.Username) ? Task.FromResult<IActionResult>(NotFound("Username does not exist")) : Task.FromResult<IActionResult>(Ok(retUser));
    }

    [AllowAnonymous]
    [HttpPost("create")]
    // Create a user using username, email and password
    public bool CreateUser([FromBody] CreateUserRequest payload)
    {
        // Runs the service
        return userService.CreateUser(payload.Username, payload.Email, payload.Pass);
    }

    [AllowAnonymous]
    [HttpPost("user/public")]
    // Gets the public part of a user using the userid
    public PublicGetUserResponse PublicGetUser([FromBody]PublicGetUserRequest payload)
    {
        return userService.PublicGetUser(payload.UserId);
    }

    [Authorize]
    [HttpDelete("delete")]
    // Deletes user using userid from auth token
    public bool DeleteUser()
    {
        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);

        // Return false if the jsonToken is wrong or something
        if (jsonToken is not JwtSecurityToken tokenS) return false;

        //  Sets the variable username to the username from the token
        var username = tokenS.Claims.First(claim => claim.Type == "Username").Value;
        var userId = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
        userService.DeleteUser(username);

        // Logs out all refresh tokens
        var logout = authService.LogOutAll(userId);

        // Returns true if logged out
        return logout == "Success";

    }

    [Authorize]
    [HttpPost("update")]
    // Update the user. User id is from auth token and rest is from request body
    public bool UpdateUser([FromBody] UpdateUserRequest payload)
    {
        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);

        // Return false if the jsonToken is wrong or something
        if (jsonToken is not JwtSecurityToken tokenS) return false;

        //  Sets the variable username to the username from the token
        var userId = tokenS.Claims.First(claim => claim.Type == "UserId").Value;

        // Runs the service
        return userService.UpdateUser( int.Parse(userId),  payload.Username, payload.Email, payload.Pfp);
    }

    [HttpPost("request-passphrase-reset")]
    public bool RequestPassphraseReset([FromBody] RequestPassphraseResetRequest payload)
    {
        return userService.SendPassphraseResetEmail(payload.Email);
    }

    [HttpPost("reset-passphrase")]
    public bool ResetUserPassphrase([FromBody] ResetPassphraseRequest payload)
    {
        return userService.ResetUserPassphraseUsingResetToken(payload.ResetToken, payload.NewPassphrase);
    }

    [Authorize]
    [HttpGet("rooms")]
    // Get user rooms with user id from auth token
    public IEnumerable<GetUserChatroomsResponse> GetUsersChatrooms()
    {
        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        // Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        // Sets the variable username to the username from the token
        // it might be safe to assume this is never null? i have no idea
        var userId = tokenS!.Claims.First(claim => claim.Type == "UserId").Value;

        //  Runs the service
        return userService.GetUserChatrooms(int.Parse(userId));
    }
}
