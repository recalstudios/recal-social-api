using System.Security.Cryptography;
using System.Text;
using MySqlConnector;
using recal_social_api.Interfaces;
using recal_social_api.Models;
using recal_social_api.Models.Responses;
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

    private static string Hash(string pass)
    {
        var passBytes = Encoding.UTF8.GetBytes(pass);
        var passHash = SHA256.Create().ComputeHash(passBytes);
        return ByteArrayToString(passHash);
    }
    
    
    
    
    
    
    public GetUserResponse GetUser(string username)
    {
        var user = new GetUserResponse();
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "select * from recal_socials_database.users where username = @user";
        var command = new MySqlCommand(commandString, connection);



        command.Parameters.AddWithValue("@user", username);


        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            // ID in the User table
            user.Id = (int) reader["uid"];
            user.Username = (string) reader["username"];
            user.Password = (string) reader["passphrase"];
            user.Email = (string) reader["email"];
            user.Pfp = (string) reader["pfp"];
        }

        connection.Close();
        return user;
    }




    public bool CreateUser(string username, string email, string pass, string pfp)
    {
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string usersString = "insert into recal_socials_database.users (username, passphrase, email, pfp) values (@username, @pass, @email, @pfp)";
        
        var userCommand = new MySqlCommand(usersString, connection);
        

        
        
        userCommand.Parameters.AddWithValue("@pass", Hash(pass));
        userCommand.Parameters.AddWithValue("@username", username);
        userCommand.Parameters.AddWithValue("@email", email);
        userCommand.Parameters.AddWithValue("@pfp", pfp);
        
        

        try
        {
            connection.Open();
            userCommand.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

    }

    public bool DeleteUser(string username)
    {
        // Changes the user to random information. This is so that user chats still make sense, but user is anonymised
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "update recal_socials_database.users set username = @username, passphrase = @pass, email = @email, pfp = 'https://via.placeholder.com/100x100', access_level = '0' where username = @oldUsername";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@oldUsername", username);
        command.Parameters.AddWithValue("@username", "Deleted user #" + RandomString(16));
        command.Parameters.AddWithValue("@pass", Hash(RandomString(32)));
        command.Parameters.AddWithValue("@email", RandomString(32));
        
        



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