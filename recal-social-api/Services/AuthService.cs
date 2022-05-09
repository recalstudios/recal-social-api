using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using MySqlConnector;
using recal_social_api;
using recal_social_api.Interfaces;
using recal_social_api.Models;
using recal_social_api.Models.Responses;

namespace recal_social_api.Services;
using ConfigurationManager = System.Configuration.ConfigurationManager;


public class AuthService : IAuthService
{

    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;

    public AuthService(IConfiguration config, IUserService userService)
    {
        _configuration = config;
        _userService = userService;
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

    //  Generate fresh refreshtoken that gets sent to database
    public string GenerateRefreshToken(int userId)
    {

        //Initialise the object that is going to be made and sent
            var refToken = new RefreshToken();

        //Create the randomness of the token
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
        
        // Information needed for a fresh RefreshToken
            refToken.Token = Convert.ToBase64String(randomNumber);
            refToken.Created = DateTime.UtcNow;
            refToken.ExpiresAt = DateTime.Now.AddDays(GlobalVars.RefreshTokenAgeDays);
            refToken.UserId = userId;


        // Insert into the DB
            using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
            const string commandString = "insert into recal_social_database.refreshtoken (token, created, expiresAt, userId) value (@token, @created, @expiresat, @userid)";
            var command = new MySqlCommand(commandString, connection);
            command.Parameters.AddWithValue("@token", refToken.Token);
            command.Parameters.AddWithValue("@created", refToken.Created);
            command.Parameters.AddWithValue("@expiresat", refToken.ExpiresAt);
            command.Parameters.AddWithValue("@userid", refToken.UserId);


            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            
        //create claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "JWTRefreshToken"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, refToken.Created.ToString(CultureInfo.CurrentCulture)),
                new Claim("UserId", userId.ToString()),
                new Claim("Token", refToken.Token),
            };

        // Create the token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: refToken.ExpiresAt,
                signingCredentials: signIn);

        //Return the token
        // ReSharper disable once InconsistentNaming
        var JwtTokenRefresh = new JwtSecurityTokenHandler().WriteToken(token).ToString();

