using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recal_social_api.Interfaces;
using recal_social_api.Models.Requests;
using recal_social_api.Models.Responses;

namespace recal_social_api.Controllers;

[ApiController]
[Route("v1/auth")]
public class AuthController(IAuthService authService, IUserService userService) : Controller
{
    [Authorize]
    [HttpPost("update/pass")]
    // Updating the password using auth token, old password and new password
    public bool UpdateCredentials([FromBody] UpdateCredentialsRequest payload)
    {
        // Get the http request headers
        var httpContext = HttpContext;
        // i think it is safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = httpContext.Request.Headers["Authorization"]!;

        // Cuts out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        // Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);

        // Stop if the token is invalid or something idk
        if (jsonToken is not JwtSecurityToken tokenS) return false;

        // Set the variable userid to the userid from the token
        var userId = tokenS.Claims.First(claim => claim.Type == "UserId").Value;

        // Run update credentials
        return authService.UpdateCredentials(int.Parse(userId), payload.Pass, payload.NewPass);

    }

    [AllowAnonymous]
    [HttpPost("token/new")]
    // Creates a new AuthToken and a renew token
    public IActionResult Post([FromBody] VerifyUserRequest payload)
    {
        // Creates a response that contains both auth and refresh token
        var jwtResponse = new JwtResponse();

        // Inputs the username and password into a function
        var result = authService.GetNewAuthToken(payload.Username, payload.Password);

        // Return error if anything goes wrong
        switch (result)
        {
            case "BadRequest": return BadRequest(HttpStatusCode.BadRequest);
            case "Unauthorized": return Unauthorized(HttpStatusCode.Unauthorized);
        }

        // Insert the auth token into the response
        jwtResponse.AuthToken = result;
        var user = userService.GetUser(payload.Username);

        // Insert the refresh token into the response
        var responseToken = authService.GenerateRefreshToken(user.Id) ?? throw new InvalidOperationException();
        jwtResponse.RefreshToken = responseToken;

        // Return auth token and refresh token
        return Ok(jwtResponse);
    }

    [AllowAnonymous]
    [HttpPost("token/test")]
    // Check the expiration of auth tokens
    public bool CheckExpiration()
    {
        // Get the http request headers
        var httpContext = HttpContext;
        // i think it is safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = httpContext.Request.Headers["Authorization"]!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        // Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        // Gets expirations from token and current time
        var expiration = double.Parse(tokenS!.Claims.First(claim => claim.Type == "exp").Value);

        // Converts to datetime
        var expirationDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        expirationDate = expirationDate.AddSeconds(expiration).ToLocalTime();

        // Sets now to Datetime Now
        var now = DateTime.Now;

        //  Returns less than zero if expiration is before now
        var compare = DateTime.Compare(expirationDate, now);

        //  Returns true if the token is expired and false if it still works
        return compare >= 0;
    }

    [AllowAnonymous]
    [HttpPost("token/renew")]
    // Renews the tokens using a renew token
    public IActionResult ChainToken()
    {
        // Creates a response with an auth and renew token
        var result = new JwtResponse();

        // Get the http request headers
        var httpContext = HttpContext;
        // i think it is safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = httpContext.Request.Headers["Authorization"]!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Gets the token and userid from jwt header
        var oldToken = tokenS!.Claims.First(claim => claim.Type == "Token").Value;
        var userId = tokenS.Claims.First(claim => claim.Type == "UserId").Value;

        // Gets info regarding old refresh token
        var oldRefreshToken = authService.GetRefreshToken(oldToken);

        // Gets user information
        var userdata = userService.GetUserById(int.Parse(userId));

        // Does not work if it is not active, revoked or expired
        if (userdata.Active != 1 | oldRefreshToken.ManuallyRevoked == 1)
        {
            return Unauthorized(HttpStatusCode.Unauthorized);
        }
        if (DateTime.Parse(oldRefreshToken.ExpiresAt!) <= DateTime.UtcNow){
            return BadRequest(HttpStatusCode.BadRequest);
        }

        // Gets user from DB
        var user = userService.GetUserById(int.Parse(userId));

        // Creates new refresh token
        var newRefreshToken = authService.NewRefreshToken(oldToken);

        // if no username, returns user fetch error
        if (user.Username == null) throw new Exception("Something went wrong when fetching user");

        // Otherwise, continue
        // Creates new auth token
        var newAuthToken = authService.GetAuthToken(user.Username);

        // Inputs the auth and refresh token into the response
        result.AuthToken = newAuthToken;
        result.RefreshToken = newRefreshToken;

        // Returns the tokens
        return Ok(result);
    }


    [Authorize]
    [HttpPost("token/logout")]
    // Logout the currently used refresh token
    public IActionResult Logout()
    {
        // Get the http request headers
        var httpContext = HttpContext;
        // i think it is safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = httpContext.Request.Headers["Authorization"]!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Gets token from
        var token = tokenS!.Claims.First(claim => claim.Type == "Token").Value;

        // Gets info on old token
        var oldRefreshToken = authService.GetRefreshToken(token);

        // If its expired or revoked, doesnt work
        if (DateTime.Parse(oldRefreshToken.ExpiresAt!) <= DateTime.UtcNow | oldRefreshToken.ManuallyRevoked == 1) return Unauthorized(HttpStatusCode.Unauthorized);

        // Logout function in service
        var logout = authService.LogOut(token);

        // Returns true if successfully logged out
        // If anything fails, returns false
        return Ok(logout == "Success");
    }

    [Authorize]
    [HttpPost("token/logout/all")]
    // Log out all refresh tokens associated with a user
    public Task<IActionResult> LogoutAll()
    {
        // Get the http request headers
        var httpContext = HttpContext;
        // i think it is safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = httpContext.Request.Headers["Authorization"]!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Get claims from the token
        var token = tokenS!.Claims.First(claim => claim.Type == "Token").Value;
        var userId = tokenS.Claims.First(claim => claim.Type == "UserId").Value;

        var cookieRefreshToken = authService.GetRefreshToken(token);

        // If its expired or revoked, doesnt work
        if (DateTime.Parse(cookieRefreshToken.ExpiresAt!) <= DateTime.UtcNow | cookieRefreshToken.ManuallyRevoked == 1) return Task.FromResult<IActionResult>(Unauthorized("Token is expired"));

        // Logout
        var logout = authService.LogOutAll(userId);

        // Returns ok if successful, otherwise returns bad request
        return logout switch
        {
            "Success" => Task.FromResult<IActionResult>(Ok("Logged Out")),
            _ => Task.FromResult<IActionResult>(BadRequest("An unknown error has occurred"))
        };
    }
}
