using Api.Leyer.DTOs;
using Application.Leyer.Interfaces;
using Domain.Leyer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace gameboxed.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepo;

    public UserController(IUserRepository userRepository)
    {
        _userRepo = userRepository;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto dto)
    {
        var result = await _userRepo.RegisterASync(dto);
        if (result.IsError)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a user by their ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userRepo.GetById(id);
        if (result.IsError)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateUser(int id, UserUpdateDto dto)
    {
        var result = await _userRepo.UpdateUser(id, dto);
        if (result.IsError)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Removes a user by their ID.
    /// </summary>
    [HttpDelete("remove/{id}")]
    public async Task<IActionResult> RemoveUser(int id)
    {
        var result = await _userRepo.RemoveUser(id);
        if (result.IsError)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Adds a game to the user's favorites.
    /// </summary>
    [HttpPost("{userId}/favorites/{gameId}")]
    public async Task<IActionResult> AddFavorite(int userId, int gameId)
    {
        var result = await _userRepo.AddGameToFavoritesAsync(userId, gameId);
        if (result.IsError)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Removes a game from the user's favorites.
    /// </summary>
    [HttpDelete("{userId}/favorites/{gameId}")]
    public async Task<IActionResult> RemoveFavorite(int userId, int gameId)
    {
        var result = await _userRepo.RemoveGameFromFavoritesAsync(userId, gameId);
        if (result.IsError)
            return NotFound(result);
        return Ok(result);
    }



    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto dto)
    {
        var result = await _userRepo.LoginAsync(dto);
        return result.IsError ? Unauthorized(result) : Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromHeader] string authorization)
    {
        // Expecting the header value to be "Bearer {token}"
        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer "))
            return BadRequest(new { message = "Invalid authorization header." });

        var token = authorization.Substring("Bearer ".Length).Trim();
        var result = await _userRepo.LogoutAsync(token);
        return result.IsError ? BadRequest(result) : Ok(result);
    }



    [Authorize]
    [HttpGet("check")]
    public IActionResult Check()
    {
        return Ok("hello world");
    }
}