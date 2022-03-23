using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using recal_social_api.Interfaces;
using recal_social_api.Models;
using recal_social_api.Models.Requests;
using recal_social_api.Services;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace recal_social_api.Controllers;


[Route("api/token")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly IUserService _userService;
    
    private readonly IConfiguration _configuration;

    public TokenController(IConfiguration config, IUserService userService)
    {
        _configuration = config;
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost]
    public Task<IActionResult> Post([FromBody] GetUserRequest payload)
    {
        if (payload.Username != null && payload.Password != null)
        {
            var user = _userService.GetUser(payload.Username, payload.Password);
            
            if (user.Username != null && user.Email != null)
            {
                //create claims details based on the user information
                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("DisplayName", user.Username),
                    new Claim("Email", user.Email)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(10),
                    signingCredentials: signIn);

                return Task.FromResult<IActionResult>(Ok(new JwtSecurityTokenHandler().WriteToken(token)));
            }
            else
            {
                return Task.FromResult<IActionResult>(BadRequest("Invalid credentials"));
            }
        }
        else
        {
            return Task.FromResult<IActionResult>(BadRequest());
        }
    }
    
}
