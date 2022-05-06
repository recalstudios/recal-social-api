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

        
        
        const string selectMessages = "select id, uid, text, timestamp from recal_social_database.messages where uid = @uid and cid = @cid order by timestamp limit @start,@end";
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
                Id = (int) messageReader["id"],
                Type = "message",
                Room = chatroomId,
                Author = (int) messageReader["uid"],
                Content = new MessageContent()
                {
                    Text = (string) messageReader["text"]
                },
                Timestamp = (DateTime) messageReader["timestamp"]
            });
        }
        connection.Close();

        response.RoomId = chatroomId;
        response.Messages = messages;

        return response;
    }

    
    public Message SaveChatMessage(int userId, int chatId, MessageContent content)
    {
        var message = new Message();
        var attachments = new List<MessageAttachement>();
        
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
       
        // Insert the message
        const string insertMessage = "insert into recal_social_database.messages (uid, text, cid) values (@uid, @text, @cid)";
        var messageCommand = new MySqlCommand(insertMessage, connection);
        messageCommand.Parameters.AddWithValue("@uid", userId);
        messageCommand.Parameters.AddWithValue("@text", content.Text);
        messageCommand.Parameters.AddWithValue("@cid", chatId);
        
        // Read the message
        const string readMessages = "select * from recal_social_database.messages where messages.id and uid = @uid and text = @text and cid = @cid";
        var readCommand = new MySqlCommand(readMessages, connection);
        readCommand.Parameters.AddWithValue("@uid", userId);
        readCommand.Parameters.AddWithValue("@text", content.Text);
        readCommand.Parameters.AddWithValue("@cid", chatId);
        
        // Read attachments
        const string readAttachments = "select * from recal_social_database.attachments where message_id = @mid";
        var readAttachmentsCommand = new MySqlCommand(readAttachments, connection);
        readAttachmentsCommand.Parameters.AddWithValue("@mid", message.Id);
        try
        {
            connection.Open();
            messageCommand.ExecuteNonQuery();
            connection.Close();
            connection.Open();
            using var reader = readCommand.ExecuteReader();
            while (reader.Read())
            {
                message.Id = (int) reader["id"];
                message.Type = "message";
                message.Room = (int) reader["cid"];
                message.Author = (int) reader["uid"];
                message.Content = new MessageContent()
                {
                    Text = (string) reader["text"]
                };
                message.Timestamp = (DateTime) reader["timestamp"];

            }
            connection.Close();
            
            
            connection.Open();
            using var aReader = readCommand.ExecuteReader();
            try
            {
                while (aReader.Read())
                {
                    attachments.Add(new MessageAttachement
                    {
                        Id = (int) aReader["attachment_id"],
                        MessageId = (int) reader["message_id"],
                        Type = (string) aReader["type"],
                        Src = (string) aReader["src"]
                    });

                }
                if (attachments.Count > 0)
                {
                    message.Content.Attachments = attachments;
                }
            }
            catch (Exception e)
            {
                message.Content.Attachments = null;
            }
            connection.Close();
            
            return message;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public bool DeleteChatMessage(int messageId, int userId)
    {
        Int64 count = 0;
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
       
        
        const string deleteMessages = "delete from recal_social_database.messages where id = @mid and uid = @uid";
        var messageCommand = new MySqlCommand(deleteMessages, connection);
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

    public Chatroom DetailsChatroom(int userId, int chatroomId)
    {
        var result = new Chatroom();
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        
        // Get the chatroom id
        const string selectCommandString = "select cid, name, image, code, pass, lastActive from recal_social_database.chatrooms, recal_social_database.users_chatrooms where chatrooms.cid = users_chatrooms.chatroom_cid and cid = @cid and users_uid = @uid";
        var selectCommand = new MySqlCommand(selectCommandString, connection);
        selectCommand.Parameters.AddWithValue("@uid", userId);
        selectCommand.Parameters.AddWithValue("@cid", chatroomId);

        
        // Runs creation and fetching
        try
        {
            connection.Open();
            using var reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                result.ChatroomId = (int) reader["cid"];
                result.Name = (string) reader["name"];
                result.Image = (string) reader["image"];
                result.Code = (string) reader["code"];
                result.Pass = (string) reader["pass"];
                result.LastActive = (DateTime) reader["lastActive"];
            }
            connection.Close();
  
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Something went wrong");
        }

        return result;
    }

    public bool UpdateChatroom(int userId, int chatroomId, string? payloadName, string? payloadImage, string? payloadPass)
    {
        var chatroom = DetailsChatroom(userId, chatroomId);

        if (string.IsNullOrEmpty(chatroom.Name))
        {
            return false;
        }
        
        payloadName ??= chatroom.Name;
        payloadImage ??= chatroom.Image;
        payloadPass ??= chatroom.Code;

        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        const string commandString = "update recal_social_database.chatrooms set name = @name, image = @image, pass = @pass where cid = @cid";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@cid", chatroomId);
        command.Parameters.AddWithValue("@name", payloadName);
        command.Parameters.AddWithValue("@image", payloadImage);
        command.Parameters.AddWithValue("@pass", payloadPass);
        

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