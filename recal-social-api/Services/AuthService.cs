using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using MySqlConnector;
using recal_social_api.Interfaces;
using recal_social_api.Models;
namespace recal_social_api.Services;
using ConfigurationManager = System.Configuration.ConfigurationManager;


public class AuthService : IAuthService
{
    
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration config)
    {
        _configuration = config;
    }
    
    private static string ByteArrayToString(byte[] arrInput)
    {
        int i;
        var sOutput = new StringBuilder(arrInput.Length);
        for (i = 0; i < arrInput.Length; i++) sOutput.Append(arrInput[i].ToString("X2"));
        return sOutput.ToString();
    }
    
    private static string Hash(string pass)
    {
        var passBytes = Encoding.UTF8.GetBytes(pass);
        var passHash = SHA256.Create().ComputeHash(passBytes);
        return ByteArrayToString(passHash);
    }
    
    private static string GenerateRefreshToken(int userId)
    {
        var RefToken = new RefreshToken();
        
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    public User VerifyCredentials(string username, string pass)
    {
        
        var userdata = new User();
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "select * from recal_social_database.users where username = @user and passphrase = @pass";
        var command = new MySqlCommand(commandString, connection);

        command.Parameters.AddWithValue("@user", username);
        command.Parameters.AddWithValue("@pass", Hash(pass));


        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            // ID in the User table
            userdata.Id = (int) reader["uid"];
            userdata.Username = (string) reader["username"];
            userdata.Password = (string) reader["passphrase"];
            userdata.Email = (string) reader["email"];
            userdata.Pfp = (string) reader["pfp"];
        }

        connection.Close();
        return userdata;
    }

    public String GetToken(string username, string pass)
    {
        if (username != null && pass != null)
        {
            var user = VerifyCredentials(username, pass);
            
            if (user.Username != null && user.Email != null)
            {
                //create claims details based on the user information
                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("Username", user.Username),
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

                return new JwtSecurityTokenHandler().WriteToken(token).ToString();
            }
            else
            {
                return "Invalid credentials";
            }
        }
        else
        {
            return "Bad request";
        }
    }

    public bool UpdatePass(string user, string pass, string newPass)
    {
        /*using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "update recal_social_database.users set passphrase = @newPass where username = @username and passphrase = @pass";
        var command = new MySqlCommand(commandString, connection);

        var passBytes = Encoding.UTF8.GetBytes(pass);
        var passHash = SHA256.Create().ComputeHash(passBytes);
        
        var newPassBytes = Encoding.UTF8.GetBytes(newPass);
        var newPassHash = SHA256.Create().ComputeHash(newPassBytes);
        
        command.Parameters.AddWithValue("@username", user);
        command.Parameters.AddWithValue("@pass", ByteArrayToString(passHash));
        command.Parameters.AddWithValue("@newPass", ByteArrayToString(newPassHash));

        
        try
        {
            connection.Open();
            command.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }*/
        throw new NotImplementedException();
    }
}