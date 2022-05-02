using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
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

    // Gets user based on username
    public GetUserResponse GetUser(string username)
    {
        var user = new GetUserResponse();
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "select * from recal_social_database.users where username = @user";
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
    
    // Gets user based on userId
    public GetUserResponse GetUserById(int userId)
    {
        var user = new GetUserResponse();
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "select * from recal_social_database.users where uid = @userId";
        var command = new MySqlCommand(commandString, connection);



        command.Parameters.AddWithValue("@userId", userId);


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

    public PublicGetUserResponse PublicGetUser(int userId)
    {
        var user = new PublicGetUserResponse();
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "select * from recal_social_database.users where uid = @userId";
        var command = new MySqlCommand(commandString, connection);



        command.Parameters.AddWithValue("@userId", userId);


        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            // ID in the User table
            user.Id = (int) reader["uid"];
            user.Username = (string) reader["username"];
            user.Pfp = (string) reader["pfp"];
        }

        connection.Close();
        return user;
    }

    public bool CreateUser(string username, string email, string pass)
    {
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string usersString = "insert into recal_social_database.users (username, passphrase, email, pfp) values (@username, @pass, @email, @pfp)";
        
        var userCommand = new MySqlCommand(usersString, connection);
        

        
        
        userCommand.Parameters.AddWithValue("@pass", Hash(pass));
        userCommand.Parameters.AddWithValue("@username", username);
        userCommand.Parameters.AddWithValue("@email", email);
        userCommand.Parameters.AddWithValue("@pfp", "https://via.placeholder.com/50");
        
        

        try
        {
            connection.Open();
            userCommand.ExecuteNonQuery();
            connection.Close();
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
        const string commandString = "update recal_social_database.users set username = @username, passphrase = @pass, email = @email, pfp = 'https://via.placeholder.com/100x100', access_level = '0' where username = @oldUsername";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@oldUsername", username);
        command.Parameters.AddWithValue("@username", "Deleted user #" + RandomString(16));
        command.Parameters.AddWithValue("@pass", Hash(RandomString(32)));
        command.Parameters.AddWithValue("@email", RandomString(32));
        
        



        try
        {
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

    }
    

    public bool UpdateUser(int payloadUserId, string? payloadUsername, string? payloadEmail, string? payloadPfp)
    {
        
        var user = GetUserById(payloadUserId);
        payloadUsername ??= user.Username;
        payloadEmail ??= user.Email;
        payloadPfp ??= user.Pfp;

        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "update recal_social_database.users set username = @username, email = @email, pfp = @pfp where uid = @userId";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@userId", payloadUserId);
        command.Parameters.AddWithValue("@username", payloadUsername);
        command.Parameters.AddWithValue("@email", payloadEmail);
        command.Parameters.AddWithValue("@pfp", payloadPfp);
        




        try
        {
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        
        throw new NotImplementedException();

    }

    public IEnumerable<GetUserChatroomsResponse> GetUserChatrooms(int userId)
    {
        // Psudokode
        // Get the user_has_chatroom table
        // Use the chatroom ID's gotten from the table to get information regarding the 
        // chatrooms from the chatroom table
        
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);

        var chatrooms = new List<GetUserChatroomsResponse>();
        var users = new List<UserHasRoomResponse>();

        
        const string selectRooms = "select cid, name,image, code, pass, lastActive from recal_social_database.chatrooms, recal_social_database.users_chatrooms where chatroom_cid = chatrooms.cid and users_uid = @id";
        var roomCommand = new MySqlCommand(selectRooms, connection);
        roomCommand.Parameters.AddWithValue("@id", userId);

        const string selectUser = "select chatroom_cid, uid, username, pfp from recal_social_database.users_chatrooms, recal_social_database.users where users_chatrooms.users_uid = uid";
        var userCommand = new MySqlCommand(selectUser, connection);

        connection.Open();
        using var userReader = userCommand.ExecuteReader();
        while (userReader.Read())
        {
            users.Add(new UserHasRoomResponse()
            {
                Id = (int) userReader["uid"],
                Username = (string) userReader["username"],
                Pfp = (string) userReader["pfp"],
                ChatroomId = (int) userReader["chatroom_cid"]
            });
        }
        connection.Close();
        
        
        connection.Open();
        using var roomReader = roomCommand.ExecuteReader();
        while (roomReader.Read())
        {
            var id = (int) roomReader["cid"];
            chatrooms.Add(new GetUserChatroomsResponse()
            {
                Id = id,
                Name = (string) roomReader["name"],
                Image = (string) roomReader["image"],
                Users = users.Where(p => p.ChatroomId == id),
            });
        }
        

        return chatrooms;
    }
}