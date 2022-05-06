using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recal_social_api.Interfaces;
using recal_social_api.Models;
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
        
        return _authService.UpdateCredentials(int.Parse(userId), payload.Pass, payload.NewPass);
    }
    
    [AllowAnonymous]
    [HttpPost("token/new")]
    // Creates a new AuthToken and a renewtoken
    public JwtResponse Post([FromBody] VerifyUserRequest payload)
    {
        var JwtResponse = new JwtResponse();
        
        var result = _authService.GetNewAuthToken(payload.Username, payload.Password);
        // Returns error if anything goes wrong
            if(result == "BadRequest")
            { throw new BadHttpRequestException("Bad request"); }
            if (result == "Invalid credentials")
            { throw new UnauthorizedAccessException("Unauthorized"); }
        
        // Inserts the authtoken into the response
            JwtResponse.AuthToken = result;
            var user = _userService.GetUser(payload.Username);
        
        // Inserts the refreshtoken into the response
            var responsetoken = _authService.GenerateRefreshToken(user.Id) ?? throw new InvalidOperationException();
            JwtResponse.RefreshToken = responsetoken;
        
        // Returns authtoken and refreshtoken
        return JwtResponse;

    }

    [AllowAnonymous]
    [HttpPost("token/test")]
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
        
        //  Gets expirations form token and current time
        var expiration = double.Parse(tokenS!.Claims.First(claim => claim.Type == "exp").Value);
        
        // Converts to datetime
        DateTime expirationDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        expirationDate = expirationDate.AddSeconds(expiration).ToLocalTime();

        var now = DateTime.Now;
        
        
        //  Returns less than zero if expiration is before now
        var compare = DateTime.Compare(expirationDate, now);
        

        //  Returns true if the token is expired and false if it still works
        if (compare >= 0)
        {
            return true;
        }

        return false;
    }

    [AllowAnonymous]
    [HttpPost("token/renew")]
    public JwtResponse ChainToken()
    {
        var result = new JwtResponse();
        
        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];
        

        
        // Checks that the token is not null
        if (authHeader == null)
        {
            throw new BadHttpRequestException("Token cannot be null");
        }
        
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
        
        // If its expired or revoked, doesnt work
        if (DateTime.Parse(oldRefreshToken.ExpiresAt!) <= DateTime.UtcNow){
            throw new BadHttpRequestException("Token is expired"); 
        }
        if (oldRefreshToken.ManuallyRevoked == 1)
        {
            throw new BadHttpRequestException("Token is revoked");
        }
        
        // Gets user from DB
        var user = _userService.GetUserById(Int32.Parse(userId));
        
        // Creates new refreshtoken
        var newRefreshToken = _authService.NewRefreshToken(oldToken);

        // if no username, returns user fetch error
        if (user.Username != null)
        {
            // Creates new auth token
            var newAuthToken = _authService.GetAuthToken(user.Username);

            // Inputs the auth and refreshtoken into the response
            result.AuthToken = newAuthToken;
            result.RefreshToken = newRefreshToken;
        
        
            return result;
        }

        throw new Exception("Something went wrong when fetching user");

    }
        
    
    [Authorize]
    [HttpPost("token/logout")]
    public bool Logout()
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
        if (DateTime.Parse(oldRefreshToken.ExpiresAt!) <= DateTime.UtcNow){
            throw new BadHttpRequestException("Token is expired"); 
        }
        if (oldRefreshToken.ManuallyRevoked == 1)
        {
            throw new UnauthorizedAccessException("Token is invalid");
        }

        // Logout function in service
        var logout = _authService.LogOut(token);

        // Returns true if successfully logged out
        if (logout == "Success")
        {
            return true;
        }
        
        // If anything fails, returns false
        return false;
    }
    
    
    [Authorize]
    [HttpPost("token/logout/all")]
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
            if (DateTime.Parse(cookieRefreshToken.ExpiresAt) <= DateTime.UtcNow){
                return Task.FromResult<IActionResult>(BadRequest("Token is expired")); 
            }

            if (cookieRefreshToken.ManuallyRevoked == 1)
            {
                return Task.FromResult<IActionResult>(BadRequest("Token is invalid"));
            }


            var logout = _authService.LogOutAll(userId);

            if (logout == "Success")
            {
                return Task.FromResult<IActionResult>(Ok("Logged Out"));
            }

            if (logout == "Failed!")
            {
                return Task.FromResult<IActionResult>(BadRequest("An error occurred when logging out token"));
            }
        
        return Task.FromResult<IActionResult>(BadRequest("An unknown error has occurred"));
    }
    
    
}