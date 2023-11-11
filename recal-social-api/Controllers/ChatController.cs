using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recal_social_api.Interfaces;
using recal_social_api.Models;
using recal_social_api.Models.Requests;
using recal_social_api.Models.Responses;

namespace recal_social_api.Controllers;
[ApiController]
[Route("v1/chat")]
public class ChatController : Controller
{
    private readonly IUserService _userService;
    private readonly IChatService _chatService;

    public ChatController(IUserService userService, IChatService chatService)
    {
        _userService = userService;
        _chatService = chatService;
    }

// Message part of chatrooms
    [Authorize]
    [HttpPost("room/backlog")]
    // Gets the backlog from a room with the chatroom id, start and length
    public GetChatroomMessagesResponse GetChatLog([FromBody] GetChatroomMessagesRequests payload)
    {
        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];

        //  Cuts out the Bearer part of the header
        var stream = authHeader.Substring("Bearer ".Length).Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return _chatService.GetChatroomMessages(payload.ChatroomId, userId, payload.Start, payload.Length);
    }

    [Authorize]
    [HttpPost("room/message/save")]
    // Save a message with the roomid and with content
    public IActionResult SaveMessage([FromBody] SaveMessageRequest payload)
    {
        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];

        //  Cuts out the Bearer part of the header
        var stream = authHeader.Substring("Bearer ".Length).Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        //  List of all rooms user is in
        var roomlists = _userService.GetUserChatrooms(userId);

        //  Uses black magic to find out if user is in chatroom
        if (roomlists.Any(x => x.Id == payload.Room))
        {
            return Ok(_chatService.SaveChatMessage(userId, payload.Room, payload.Content));
        }
        return Unauthorized();
    }

    [Authorize]
    [HttpPost("room/message/delete")]
    // Deletes the message with the message id
    public bool DeleteMessage([FromBody] DeleteMessageRequest payload)
    {
        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];

        //  Cuts out the Bearer part of the header
        var stream = authHeader.Substring("Bearer ".Length).Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return _chatService.DeleteChatMessage(payload.MessageId, userId);
    }

// Room part of chatrooms

    [Authorize]
    [HttpPost("room/create")]
    // Creates the chatroom with a name and a pass
    public bool CreateChatroom([FromBody] CreateChatroomRequest payload)
    {
        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];

        //  Cuts out the Bearer part of the header
        var stream = authHeader.Substring("Bearer ".Length).Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return _chatService.CreateChatroom(payload.Name, payload.Pass, userId);
    }

    [Authorize]
    [HttpPost("room/details")]
    // Gives detailed information on the room with the userid and chatroom id
    public Chatroom DetailsChatroom([FromBody] DetailsChatroomRequest payload)
    {
        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];

        //  Cuts out the Bearer part of the header
        var stream = authHeader.Substring("Bearer ".Length).Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return _chatService.DetailsChatroom(userId, payload.ChatroomId);
    }

    [Authorize]
    [HttpPost("room/update")]
    // Updates the chatroom with name, image or pass
    public bool UpdateChatroom([FromBody] UpdateChatroomRequest payload)
    {
        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];

        //  Cuts out the Bearer part of the header
        var stream = authHeader.Substring("Bearer ".Length).Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return _chatService.UpdateChatroom(userId, payload.ChatroomId, payload.Name, payload.Image, payload.Pass);
    }

    [Authorize]
    [HttpPost("room/delete")]
    // Delete rooms with user id and chatroom id
    public bool DeleteChatroom([FromBody] DeleteChatroomRequest payload)
    {
        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];

        //  Cuts out the Bearer part of the header
        var stream = authHeader.Substring("Bearer ".Length).Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return _chatService.DeleteChatroom(userId, payload.ChatroomId);
    }

    [Authorize]
    [HttpPost("room/join")]
    // Lets users join chatrooms with userid, room code and room pass
    public bool JoinChatroom([FromBody] JoinChatroomRequest payload)
    {
        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];

        //  Cuts out the Bearer part of the header
        var stream = authHeader.Substring("Bearer ".Length).Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        // Runs the service
        return _chatService.JoinChatroom(payload.Code, payload.Pass, userId);
    }

    [Authorize]
    [HttpPost("room/leave")]
    // Lets users leave chatrooms with their authtoken and chatroom id
    public bool LeaveChatroom([FromBody] LeaveChatroomRequest payload)
    {
        //  Gets the http request headers
        HttpContext httpContext = HttpContext;
        string authHeader = httpContext.Request.Headers["Authorization"];

        //  Cuts out the Bearer part of the header
        var stream = authHeader.Substring("Bearer ".Length).Trim();

        //  Does some JWT magic
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;

        //  Sets the variable username to the username from the token
        var userId = int.Parse(tokenS!.Claims.First(claim => claim.Type == "UserId").Value);

        return _chatService.LeaveChatroom(userId, payload.ChatroomId);
    }
}
