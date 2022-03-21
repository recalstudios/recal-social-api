﻿using Microsoft.AspNetCore.Mvc;
using recal_social_api.Interfaces;
using recal_social_api.Models;
using recal_social_api.Models.Requests;

namespace recal_social_api.Controllers;

[ApiController]
[Route("user")]

public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("user")]
    public User GetUser([FromHeader]string token)
    {
        return _userService.GetUser(token);
    }
    
    [HttpPost("create")]
    public bool CreateUser([FromBody] CreateUserRequest payload)
    {
        return _userService.CreateUser(payload.FirstName, payload.LastName, payload.Username, payload.Email, payload.PhoneNumber, payload.Pass, payload.Pfp);
    }

    [HttpDelete("delete")]
    public bool DeleteUser([FromBody] DeleteUserRequest payload)
    {
        return _userService.DeleteUser(payload.Username);
    }

    [HttpPost("update")]
    public bool UpdateUser([FromBody] UpdateUserRequest payload)
    {
        return _userService.UpdateUser(payload.Token, payload.FirstName, payload.LastName, payload.Email, payload.PhoneNumber, payload.Pfp);
    }
}