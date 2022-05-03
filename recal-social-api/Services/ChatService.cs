using MySqlConnector;
using recal_social_api.Interfaces;
using recal_social_api.Models;
using recal_social_api.Models.Responses;

using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace recal_social_api.Services;

public class ChatService : IChatService
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;

    public ChatService(IConfiguration config, IUserService userService)
    {
        _configuration = config;
        _userService = userService;
    }
    
    private static readonly Random Random = new Random();

    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
    
// Message part of service
    public GetChatroomMessagesResponse GetChatroomMessages(int chatroomId, int userId, int start, int lenght)
    {
        var response = new GetChatroomMessagesResponse();
        var messages = new List<Message>();
        int end = start + lenght;
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);

        
        
        const string selectMessages = "select id, uid, data, timestamp from recal_social_database.messages where uid = @uid and cid = @cid order by timestamp desc limit @start,@end";
        var messageCommand = new MySqlCommand(selectMessages, connection);
        messageCommand.Parameters.AddWithValue("@cid", chatroomId);
        messageCommand.Parameters.AddWithValue("@uid", userId);
        messageCommand.Parameters.AddWithValue("@start", start);
        messageCommand.Parameters.AddWithValue("@end", end);
        
        
        connection.Open();
        using var messageReader = messageCommand.ExecuteReader();
        while (messageReader.Read())
        {
            messages.Add(new Message()
            {
                MessageId = (int) messageReader["id"],
                Data = (string) messageReader["data"],
                AuthorId = (int) messageReader["uid"],
                Time = (DateTime) messageReader["timestamp"]
            });
        }
        connection.Close();

        response.RoomId = chatroomId;
        response.Messages = messages;

        return response;
    }

    public int SaveChatMessage(int userId, string data, int chatId)
    {
        var id = 0;
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
       
        
        const string selectMessages = "insert into recal_social_database.messages (uid, data, cid) values (@uid, @data, @cid)";
        var messageCommand = new MySqlCommand(selectMessages, connection);
        messageCommand.Parameters.AddWithValue("@uid", userId);
        messageCommand.Parameters.AddWithValue("@data", data);
        messageCommand.Parameters.AddWithValue("@cid", chatId);
        
        const string readMessages = "select id from recal_social_database.messages where uid = @uid and data = @data and cid = @cid";
        var readCommand = new MySqlCommand(readMessages, connection);
        readCommand.Parameters.AddWithValue("@uid", userId);
        readCommand.Parameters.AddWithValue("@data", data);
        readCommand.Parameters.AddWithValue("@cid", chatId);
        
        try
        {
            connection.Open();
            messageCommand.ExecuteNonQuery();
            using var reader = readCommand.ExecuteReader();
            while (reader.Read())
            {
                id = (int) reader["id"];
            }
            connection.Close();
            
            return id;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public bool DeleteChatMessage(int messageId, int userId)
    {
        Int64 count = 0;
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
       
        
        const string selectMessages = "delete from recal_social_database.messages where id = @mid and uid = @uid";
        var messageCommand = new MySqlCommand(selectMessages, connection);
        messageCommand.Parameters.AddWithValue("@mid", messageId);
        messageCommand.Parameters.AddWithValue("@uid", userId);
        
        
        
        const string readMessages = "select count(*) from recal_social_database.messages where id = @mid and uid = @uid";
        var readCommand = new MySqlCommand(readMessages, connection);
        readCommand.Parameters.AddWithValue("@mid", messageId);
        readCommand.Parameters.AddWithValue("@uid", userId);
        try
        {
            connection.Open();
            messageCommand.ExecuteNonQuery();
            using var reader = readCommand.ExecuteReader();
            while (reader.Read())
            {
                count = (Int64) reader[0];
            }
            connection.Close();

            if (count == 0)
            {
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
// Room part of service
    public bool CreateChatroom(string name, string pass, int userId)
    {
        // Generate room code
        var code = RandomString(8);
        var cid = new int();
        
        // Creates the chatroom
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "insert into recal_social_database.chatrooms (name, code, pass, lastActive) values (@name, @code, @pass, @lastActive)";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@code", code);
        command.Parameters.AddWithValue("@pass", pass);
        command.Parameters.AddWithValue("@LastActive", DateTime.Now);
        
        // Get the chatroom id
        const string selectCommandString = "select cid from recal_social_database.chatrooms where name = @name and code = @code and pass = @pass";
        var selectCommand = new MySqlCommand(selectCommandString, connection);
        selectCommand.Parameters.AddWithValue("@name", name);
        selectCommand.Parameters.AddWithValue("@code", code);
        selectCommand.Parameters.AddWithValue("@pass", pass);
        
        // Runs creation and fetching
        try
        {
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            connection.Open();
            using var reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                cid = (int) reader["cid"];
            }
            connection.Close();
  
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        
        
        // Adds creator of chatroom
        const string joinCommandString = "insert into recal_social_database.users_chatrooms (users_uid, chatroom_cid) values (@uid, @cid)";
        var joinCommand = new MySqlCommand(joinCommandString, connection);
        joinCommand.Parameters.AddWithValue("@uid", userId);
        joinCommand.Parameters.AddWithValue("@cid", cid);

        // Adds user to chatroom with previously obtained data
        try
        {
            connection.Open();
            joinCommand.ExecuteNonQuery();
            connection.Close();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool DeleteChatroom(int userId, int chatroomId)
    {
        var status = new Int64();
        
        // Creates the connection
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);

        // Get if the user is in chatroom
        const string selectCommandString = "select count(*) from recal_social_database.users_chatrooms where users_uid = @uid and chatroom_cid = @cid";
        var selectCommand = new MySqlCommand(selectCommandString, connection);
        selectCommand.Parameters.AddWithValue("@uid", userId);
        selectCommand.Parameters.AddWithValue("@cid", chatroomId);

        // Runs selection of user
        try
        {
            connection.Open();
            using var reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                status = (Int64) reader[0];
            }
            connection.Close();
  
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        // If no user with chatroom and userid combo, returns false
        if (status == 0)
        {
            return false;
        }
        
        // Remove messages from chatroom
        const string removeMessagesString = " delete from recal_social_database.messages where cid = @cid";
        var removeMessagesCommand = new MySqlCommand(removeMessagesString, connection);
        removeMessagesCommand.Parameters.AddWithValue("@cid", chatroomId);
        
        // Remove users from chatroom
        const string removeCommandString = "delete from recal_social_database.users_chatrooms where users_uid = @uid and chatroom_cid = @cid";
        var removeUserCommand = new MySqlCommand(removeCommandString, connection);
        removeUserCommand.Parameters.AddWithValue("@uid", userId);
        removeUserCommand.Parameters.AddWithValue("@cid", chatroomId);
        
        // Runs deletion of entry
        try
        {
            connection.Open();
            removeMessagesCommand.ExecuteNonQuery();
            connection.Close();
            connection.Open();
            removeUserCommand.ExecuteNonQuery();
            connection.Close();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool JoinChatroom(string code, string pass, int userId)
    {
        var cid = new int?();
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        
        
        // Get the chatroom id
        const string selectCommandString = "select cid from recal_social_database.chatrooms where code = @code and pass = @pass";
        var selectCommand = new MySqlCommand(selectCommandString, connection);
        selectCommand.Parameters.AddWithValue("@code", code);
        selectCommand.Parameters.AddWithValue("@pass", pass);
        
        // Runs fetching
        try
        {
            connection.Open();
            using var reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                cid = (int) reader["cid"];
            }
            connection.Close();
  
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        if (string.IsNullOrEmpty(cid.ToString()))
        {
            return false;
        }
        
        // Adds to chatroom
        const string joinCommandString = "insert into recal_social_database.users_chatrooms (users_uid, chatroom_cid) values (@uid, @cid)";
        var joinCommand = new MySqlCommand(joinCommandString, connection);
        joinCommand.Parameters.AddWithValue("@uid", userId);
        joinCommand.Parameters.AddWithValue("@cid", cid);

        // Adds user to chatroom with previously obtained data
        try
        {
            connection.Open();
            joinCommand.ExecuteNonQuery();
            connection.Close();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool LeaveChatroom(int userId, int chatroomId)
    {
        var status = new Int64();
        var users = new Int64();
        
        // Creates the connection
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);

        // Get if the user is in chatroom
        const string selectCommandString = "select count(*) from recal_social_database.users_chatrooms where users_uid = @uid and chatroom_cid = @cid";
        var selectCommand = new MySqlCommand(selectCommandString, connection);
        selectCommand.Parameters.AddWithValue("@uid", userId);
        selectCommand.Parameters.AddWithValue("@cid", chatroomId);

        // Runs selection of user
        try
        {
            connection.Open();
            using var reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                status = (Int64) reader[0];
            }
            connection.Close();
  
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        // If no user with chatroom and userid combo, returns false
        if (status == 0)
        {
            return false;
        }
        
        // Count users remaining
        const string selectUserCountCommandString = "select count(*) from recal_social_database.users_chatrooms where chatroom_cid = @cid";
        var selectUserCountCommand = new MySqlCommand(selectUserCountCommandString, connection);
        selectUserCountCommand.Parameters.AddWithValue("@uid", userId);
        selectUserCountCommand.Parameters.AddWithValue("@cid", chatroomId);

        // Runs selection of user
        try
        {
            connection.Open();
            using var reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                users = (Int64) reader[0];
            }
            connection.Close();
  
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        if (users <= 1)
        {
            return DeleteChatroom(userId, chatroomId);
        }

        // Get if the user is in chatroom
        const string removeCommandString = "delete from recal_social_database.users_chatrooms where users_uid = @uid and chatroom_cid = @cid";
        var removeCommand = new MySqlCommand(removeCommandString, connection);
        removeCommand.Parameters.AddWithValue("@uid", userId);
        removeCommand.Parameters.AddWithValue("@cid", chatroomId);
        
        // Runs deletion of entry
        try
        {
            connection.Open();
            removeCommand.ExecuteNonQuery();
            connection.Close();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}