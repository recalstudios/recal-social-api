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
    public Task<IActionResult> Post([FromBody] VerifyUserRequest payload)
    {
        var response = new GetJwtTokenResponse();
        
        var result = _authService.GetNewAuthToken(payload.Username, payload.Password);
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
            
        

        var cookieOptions = new CookieOptions()
        {
            Expires = DateTime.Now.AddDays(GlobalVars.RefreshTokenAgeDays),
            SameSite = SameSiteMode.None,
            Path = "/",
            HttpOnly = true,
            Secure = true
        };

        HttpContext.Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);


        return Task.FromResult<IActionResult>(Ok(response.AuthToken));

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
    public Task<IActionResult> ChainToken()
    {
        Request.Cookies.TryGetValue("refreshToken", out string? refreshToken);

        if (refreshToken == null)
        {
            return Task.FromResult<IActionResult>(BadRequest("Token cannot be null"));
        }

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(refreshToken);
        var tokenS = jsonToken as JwtSecurityToken;
        
        //  Sets the variable username to the username from the token
        var oldToken = tokenS!.Claims.First(claim => claim.Type == "Token").Value;
        var userId = tokenS!.Claims.First(claim => claim.Type == "UserId").Value;

        var oldRefreshToken = _authService.GetRefreshToken(oldToken);
        
        // If its expired or revoked, doesnt work
        if (DateTime.Parse(oldRefreshToken.ExpiresAt) <= DateTime.UtcNow){
            return Task.FromResult<IActionResult>(BadRequest("Token is expired")); 
        }

        if (oldRefreshToken.ManuallyRevoked == 1)
        {
            return Task.FromResult<IActionResult>(BadRequest("Token is invalid"));
        }
        
        var user = _userService.GetUserById(Int32.Parse(userId));
        
        var newRefreshToken = _authService.NewRefreshToken(oldToken);

        if (user.Username != null)
        {
            var newAuthToken = _authService.GetAuthToken(user.Username);
        
            var cookieOptions = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(GlobalVars.RefreshTokenAgeDays),
                HttpOnly = true,
                Secure = true
            };

            HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);
        
        
            return Task.FromResult<IActionResult>(Ok(newAuthToken));
        }

        throw new Exception("Something went wrong when fetching user");

    }
        
    
    [Authorize]
    [HttpPost("token/logout")]
    public Task<IActionResult> Logout()
    {
        Request.Cookies.TryGetValue("refreshToken", out var refreshToken);

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(refreshToken);
        var tokenS = jsonToken as JwtSecurityToken;
        
        //  
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


        var logout = _authService.LogOut(token);

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
    
    
    [Authorize]
    [HttpPost("token/logout/all")]
    public Task<IActionResult> LogoutAll()
    {
        Request.Cookies.TryGetValue("refreshToken", out var refreshToken);

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(refreshToken);
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