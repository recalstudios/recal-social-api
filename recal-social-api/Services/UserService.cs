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

    // FIXME: This is not cryptographically secure. See: https://stackoverflow.com/questions/730268/unique-random-string-generation
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

    // Function for invalidating a reset token after one hour
    // From https://stackoverflow.com/questions/13740629/how-do-i-trigger-a-method-to-run-after-x-seconds
    private static async void InvalidateResetToken(string resetToken)
    {
        // Wait before invalidation
        // 1 hour = 3600 seconds
        await Task.Delay(TimeSpan.FromSeconds(3600));

        // Create sql command
        await using var connection = new MySqlConnection(GlobalVars.DatabaseConnectionString);
        const string commandString = "update recal_social_database.passphrase_reset_tokens set active = false where token = @token";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@token", resetToken);

        // Run the command
        try
        {
            connection.Open();
            command.ExecuteNonQuery();
            await connection.CloseAsync(); // idk why this is async, but rider told me to, so it's probably smart
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    // Gets user based on username
    public User GetUser(string username)
    {
        // Creates the response
        var user = new User();

        // Select user command
        using var connection = new MySqlConnection(GlobalVars.DatabaseConnectionString);
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
        using var connection = new MySqlConnection(GlobalVars.DatabaseConnectionString);
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
        using var connection = new MySqlConnection(GlobalVars.DatabaseConnectionString);
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
        using var connection = new MySqlConnection(GlobalVars.DatabaseConnectionString);
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
        using var connection = new MySqlConnection(GlobalVars.DatabaseConnectionString);
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
        using var connection = new MySqlConnection(GlobalVars.DatabaseConnectionString);
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
        // Declare variables
        int? userId = null;
        string? username = null;

        // Define connection
        using var connection = new MySqlConnection(GlobalVars.DatabaseConnectionString);

        // Construct SQL command
        const string commandString = "select uid, username from recal_social_database.users where email = @email";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@email", emailAddress);

        try
        {
            // Get users with specified email address
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                userId = (int) reader["uid"];
                username = (string) reader["username"];
            }

            connection.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        // If the user doesn't exist, stop the function
        if (username == null) return true;

        // If user exists, create token and send password reset email
        // Generate token
        var resetToken = RandomString(256);

        // Save the token with the user id
        const string saveTokenCommandString = "insert into recal_social_database.passphrase_reset_tokens (user_id, token) values (@userId, @resetToken)";
        var saveTokenCommand = new MySqlCommand(saveTokenCommandString, connection);
        saveTokenCommand.Parameters.AddWithValue("@userId", userId);
        saveTokenCommand.Parameters.AddWithValue("@resetToken", resetToken);

        try
        {
            // Run the command
            connection.Open();
            saveTokenCommand.ExecuteNonQuery();
            connection.Close();

            _mailService.SendMail(new MailData
            {
                EmailBody = $@"
                    <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
                    <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
                    <link href=""https://fonts.googleapis.com/css2?family=Poppins:wght@100;200;300;400;500;600;700;800;900&display=swap"" rel=""stylesheet"">

                    <style>
                        h1
                        {{
                            width: max-content;
                            color: transparent;
                            background: linear-gradient(to right, #40446e, #8892ec);

                            background-clip: text;
                            -webkit-background-clip: text;

                            font-family: 'Poppins', sans-serif;
                            font-size: 15vw;
                            font-weight: 600;
                        }}

                        .btn
                        {{
                            display: inline-block;
                            color: #d3d6f0;
                            background-color: #40446e;
                            padding: 10px 45px;
                            margin: 2rem 0;
                            border-radius: 15px;
                            text-decoration: none;
                            font-family: 'Poppins', sans-serif;
                            font-size: 24px;
                            font-weight: bold;
                        }}
                    </style>

                    <h1>Recal Social</h1>
                    <h2>Reset passphrase</h2>

                    <p>We have received your request to reset your Recal Social passphrase. To reset your passphrase,
                        click the following button:</p>

                    <a class=""btn"" href=""https://social.recalstudios.net/reset-passphrase?resetToken={resetToken}"">Reset passphrase</a>

                    <p>If the button doesn't work, paste the following URL into your browser's address bar: <a href=""https://social.recalstudios.net/reset-passphrase?resetToken={resetToken}"">https://social.recalstudios.net/reset-passphrase?resetToken={resetToken}</a></p>

                    <p>This action was triggered by submitting a passphrase reset request on our website. If you didn't
                        do this, don't worry. Your password will not be changed unless you click the link above.</p>

                    <p>Only you can see this link, and it can only be used once. It will automatically be invalidated in
                        1 hour if it hasn't been used, to protect your account.</p>

                    <p><i>Please note that replied to this email address will not be handled. If you have any questions,
                        contact <a href=""mailto:soni@recalstudios.net"">soni@recalstudios.net</a>.</i></p>

                    <br>",
                EmailSubject = "Recal Social passphrase reset",
                RecipientEmail = emailAddress,
                RecipientName = username
            });

            // Invalidate reset token after 1 hour
            InvalidateResetToken(resetToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return true;
    }

    public bool ResetUserPassphraseUsingResetToken(string resetToken, string newPassphrase)
    {
        // Declare userId
        var userId = -1;

        // Get user id
        using var connection = new MySqlConnection(GlobalVars.DatabaseConnectionString);
        const string selectUserCommandString = "select user_id from recal_social_database.passphrase_reset_tokens where token = @resetToken and active = 1";
        var selectUserCommand = new MySqlCommand(selectUserCommandString, connection);
        selectUserCommand.Parameters.AddWithValue("@resetToken", resetToken);

        try
        {
            connection.Open();
            using var reader = selectUserCommand.ExecuteReader();
            while (reader.Read())
            {
                userId = (int) reader[0];
            }
            connection.Close();

            // If no user has been found tied to the provided resetToken, or the token is inactive, stop
            if (userId == -1) return false;

            // Otherwise, continue
            // Create new commands
            const string updatePassphraseCommandString = "update recal_social_database.users set passphrase = @newPassphrase where uid = @userId";
            var updatePassphraseCommand = new MySqlCommand(updatePassphraseCommandString, connection);
            updatePassphraseCommand.Parameters.AddWithValue("@newPassphrase", Hash(newPassphrase));
            updatePassphraseCommand.Parameters.AddWithValue("@userId", userId);

            const string deactivateResetTokenCommandString = "update recal_social_database.passphrase_reset_tokens set active = false where token = @token";
            var deactivateResetTokenCommand = new MySqlCommand(deactivateResetTokenCommandString, connection);
            deactivateResetTokenCommand.Parameters.AddWithValue("@token", resetToken);

            // Run command
            try
            {
                // Update passphrase
                connection.Open();
                updatePassphraseCommand.ExecuteNonQuery();
                connection.Close();

                // Deactivate token
                connection.Open();
                deactivateResetTokenCommand.ExecuteNonQuery();
                connection.Close();

                // Passphrase has been updated
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    // Get the user chatrooms
    public IEnumerable<GetUserChatroomsResponse> GetUserChatrooms(int userId)
    {
        // Defines connection
        using var connection = new MySqlConnection(GlobalVars.DatabaseConnectionString);

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
                users.Add(new UserHasRoomResponse
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
                chatrooms.Add(new GetUserChatroomsResponse
                {
                    Id = id,
                    Name = (string) roomReader["name"],
                    Image = (string) roomReader["image"],
                    Users = users.Where(p => p.ChatroomId == id),
                });
            }
            connection.Close();
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
