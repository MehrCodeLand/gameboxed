using Api.Leyer.DTOs;
using Api.Leyer.Strcuts;
using Application.Leyer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace gameboxed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameRepository _gameRepo;
        private readonly IUserRepository _userRepo;

        public GameController(IGameRepository gameRepo, IUserRepository userRepo)
        {
            _gameRepo = gameRepo;
            _userRepo = userRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGames()
        {
            var games = await _gameRepo.GetAllAsync();
            return Ok(new { Message = "Success", Data = games });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame(int id)
        {
            var game = await _gameRepo.GetByIdAsync(id);
            if (game == null)
                return NotFound(new { Message = "Game not found" });

            return Ok(new { Message = "Success", Data = game });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchGames([FromQuery] string term, [FromQuery] int limit = 3)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { Message = "Search term is required" });

            var result = await _gameRepo.SearchGamesAsync(term, limit);
            if (result.IsError)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{id}/rating")]
        public async Task<IActionResult> GetGameAverageRating(int id)
        {
            var result = await _gameRepo.GetAverageRatingAsync(id);
            if (result.IsError)
                return NotFound(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("{id}/rate")]
        public async Task<IActionResult> RateGame(int id, [FromBody] RateGameDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                return Unauthorized(new { Message = "Invalid user identifier" });

            var result = await _gameRepo.RateGameAsync(userIdInt, id, dto.Rating);
            if (result.IsError)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("played")]
        public async Task<IActionResult> AddToPlayed([FromBody] PlayedGameDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                return Unauthorized(new { Message = "Invalid user identifier" });

            var result = await _userRepo.AddGameToPlayedAsync(userIdInt, dto);
            if (result.IsError)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("played/{gameId}")]
        public async Task<IActionResult> RemoveFromPlayed(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                return Unauthorized(new { Message = "Invalid user identifier" });

            var result = await _userRepo.RemoveGameFromPlayedAsync(userIdInt, gameId);
            if (result.IsError)
                return NotFound(result);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("played")]
        public async Task<IActionResult> GetPlayedGames()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                return Unauthorized(new { Message = "Invalid user identifier" });

            var result = await _userRepo.GetPlayedGamesAsync(userIdInt);
            if (result.IsError)
                return NotFound(result);

            return Ok(result);
        }

        // Admin-only endpoints for game management
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddGame([FromBody] GameDto dto)
        {
            var result = await _gameRepo.AddAsync(dto);
            if (result.IsError)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGame(int id, [FromBody] GameDto dto)
        {
            var result = await _gameRepo.UpdateAsync(id, dto);
            if (result.IsError)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var result = await _gameRepo.DeleteAsync(id);
            if (result.IsError)
                return NotFound(result);

            return Ok(result);
        }
    }
}