using System.Globalization;
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

    public GetChatroomMessagesResponse GetChatroomMessages(int ChatroomId, int UserId, int Start, int Lenght)
    {
        var response = new GetChatroomMessagesResponse();
        var messages = new List<Message>();
        int End = Start + Lenght;
        
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);

        
        
        const string selectMessages = "select id, uid, data, timestamp from recal_social_database.messages where uid = @uid and cid = @cid order by timestamp desc limit @start,@end";
        var messageCommand = new MySqlCommand(selectMessages, connection);
        messageCommand.Parameters.AddWithValue("@cid", ChatroomId);
        messageCommand.Parameters.AddWithValue("@uid", UserId);
        messageCommand.Parameters.AddWithValue("@start", Start);
        messageCommand.Parameters.AddWithValue("@end", End);
        
        
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

        response.RoomId = ChatroomId;
        response.Messages = messages;

        return response;
    }
}