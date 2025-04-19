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
        private readonly ILogger<GameController> _logger;

        public GameController(IGameRepository gameRepo, IUserRepository userRepo, ILogger<GameController> logger)
        {
            _gameRepo = gameRepo;
            _userRepo = userRepo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGames()
        {
            _logger.LogInformation("Retrieving all games");
            var result = await _gameRepo.GetAllAsync();

            if (result.IsError)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame(int id)
        {
            _logger.LogInformation("Retrieving game with ID: {GameId}", id);
            var result = await _gameRepo.GetByIdAsync(id);

            if (result.IsError)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchGames([FromQuery] string term, [FromQuery] int limit = 3)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                _logger.LogWarning("Search attempted with empty term");
                return BadRequest(new MyResponse<string> { Message = "Search term is required", IsError = true });
            }

            _logger.LogInformation("Searching games with term: {SearchTerm}, limit: {Limit}", term, limit);
            var result = await _gameRepo.SearchGamesAsync(term, limit);

            if (result.IsError)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{id}/rating")]
        public async Task<IActionResult> GetGameAverageRating(int id)
        {
            _logger.LogInformation("Getting average rating for game ID: {GameId}", id);
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
            {
                _logger.LogWarning("Invalid user identifier in token");
                return Unauthorized(new MyResponse<string> { Message = "Invalid user identifier", IsError = true });
            }

            _logger.LogInformation("User {UserId} rating game {GameId} with score {Rating}", userIdInt, id, dto.Rating);
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
            {
                _logger.LogWarning("Invalid user identifier in token");
                return Unauthorized(new MyResponse<string> { Message = "Invalid user identifier", IsError = true });
            }

            _logger.LogInformation("User {UserId} adding game {GameId} to played list", userIdInt, dto.GameId);
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
            {
                _logger.LogWarning("Invalid user identifier in token");
                return Unauthorized(new MyResponse<string> { Message = "Invalid user identifier", IsError = true });
            }

            _logger.LogInformation("User {UserId} removing game {GameId} from played list", userIdInt, gameId);
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
            {
                _logger.LogWarning("Invalid user identifier in token");
                return Unauthorized(new MyResponse<string> { Message = "Invalid user identifier", IsError = true });
            }

            _logger.LogInformation("User {UserId} retrieving played games", userIdInt);
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Admin user {UserId} adding new game: {GameTitle}", userId, dto.Title);

            var result = await _gameRepo.AddAsync(dto);
            if (result.IsError)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGame(int id, [FromBody] GameDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Admin user {UserId} updating game {GameId}", userId, id);

            var result = await _gameRepo.UpdateAsync(id, dto);
            if (result.IsError)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Admin user {UserId} deleting game {GameId}", userId, id);

            var result = await _gameRepo.DeleteAsync(id);
            if (result.IsError)
                return NotFound(result);

            return Ok(result);
        }
    }
}