        return JwtTokenRefresh;
    }

    //  Get refresh token from DB
    public GetRefreshTokenResponse GetRefreshToken(string token)
    {
        var refreshToken = new GetRefreshTokenResponse();

        // Get from the DB
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "select * from recal_social_database.refreshtoken where token = @token";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@token", token);

        // Reads and puts into var
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            // ID in the User table
            refreshToken.RefreshTokenId = (int) reader["refreshTokenId"];
            refreshToken.Token = (string) reader["token"];
            refreshToken.Created = (DateTime) reader["created"];
            refreshToken.RevokationDate = reader["revokationDate"].ToString();
            refreshToken.ManuallyRevoked = (int) (reader["manuallyRevoked"].ToString()!.Length == 0 ? 0 : reader["manuallyRevoked"]);
            refreshToken.ExpiresAt = reader["expiresAt"].ToString();
            refreshToken.UserId = (int) reader["userId"];
        }


        connection.Close();
        
        // Return
        return refreshToken;
    }
    
    // Update the refreshtoken in the DB
    public bool UpdateRefreshTokenChain(int tokenId, int oldTokenId)
    {
        var errors = 0;
        
        // Update old token
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "update recal_social_database.refreshtoken set replacedById = @tokenId, revokationDate = @revdate, manuallyRevoked = 1 where refreshTokenId = @oldTokenId";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@tokenId", tokenId);
        command.Parameters.AddWithValue("@oldTokenId", oldTokenId);
        command.Parameters.AddWithValue("@revdate", DateTime.UtcNow);
        
        // Update new token
        const string commandString2 = "update recal_social_database.refreshtoken set replacesId = @oldTokenId where refreshTokenId = @tokenId";
        var command2 = new MySqlCommand(commandString2, connection);
        command2.Parameters.AddWithValue("@tokenId", tokenId);
        command2.Parameters.AddWithValue("@oldTokenId", oldTokenId);
        
        try
        {
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            connection.Open();
            command2.ExecuteNonQuery();
            connection.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            errors++;
        }

        if(errors == 0)
        {
            return true;
        }

        return false;
    }
    
    // Creates another refreshtoken in the chain
    public string NewRefreshToken(string oldToken)
    {
        var oldRefreshToken = GetRefreshToken(oldToken);
        
        //Initialise the object that is going to be made and sent
            var refToken = new RefreshToken();

        //Create the randomness of the token
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
        
        // Information needed for a fresh RefreshToken
            refToken.Token = Convert.ToBase64String(randomNumber);
            refToken.Created = DateTime.UtcNow;
            refToken.ExpiresAt = DateTime.Now.AddDays(GlobalVars.RefreshTokenAgeDays);
            refToken.UserId = oldRefreshToken.UserId;
            
            


        // Insert into the DB
            using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
            const string commandString = "insert into recal_social_database.refreshtoken (token, created, expiresAt, userId) value (@token, @created, @expiresat, @userid)";
            var command = new MySqlCommand(commandString, connection);
            command.Parameters.AddWithValue("@token", refToken.Token);
            command.Parameters.AddWithValue("@created", refToken.Created);
            command.Parameters.AddWithValue("@expiresat", refToken.ExpiresAt);
            command.Parameters.AddWithValue("@userid", refToken.UserId);


            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var newRefreshToken = GetRefreshToken(refToken.Token);

            UpdateRefreshTokenChain(newRefreshToken.RefreshTokenId, oldRefreshToken.RefreshTokenId);
            
        //create claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "JWTRefreshToken"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, refToken.Created.ToString(CultureInfo.CurrentCulture)),
                new Claim("UserId", newRefreshToken.UserId.ToString()),
                new Claim("Token", refToken.Token),
            };

        // Create the token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: refToken.ExpiresAt,
                signingCredentials: signIn);

        //Return the token
        // ReSharper disable once InconsistentNaming
        var JwtTokenRefresh = new JwtSecurityTokenHandler().WriteToken(token);

        return JwtTokenRefresh;
    }

    // Verify if user exists in DB with username and pass
    public User GetUserInfo(string username, string pass)
    {
        
        var userdata = new User();
        
        // Connect to DB and get user
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
            userdata.Active = (int) reader["active"];
        }

        connection.Close();
        return userdata;
    }

    public bool UpdateCredentials(int userId, string pass, string newPass)
    {
        Int64 count = 0;
        
        // Updates password in DB
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "update recal_social_database.users set passphrase = @newPass where uid = @userid and passphrase = @pass";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@userid", userId);
        command.Parameters.AddWithValue("@pass", Hash(pass));
        command.Parameters.AddWithValue("@newPass", Hash(newPass));

        // Gets amount of users matching userid and new password
        const string selectCommandString = "select count(*) from recal_social_database.users where uid = @uid and passphrase = @newPass";
        var selectCommand = new MySqlCommand(selectCommandString, connection);
        selectCommand.Parameters.AddWithValue("@uid", userId);
        selectCommand.Parameters.AddWithValue("@newPass", Hash(newPass));
        
        // Tries to update password and count users with new password
        try
        {
            // Update password in db
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            
            // Gets count
            connection.Open();
            using var reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                count = (Int64) reader[0];
                Console.WriteLine(count);
            }
            connection.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        
        // If count of users matching the new password and uid is not zero, it returns true
        if (count != 0)
        {
            return true;
        }
        return false;
    }

    public string GetNewAuthToken(string username, string pass)
    {
        //  If username isn't empty, starts creating JWT token
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (username != null)
        {
            // Gets the user from the DB
            var user = GetUserInfo(username, pass);
            
                //  If returning Username successfully, create JWT. Else fails
            if (user.Username != null && user.Active == 1)
            {
                //create claims details based on the user information
                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.CurrentCulture)),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("Username", user.Username!),
                };

                // Creates the JWT token and includes the claims
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(7200),
                    signingCredentials: signIn);

                return new JwtSecurityTokenHandler().WriteToken(token).ToString();
            }
            else
            {
                return "Unauthorized";
            }
        }
        else
        {
            return "Bad request";
        }
    }
    public string GetAuthToken(string username)
    {
        //  If username isn't empty, starts creating JWT token
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (username != null)
        {
            var user = _userService.GetUser(username);
            
            //  If returning Username successfully, create JWT. Else fails
            if (user.Username != null)
            {
                //create claims details based on the user information
                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.CurrentCulture)),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("Username", user.Username),
                };

                // Creates the JWT token and includes the claims
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(GlobalVars.AuthTokenAgeMinutes),
                    signingCredentials: signIn);

                return new JwtSecurityTokenHandler().WriteToken(token);
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

    public string LogOut(string token)
    {
        // Insert into the DB
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "update recal_social_database.refreshtoken set revokationDate = @revdate, manuallyRevoked = 1 where token = @token";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@token", token);
        command.Parameters.AddWithValue("@revdate", DateTime.UtcNow);


        try
        {
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            return "Success";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "Failed!";
        }
    }

    public string LogOutAll(string userId)
    {
        // Insert into the DB
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "update recal_social_database.refreshtoken set revokationDate = @revdate, manuallyRevoked = 1 where userId = @userid and expiresAt > @expiresat";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@userid", userId);
        command.Parameters.AddWithValue("@revdate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@expiresat", DateTime.UtcNow.AddDays(GlobalVars.RefreshTokenAgeDays));


        try
        {
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            return "Success";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "Failed!";
        }
    }
}