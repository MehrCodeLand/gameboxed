using Api.Leyer.DTOs;
using Application.Leyer.Interfaces;
using Azure;
using Domain.Leyer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace gameboxed.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly ITokenService _tokenService;
    private readonly IGameRepository _gameRepo;
    public UserController(IUserRepository userRepository, ITokenService tokenService, IGameRepository gameRepo)
    {
        _userRepo = userRepository;
        _tokenService = tokenService;
        _gameRepo = gameRepo;
    }



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

        // Get the current user's claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            Message = "Authentication successful",
            UserId = userId,
            Username = username,
            Roles = roles
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-check")]
    public IActionResult AdminCheck()
    {
        return Ok(new { Message = "You have admin privileges" });
    }

    [Authorize(Roles = "User")]
    [HttpGet("user-check")]
    public IActionResult UserCheck()
    {
        return Ok(new { Message = "You have user privileges" });
    }





    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userRepo.GetAllAsync();
        return Ok(new { Message = "Success", Data = users });
    }


    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            return Unauthorized(new { Message = "Invalid user identifier" });

        var result = await _userRepo.GetById(userIdInt);
        if (result.IsError)
            return NotFound(result);

        return Ok(result);
    }





    // Method to get all users - Admin only
    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userRepo.GetAllAsync();
        return Ok(new { Message = "Success", Data = users });
    }

    // Method to get current user profile
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            return Unauthorized(new { Message = "Invalid user identifier" });

        var result = await _userRepo.GetById(userIdInt);
        if (result.IsError)
            return NotFound(result);

        return Ok(result);
    }

    // Method to get user's favorite games
    [Authorize]
    [HttpGet("my-favorites")]
    public async Task<IActionResult> GetMyFavoriteGames()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            return Unauthorized(new { Message = "Invalid user identifier" });

        // This method needs to be added to the user repository
        var user = await _userRepo.GetUserWithFavoriteGames(userIdInt);
        if (user == null)
            return NotFound(new { Message = "User not found" });

        return Ok(new { Message = "Success", FavoriteGames = user.FavoriteGames.Select(fg => fg.Game) });
    }

    // Method to change user password
    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            return Unauthorized(new { Message = "Invalid user identifier" });

        // This method needs to be added to the user repository
        var result = await _userRepo.ChangePasswordAsync(userIdInt, dto);
        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

    // Method to rate a game
    [Authorize]
    [HttpPost("rate-game/{gameId}")]
    public async Task<IActionResult> RateGame(int gameId, [FromBody] RateGameDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            return Unauthorized(new { Message = "Invalid user identifier" });

        var result = await _gameRepo.RateGameAsync(userIdInt, gameId, dto.Rating);
        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }



}




//{
//  "username": "aria",
//  "email": "aa@gmail.com",
//  "password": "1234",
//  "rePassword": "1234"
//}



//{
//  "message": "Done",
//  "isError": false,
//  "data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwidW5pcXVlX25hbWUiOiJhcmlhIiwiZXhwIjoxNzQ0ODkwMDU3LCJpc3MiOiJNeUdhbWVBcHAiLCJhdWQiOiJNeUdhbWVBcHBVc2VycyJ9.nmJ9y38pzOIFHhT8n4Yg6_mutrjIwiM25098SdLR6I0"
//}




	
//Response body
//Download
//{
//  "message": "Done",
//  "isError": false,
//  "data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwidW5pcXVlX25hbWUiOiJhcmlhIiwianRpIjoiZWNkYTFhMDktMmRhMy00OWE5LWI1MGUtNjNjYWQwN2JlN2E0IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImV4cCI6MTc0NDg5NjI5OCwiaXNzIjoiTXlHYW1lQXBwIiwiYXVkIjoiTXlHYW1lQXBwVXNlcnMifQ.oyEcChDxIbsVgVRVcExIUqOYS_NCA0pIlDoOQ570bOA"
//}