using System.Security.Cryptography;
using System.Text;
using MySqlConnector;
using recal_social_api.Interfaces;
using recal_social_api.Models;
using recal_social_api.Models.Responses;
namespace recal_social_api.Services;

public class UserService : IUserService
{
    private readonly IMailService _mailService;

    private static readonly Random Random = new();

    public UserService(IMailService mailService)
    {
        _mailService = mailService;
    }

    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
    }
    private static string ByteArrayToString(byte[] arrInput)
    {
        int i;
        var sOutput = new StringBuilder(arrInput.Length);
        for (i = 0; i < arrInput.Length; i++) sOutput.Append(arrInput[i].ToString("X2"));
        return sOutput.ToString();
    }

    // Function used to hash user information
    private static string Hash(string pass)
    {
        var passBytes = Encoding.UTF8.GetBytes(pass);
        var passHash = SHA256.Create().ComputeHash(passBytes);
        return ByteArrayToString(passHash);
    }

    // Gets user based on username
    public User GetUser(string username)
    {
        // Creates the response
        var user = new User();

        // Select user command
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
        const string commandString = "select * from recal_social_database.users where username = @user";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@user", username);

        try
        {
            // Reads the user form db
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
                user.Active = (int) reader["active"];
            }
            connection.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        // Return the user
        return user;
    }


    // Gets user based on userId
    public User GetUserById(int userId)
    {
        // Creates user used in response
        var user = new User();

        // Selects the user
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
        const string commandString = "select * from recal_social_database.users where uid = @userId";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@userId", userId);

        try
        {
            // Reads the user
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
                user.Active = (int) reader["active"];
            }

            connection.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return user;
    }

    // Gets the public part of the user from the user id
    public PublicGetUserResponse PublicGetUser(int userId)
    {
        // Creates user variable used to return info
        var user = new PublicGetUserResponse();

        // Gets the user
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
        const string commandString = "select * from recal_social_database.users where uid = @userId";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@userId", userId);

        try
        {
            // Gets the user
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        // Returns the user
        return user;
    }

    // Create user with username, email and password
    public bool CreateUser(string username, string email, string pass)
    {
        // Insert the user record
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
        const string usersString = "insert into recal_social_database.users (username, passphrase, email, pfp) values (@username, @pass, @email, @pfp)";
        var userCommand = new MySqlCommand(usersString, connection);
        userCommand.Parameters.AddWithValue("@pass", Hash(pass));
        userCommand.Parameters.AddWithValue("@username", username);
        userCommand.Parameters.AddWithValue("@email", email);
        userCommand.Parameters.AddWithValue("@pfp", "https://via.placeholder.com/50");

        try
        {
            // Does the creation
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

    // Delete user with the username
    public bool DeleteUser(string username)
    {
        // Changes the user to random information. This is so that user chats still make sense, but user is anonymised
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
        const string commandString = "update recal_social_database.users set username = @username, passphrase = @pass, email = @email, pfp = 'https://via.placeholder.com/100x100', access_level = '0', active = 0 where username = @oldUsername";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@oldUsername", username);
        command.Parameters.AddWithValue("@username", "Deleted user #" + RandomString(16));
        command.Parameters.AddWithValue("@pass", Hash(RandomString(32)));
        command.Parameters.AddWithValue("@email", RandomString(32));

        try
        {
            // Updates the user
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
        // Get the user and fill in the gaps
        var user = GetUserById(payloadUserId);
        payloadUsername ??= user.Username;
        payloadEmail ??= user.Email;
        payloadPfp ??= user.Pfp;

        // Updates the user
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
        const string commandString = "update recal_social_database.users set username = @username, email = @email, pfp = @pfp where uid = @userId";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@userId", payloadUserId);
        command.Parameters.AddWithValue("@username", payloadUsername);
        command.Parameters.AddWithValue("@email", payloadEmail);
        command.Parameters.AddWithValue("@pfp", payloadPfp);

        try
        {
            // Runs the command
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

    public bool SendPassphraseResetEmail(string emailAddress)
    {
        // Define connection
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));

        // Construct SQL command
        const string commandString = "select username from recal_social_database.users where email = @email";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@email", emailAddress);

        try
        {
            // Declare variable
            string? username = null;

            // Get users with specified email address
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                username = (string) reader[0];
            }

            connection.Close();

            // If user exists, send password reset email
            if (username != null)
            {
                _mailService.SendMail(new MailData
                {
                    EmailBody = "This is a test email. You can not reply to this email.",
                    EmailSubject = "Recal Social password reset",
                    RecipientEmail = emailAddress,
                    RecipientName = username
                });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }



        return true;
    }

    // Get the user chatrooms
    public IEnumerable<GetUserChatroomsResponse> GetUserChatrooms(int userId)
    {
        // Defines connection
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));

        // Creates a list for chatrooms and for users
        var chatrooms = new List<GetUserChatroomsResponse>();
        var users = new List<UserHasRoomResponse>();

        // Selects the user rooms
        const string selectRooms = "select cid, name,image, code, pass, lastActive from recal_social_database.chatrooms, recal_social_database.users_chatrooms where chatroom_cid = chatrooms.cid and users_uid = @id  order by lastActive DESC";
        var roomCommand = new MySqlCommand(selectRooms, connection);
        roomCommand.Parameters.AddWithValue("@id", userId);

        // Gets the users from the room
        const string selectUser = "select chatroom_cid, uid, username, pfp from recal_social_database.users_chatrooms, recal_social_database.users where users_chatrooms.users_uid = uid";
        var userCommand = new MySqlCommand(selectUser, connection);
        try
        {
            // Reads the users
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

            // Reads the chatrooms
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        // Returns the chatroom
        return chatrooms;
    }
}
