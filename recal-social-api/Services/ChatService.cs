﻿using System.Globalization;
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

    
    public int SaveChatMessage(int UserId, string Data, int ChatId)
    {
        var id = 0;
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
       
        
        const string selectMessages = "insert into recal_social_database.messages (uid, data, cid) values (@uid, @data, @cid)";
        var messageCommand = new MySqlCommand(selectMessages, connection);
        messageCommand.Parameters.AddWithValue("@uid", UserId);
        messageCommand.Parameters.AddWithValue("@data", Data);
        messageCommand.Parameters.AddWithValue("@cid", ChatId);
        
        const string readMessages = "select id from recal_social_database.messages where uid = @uid and data = @data and cid = @cid";
        var readCommand = new MySqlCommand(readMessages, connection);
        readCommand.Parameters.AddWithValue("@uid", UserId);
        readCommand.Parameters.AddWithValue("@data", Data);
        readCommand.Parameters.AddWithValue("@cid", ChatId);
        
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

    public bool DeleteChatMessage(int MessageId, int UserId)
    {
        Int64 count = 0;
        using var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
       
        
        const string selectMessages = "delete from recal_social_database.messages where id = @mid and uid = @uid";
        var messageCommand = new MySqlCommand(selectMessages, connection);
        messageCommand.Parameters.AddWithValue("@mid", MessageId);
        messageCommand.Parameters.AddWithValue("@uid", UserId);
        
        
        
        const string readMessages = "select count(*) from recal_social_database.messages where id = @mid and uid = @uid";
        var readCommand = new MySqlCommand(readMessages, connection);
        readCommand.Parameters.AddWithValue("@mid", MessageId);
        readCommand.Parameters.AddWithValue("@uid", UserId);
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
}