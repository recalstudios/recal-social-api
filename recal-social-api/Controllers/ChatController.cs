using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recal_social_api.Interfaces;
using recal_social_api.Models;
using recal_social_api.Models.Requests;
using recal_social_api.Models.Responses;

namespace recal_social_api.Controllers;
[ApiController]
[Route("v1/chat/room")] // this should probably be changed at some point, given that it has two static levels of routing after v1/
public class ChatController(IUserService userService, IChatService chatService, ILogger<ChatController> logger) : Controller
{
    private readonly ILogger _logger = logger;

    // Message part of chatrooms
    [Authorize]
    [HttpPost("backlog")]
    // Gets the backlog from a room with the chatroom id, start and length
    public GetChatroomMessagesResponse GetChatLog([FromBody] GetChatroomMessagesRequests payload)
    {
        _logger.LogInformation("Got 'backlog' request");

        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return chatService.GetChatroomMessages(payload.ChatroomId, userId, payload.Start, payload.Length);
    }

    [Authorize]
    [HttpPost("message/save")]
    // Save a message with the room id and with content
    public IActionResult SaveMessage([FromBody] SaveMessageRequest payload)
    {
        _logger.LogInformation("Got 'message/save' request");

        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        //  List of all rooms user is in
        var roomList = userService.GetUserChatrooms(userId);

        //  Uses black magic to find out if user is in chatroom
        if (roomList.Any(x => x.Id == payload.Room))
        {
            return Ok(chatService.SaveChatMessage(userId, payload.Room, payload.Content));
        }
        return Unauthorized();
    }

    [Authorize]
    [HttpPost("message/delete")]
    // Deletes the message with the message id
    public bool DeleteMessage([FromBody] DeleteMessageRequest payload)
    {
        _logger.LogInformation("Got 'message/delete' request");

        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return chatService.DeleteChatMessage(payload.MessageId, userId);
    }

// Room part of chatrooms

    [Authorize]
    [HttpPost("create")]
    // Creates the chatroom with a name and a pass
    public bool CreateChatroom([FromBody] CreateChatroomRequest payload)
    {
        _logger.LogInformation("Got 'create' request");

        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return chatService.CreateChatroom(payload.Name, payload.Pass, userId);
    }

    [Authorize]
    [HttpPost("details")]
    // Gives detailed information on the room with the userid and chatroom id
    public Chatroom DetailsChatroom([FromBody] DetailsChatroomRequest payload)
    {
        _logger.LogInformation("Got 'details' request");

        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return chatService.DetailsChatroom(userId, payload.ChatroomId);
    }

    [Authorize]
    [HttpPost("update")]
    // Updates the chatroom with name, image or pass
    public bool UpdateChatroom([FromBody] UpdateChatroomRequest payload)
    {
        _logger.LogInformation("Got 'update' request");

        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return chatService.UpdateChatroom(userId, payload.ChatroomId, payload.Name, payload.Image, payload.Pass);
    }

    [Authorize]
    [HttpPost("delete")]
    // Delete rooms with user id and chatroom id
    public bool DeleteChatroom([FromBody] DeleteChatroomRequest payload)
    {
        _logger.LogInformation("Got 'delete' request");

        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return chatService.DeleteChatroom(userId, payload.ChatroomId);
    }

    [Authorize]
    [HttpPost("join")]
    // Lets users join chatrooms with userid, room code and room pass
    public bool JoinChatroom([FromBody] JoinChatroomRequest payload)
    {
        _logger.LogInformation("Got 'join' request");

        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return chatService.JoinChatroom(payload.Code, payload.Pass, userId);
    }

    [Authorize]
    [HttpPost("leave")]
    // Lets users leave chatrooms with their auth token and chatroom id
    public bool LeaveChatroom([FromBody] LeaveChatroomRequest payload)
    {
        _logger.LogInformation("Got 'leave' request");

        // It should be safe to assume that this is never null, because asp.net probably handles the authorization part?
        string authHeader = HttpContext.Request.Headers.Authorization!;

        // Cut out the Bearer part of the header
        // This uses range indexing instead of substring now
        var stream = authHeader["Bearer ".Length..].Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        return chatService.LeaveChatroom(userId, payload.ChatroomId);
    }
}
