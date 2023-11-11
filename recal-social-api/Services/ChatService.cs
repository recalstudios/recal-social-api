using System.Net;
using System.Text.RegularExpressions;
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

    // Creates a random string of x length
    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
    
    // Clean input function
    public static string CleanInput(string inputHTML)
    {

        var map = new Dictionary<string, string>
        {
            {"<", "&lt;"},
            {">", "&gt;"},
            {"{","&#123"},
            {"}","&#125"}
        };
        
        
        foreach (var item in map)
        {
            inputHTML = Regex.Replace(inputHTML, item.Key, item.Value);
        }

        return inputHTML;
    }
    
// Message part of service
    
    // Gets the chatroom messages with the chatroom id, userid, start and length
    public GetChatroomMessagesResponse GetChatroomMessages(int chatroomId, int userId, int start, int lenght)
    {
        // Creates a response and a list of messages for it to be populated with
        var response = new GetChatroomMessagesResponse();
        var messages = new List<Message>();
        
        // Creates end which is the start plus lenght
        int end = start + lenght;

        
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));

        
        // Selects all the messages from based on the criteria
        const string selectMessages = "select id, uid, text, timestamp from recal_social_database.messages where cid = @cid order by timestamp desc limit @start,@end";
        var messageCommand = new MySqlCommand(selectMessages, connection);
        messageCommand.Parameters.AddWithValue("@cid", chatroomId);
        messageCommand.Parameters.AddWithValue("@uid", userId);
        messageCommand.Parameters.AddWithValue("@start", start);
        messageCommand.Parameters.AddWithValue("@end", end);

        
        try
        {
            // Gets a list of messages
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        // Sets the room id into the response and the list of messages into the response
        response.RoomId = chatroomId;
        response.Messages = messages;
        
        return response;
    }

    
    // Saves a message to a chatroom
    public Message SaveChatMessage(int userId, int chatId, MessageContent content)
    {
        // Variable for message and attachments
        var message = new Message();
        var attachments = new List<MessageAttachement>();
        
        // Connectionstring
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
       
        // Insert the message
        const string insertMessage = "insert into recal_social_database.messages (uid, text, cid) values (@uid, @text, @cid)";
        var messageCommand = new MySqlCommand(insertMessage, connection);
        messageCommand.Parameters.AddWithValue("@uid", userId);
        messageCommand.Parameters.AddWithValue("@text", CleanInput(content.Text));
        messageCommand.Parameters.AddWithValue("@cid", chatId);
        
        // Insert current time into last active in the groupchat
        const string updateChatroom = "update recal_social_database.chatrooms set lastActive = @time where cid = @cid";
        var updateCommand = new MySqlCommand(updateChatroom, connection);
        updateCommand.Parameters.AddWithValue("@time", DateTime.Now);
        updateCommand.Parameters.AddWithValue("@cid", chatId);

        // Read the message
        const string readMessages = "select * from recal_social_database.messages where messages.id and uid = @uid and text = @text and cid = @cid";
        var readCommand = new MySqlCommand(readMessages, connection);
        readCommand.Parameters.AddWithValue("@uid", userId);
        readCommand.Parameters.AddWithValue("@text", CleanInput(content.Text));
        readCommand.Parameters.AddWithValue("@cid", chatId);
        
        // Read attachments
        const string readAttachments = "select * from recal_social_database.attachments where message_id = @mid";
        var readAttachmentsCommand = new MySqlCommand(readAttachments, connection);
        readAttachmentsCommand.Parameters.AddWithValue("@mid", message.Id);
        try
        {
            // Inserts the message
            connection.Open();
            messageCommand.ExecuteNonQuery();
            connection.Close();
            
            // Gets the message
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
            
            // Gets all attachments
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
                // If there are more than zero attachments, adds them to the message.content
                if (attachments.Count > 0)
                {
                    message.Content.Attachments = attachments;
                }
            }
            
            // Sets attachments to null if no attachments are found
            catch (Exception)
            {
                message.Content.Attachments = null;
            }
            connection.Close();
            
            // Update last active
            connection.Open();
            updateCommand.ExecuteNonQuery();
            connection.Close();
            
            return message;
        }
        // If anything fails, returns null
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null!;
        }
    }

    
    // Deletes a message with the message id and userid
    public bool DeleteChatMessage(int messageId, int userId)
    {
        // Sets the amount of matching messages to 0
        Int64 count = 0;
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
       
        // Deletes the messages
        const string deleteMessages = "delete from recal_social_database.messages where id = @mid and uid = @uid";
        var messageCommand = new MySqlCommand(deleteMessages, connection);
        messageCommand.Parameters.AddWithValue("@mid", messageId);
        messageCommand.Parameters.AddWithValue("@uid", userId);
        
        // Counts messages 
        const string readMessages = "select count(*) from recal_social_database.messages where id = @mid and uid = @uid";
        var readCommand = new MySqlCommand(readMessages, connection);
        readCommand.Parameters.AddWithValue("@mid", messageId);
        readCommand.Parameters.AddWithValue("@uid", userId);
        
        try
        {
            // Delete the message
            connection.Open();
            messageCommand.ExecuteNonQuery();
            connection.Close();
            
            // Count the messages matching
            connection.Open();
            using var reader = readCommand.ExecuteReader();
            while (reader.Read())
            {
                count = (Int64) reader[0];
            }
            connection.Close();

            // If the count is zero, it worked
            return count == 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
// Room part of service

    // Create chatroom with a name, password and userid
    public bool CreateChatroom(string name, string pass, int userId)
    {
        // Generate room code
        var code = RandomString(8);
        var cid = new int();
        
        // Creates the chatroom
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
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
            // Insert command
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            
            // Get the chatroom id
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
            // Insert user into group
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

    // Gets the details about the chatroom
    public Chatroom DetailsChatroom(int userId, int chatroomId)
    {
        // Creates the result
        var result = new Chatroom();
        
        
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
        
        // Get the chatroom id
        const string selectCommandString = "select cid, name, image, code, pass, lastActive from recal_social_database.chatrooms, recal_social_database.users_chatrooms where chatrooms.cid = users_chatrooms.chatroom_cid and cid = @cid and users_uid = @uid";
        var selectCommand = new MySqlCommand(selectCommandString, connection);
        selectCommand.Parameters.AddWithValue("@uid", userId);
        selectCommand.Parameters.AddWithValue("@cid", chatroomId);

        
        
        try
        {
            // Runs creation and fetching
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

    // Updates the chatroom name, image or password
    public bool UpdateChatroom(int userId, int chatroomId, string? payloadName, string? payloadImage, string? payloadPass)
    {
        // Gets the chatroom information
        var chatroom = DetailsChatroom(userId, chatroomId);

        // If the name is null or empty, break
        if (string.IsNullOrEmpty(chatroom.Name))
        {
            return false;
        }
        
        // Sets the variables to chatroom name if not otherwise defined
        payloadName ??= chatroom.Name;
        payloadImage ??= chatroom.Image;
        payloadPass ??= chatroom.Code;

        
        // Runs the update command
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
        const string commandString = "update recal_social_database.chatrooms set name = @name, image = @image, pass = @pass where cid = @cid";
        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@cid", chatroomId);
        command.Parameters.AddWithValue("@name", payloadName);
        command.Parameters.AddWithValue("@image", payloadImage);
        command.Parameters.AddWithValue("@pass", payloadPass);
        

        try
        {
            // Tries to run
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

    // Deletes the chatroom with user id and chatroom id
    public bool DeleteChatroom(int userId, int chatroomId)
    {
        // Store if the user is a part of the chatroom
        var status = new Int64();
        
        // Creates the connection
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));

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

        // If no entry with chatroom and userid combo, returns false
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
            // Removes messages
            connection.Open();
            removeMessagesCommand.ExecuteNonQuery();
            connection.Close();
            
            // Removes users
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

    // Lets user join chatroom with code, pass and user id
    public bool JoinChatroom(string code, string pass, int userId)
    {
        // Creates variable where chatroom id is stored
        var cid = new int?();
        
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));
        
        
        // Get the chatroom id
        const string selectCommandString = "select cid from recal_social_database.chatrooms where code = @code and pass = @pass";
        var selectCommand = new MySqlCommand(selectCommandString, connection);
        selectCommand.Parameters.AddWithValue("@code", code);
        selectCommand.Parameters.AddWithValue("@pass", pass);
        
        // Runs fetching
        try
        {
            // Gets the chatroom id
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

        // Checks if chatroom id is null or empty
        if (string.IsNullOrEmpty(cid.ToString()))
        {
            return false;
        }
        
        // Adds to chatroom
        const string joinCommandString = "insert into recal_social_database.users_chatrooms (users_uid, chatroom_cid) values (@uid, @cid)";
        var joinCommand = new MySqlCommand(joinCommandString, connection);
        joinCommand.Parameters.AddWithValue("@uid", userId);
        joinCommand.Parameters.AddWithValue("@cid", cid);

        
        try
        {
            // Adds user to chatroom with previously obtained data
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

    // Lets user leave chatroom with user id and chatroom id
    public bool LeaveChatroom(int userId, int chatroomId)
    {
        // Creates variable for storing if the user is in the chatroom and selecting remaining users
        var isUserInChatroom = new Int64();
        var usersLeft = new Int64();
        
        // Creates the connection
        using var connection = new MySqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));

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
                isUserInChatroom = (long) reader[0];
            }
            connection.Close();
  
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        // If no user with chatroom and userid combo, returns false
        if (isUserInChatroom == 0)
        {
            return false;
        }
        
        // Count users remaining
        const string selectUserCountCommandString = "select count(*) from recal_social_database.users_chatrooms where chatroom_cid = @cid";
        var selectUserCountCommand = new MySqlCommand(selectUserCountCommandString, connection);
        selectUserCountCommand.Parameters.AddWithValue("@cid", chatroomId);

        try
        {
            // Runs counting of users left
            connection.Open();
            using var reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                usersLeft = (long) reader[0];
            }
            connection.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        // If there are no other users left in the chatroom, deletes the chatroom
        /*if (usersLeft <= 1)
        {
            return DeleteChatroom(userId, chatroomId);
        }*/
        
        // Delete the user from the chatroom
        const string removeCommandString = "delete from recal_social_database.users_chatrooms where users_uid = @uid and chatroom_cid = @cid";
        var removeCommand = new MySqlCommand(removeCommandString, connection);
        removeCommand.Parameters.AddWithValue("@uid", userId);
        removeCommand.Parameters.AddWithValue("@cid", chatroomId);
        
        
        try
        {
            // Runs deletion of entry
            connection.Open();
            removeCommand.ExecuteNonQuery();
            connection.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        
        return true;
    }
}