using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recal_social_api.Interfaces;
using recal_social_api.Models.Requests;
using recal_social_api.Models.Responses;

namespace recal_social_api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [Authorize]
    [HttpPost("update/pass")]
    // Updating the password using authtoken, old password and new password
    public bool UpdateCredentials([FromBody] UpdateCredentialsRequest payload)
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

        //  Sets the variable userid to the userid from the token
        var userId = tokenS.Claims.First(claim => claim.Type == "UserId").Value;

        // Runs update credentials
        return _authService.UpdateCredentials(int.Parse(userId), payload.Pass, payload.NewPass);
    }

    [AllowAnonymous]
    [HttpPost("token/new")]
    // Creates a new AuthToken and a renewtoken
    public IActionResult Post([FromBody] VerifyUserRequest payload)
    {
        // Creates a response that contains both auth and refresh token
        var jwtResponse = new JwtResponse();

        // Inputs the username and password into a function
        var result = _authService.GetNewAuthToken(payload.Username, payload.Password);
        // Returns error if anything goes wrong
            if(result == "BadRequest")
            { return BadRequest(HttpStatusCode.BadRequest); }
            if (result == "Unauthorized")
            { return Unauthorized(HttpStatusCode.Unauthorized); }

        // Inserts the authtoken into the response
            jwtResponse.AuthToken = result;
            var user = _userService.GetUser(payload.Username);

        // Inserts the refreshtoken into the response
            var responsetoken = _authService.GenerateRefreshToken(user.Id) ?? throw new InvalidOperationException();
            jwtResponse.RefreshToken = responsetoken;

        // Returns authtoken and refreshtoken
        return Ok(jwtResponse);
    }

    [AllowAnonymous]
    [HttpPost("token/test")]
    // Check the expiration of auth tokens
    public bool CheckExpiration()
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

        //  Gets expirations from token and current time
        var expiration = double.Parse(tokenS!.Claims.First(claim => claim.Type == "exp").Value);

        // Converts to datetime
        DateTime expirationDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
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
    // Renews the tokens using a renewtoken
    public IActionResult ChainToken()
    {
        // Creates a response with an auth and renewtoken
        var result = new JwtResponse();

        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];

        //  Cuts out the Bearer part of the header
        var stream = authHeader.Substring("Bearer ".Length).Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Gets the token and userid from jwt header
        var oldToken = tokenS!.Claims.First(claim => claim.Type == "Token").Value;
        var userId = tokenS!.Claims.First(claim => claim.Type == "UserId").Value;

        // Gets info regarding old refreshtoken
        var oldRefreshToken = _authService.GetRefreshToken(oldToken);

        // Gets user information
        var userdata = _userService.GetUserById(int.Parse(userId));

        // Does not work if it is not active, revoked or expired
        if (userdata.Active != 1 | oldRefreshToken.ManuallyRevoked == 1)
        {
            return Unauthorized(HttpStatusCode.Unauthorized);
        }
        if (DateTime.Parse(oldRefreshToken.ExpiresAt!) <= DateTime.UtcNow){
            return BadRequest(HttpStatusCode.BadRequest);
        }

        // Gets user from DB
        var user = _userService.GetUserById(Int32.Parse(userId));

        // Creates new refreshtoken
        var newRefreshToken = _authService.NewRefreshToken(oldToken);

        // if no username, returns user fetch error
        if (user.Username == null) throw new Exception("Something went wrong when fetching user");

        // Otherwise, continue
        // Creates new auth token
        var newAuthToken = _authService.GetAuthToken(user.Username);

        // Inputs the auth and refreshtoken into the response
        result.AuthToken = newAuthToken;
        result.RefreshToken = newRefreshToken;

        // Returns the tokens
        return Ok(result);
    }


    [Authorize]
    [HttpPost("token/logout")]
    // Logout the currently used refreshtoken
    public IActionResult Logout()
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

        //  Gets token from
        var token = tokenS!.Claims.First(claim => claim.Type == "Token").Value;

        // Gets info on old token
        var oldRefreshToken = _authService.GetRefreshToken(token);

        // If its expired or revoked, doesnt work
        if (DateTime.Parse(oldRefreshToken.ExpiresAt!) <= DateTime.UtcNow | oldRefreshToken.ManuallyRevoked == 1){
            return Unauthorized(HttpStatusCode.Unauthorized);
        }

        // Logout function in service
        var logout = _authService.LogOut(token);

        // Returns true if successfully logged out
        if (logout == "Success") return Ok(true);

        // If anything fails, returns false
        return Ok(false);
    }


    [Authorize]
    [HttpPost("token/logout/all")]
    // Log out all refreshtokens associated with a user
    public Task<IActionResult> LogoutAll()
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

        //  Get claims from the token
        var token = tokenS!.Claims.First(claim => claim.Type == "Token").Value;
        var userId = tokenS!.Claims.First(claim => claim.Type == "UserId").Value;

        var cookieRefreshToken = _authService.GetRefreshToken(token);

        // If its expired or revoked, doesnt work
        if (DateTime.Parse(cookieRefreshToken.ExpiresAt!) <= DateTime.UtcNow | cookieRefreshToken.ManuallyRevoked == 1){
            return Task.FromResult<IActionResult>(Unauthorized("Token is expired"));
        }

        // Logout
        var logout = _authService.LogOutAll(userId);

        // Returns ok if successful, otherwise returns bad request
        return logout switch
        {
            "Success" => Task.FromResult<IActionResult>(Ok("Logged Out")),
            _ => Task.FromResult<IActionResult>(BadRequest("An unknown error has occurred"))
        };
    }
}
