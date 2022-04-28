using System.IdentityModel.Tokens.Jwt;
using System.IO.Pipes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recal_social_api.Interfaces;
using recal_social_api.Models.Requests;
using recal_social_api.Models.Responses;

namespace recal_social_api.Controllers;
[ApiController]
[Route("chat")]
public class ChatController : Controller
{ 
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IChatService _chatService;

    public ChatController(IAuthService authService, IUserService userService, IChatService chatService)
    {
        _authService = authService;
        _userService = userService;
        _chatService = chatService;
    }
    
    [Authorize]
    [HttpPost("room/backlog")]
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
        
        return _chatService.GetChatroomMessages(payload.ChatroomId, userId, payload.Start, payload.Length);
    }

    [Authorize]
    [HttpPost("room/message/save")]
    public bool SaveMessage([FromBody] SaveMessageRequest payload)
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

        //  Uses black magic to find out if it is yes
        if (roomlists.Any(x => x.Id == payload.ChatroomId))
        {
            return _chatService.SaveChatMessage(userId, payload.Data, payload.ChatroomId);
        }
        return false;
    }
}