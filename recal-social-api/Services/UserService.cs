using System.Security.Cryptography;
using System.Text;
using MySqlConnector;
using recal_social_api.Interfaces;
using recal_social_api.Models;
namespace recal_social_api.Services;
using ConfigurationManager = System.Configuration.ConfigurationManager;

public class UserService : IUserService
{
    private static readonly Random Random = new Random();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
    private static string ByteArrayToString(byte[] arrInput)
    {
        int i;
        var sOutput = new StringBuilder(arrInput.Length);
        for (i = 0; i < arrInput.Length; i++) sOutput.Append(arrInput[i].ToString("X2"));
        return sOutput.ToString();
    }
    
    
    
    
    
    
    public User GetUser(string token)
    {
        /*var user = new User();
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "select * from online_store.user, online_store.credentials where user.uusername = credentials.username and credentials.token = @token";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@token", token);
        
        
        
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            // ID in the User table
            user.Id = (int) reader["uuid"];
            user.FirstName = (string) reader["first_name"];
            user.LastName = (string) reader["last_name"];
            user.Email = (string) reader["email"];
            user.PhoneNumber = (int) reader["phone_number"];
            user.pfp = (string) reader["pfp"];
            user.Credentials = new Credentials
            {
                Username = (string) reader["username"],
                Password = (string) reader["password"],
                Token = (string) reader["token"],
                AccessLevel = (int) reader["access_level"]
            };
        }

        connection.Close();
        return user;*/

        throw new NotImplementedException();
    }
    
    
    
   
    public bool CreateUser(string firstName, string lastName, string username, string email, int phoneNumber, string pass, string pfp)
    {
        /*
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string credentialsString = "insert into online_store.credentials (username, password, token) values (@username, @pass, @token)";
        const string usersString = "insert into online_store.user (email, phone_number, first_name, last_name,pfp, uusername) values (@email, @phoneNumber, @firstName, @lastName, @pfp, @username)";
        var credentialsCommand = new MySqlCommand(credentialsString, connection);
        var userCommand = new MySqlCommand(usersString, connection);
        
        var passBytes = Encoding.UTF8.GetBytes(pass);
        var passHash = SHA256.Create().ComputeHash(passBytes);
        
        
        credentialsCommand.Parameters.AddWithValue("@username", username);
        credentialsCommand.Parameters.AddWithValue("@pass", ByteArrayToString(passHash));
        credentialsCommand.Parameters.AddWithValue("@token", RandomString(64));
        userCommand.Parameters.AddWithValue("@username", username);
        userCommand.Parameters.AddWithValue("@email", email);
        userCommand.Parameters.AddWithValue("@phoneNumber", phoneNumber);
        userCommand.Parameters.AddWithValue("@firstName", firstName);
        userCommand.Parameters.AddWithValue("@lastName", lastName);
        userCommand.Parameters.AddWithValue("@pfp", pfp);
        
        

        try
        {
            connection.Open();
            credentialsCommand.ExecuteNonQuery();
            userCommand.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        return true;
        */
        throw new NotImplementedException();
    }

    public bool DeleteUser(string username)
    {
        /*
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "delete from online_store.user where uusername = @username; delete from online_store.credentials where username = @username";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@username", username);
        



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
        }
        */
        throw new NotImplementedException();
    }

    public bool UpdateUser(string payloadToken, string? payloadFirstName, string? payloadLastName, string? payloadEmail, int? payloadPhoneNumber, string? payloadPfp)
    {
        /*
        var user = GetUser(payloadToken);
        payloadFirstName ??= user.FirstName;
        payloadLastName ??= user.LastName;
        payloadEmail ??= user.Email;
        payloadPhoneNumber ??= user.PhoneNumber;
        payloadPfp ??= user.pfp;
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "update online_store.user set email = @email , phone_number = @phonenumber , first_name = @firstname , last_name = @lastname , pfp = @pfp where uusername = @username";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@email", payloadEmail);
        command.Parameters.AddWithValue("@phonenumber", payloadPhoneNumber);
        command.Parameters.AddWithValue("@firstname", payloadFirstName);
        command.Parameters.AddWithValue("@lastname", payloadLastName);
        command.Parameters.AddWithValue("@pfp", payloadPfp);
        command.Parameters.AddWithValue("@username", user.Credentials.Username);
        



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
        }
        */
        throw new NotImplementedException();

    }
}