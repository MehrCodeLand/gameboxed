using Api.Leyer.DTOs;
using Api.Leyer.Strcuts;
using Application.Leyer.Interfaces;
using Domain.Leyer.Entities;
using Infrastructure.Leyer.Helper;
using Infrastructure.Leyer.MyDbSetting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Leyer.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GameRepository> _logger;
        public GameRepository(AppDbContext context , ILogger<GameRepository> logger )
        {
            _logger = logger;
            _context = context;
        }

        public async Task<MyResponse<bool>> RateGameAsync(int userId, int gameId, int rating)
        {
            _logger.LogInformation("User {UserId} is rating game {GameId} with score {Rating}", userId, gameId, rating);

            // Validate rating (assuming rating is 0-5)
            if (rating < 0 || rating > 5)
                return MyResponse<bool>.Error("Rating must be between 0 and 5.");

            // Check if game exists
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
                return MyResponse<bool>.Error("Game not found.");

            // Check if the user has already rated this game
            var existingRating = await _context.GameRatings
                .SingleOrDefaultAsync(gr => gr.UserId == userId && gr.GameId == gameId);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.Rating = rating;
                existingRating.RatedAt = DateTime.UtcNow;
                _context.GameRatings.Update(existingRating);
            }
            else
            {
                // Create new rating
                var newRating = new GameRating
                {
                    UserId = userId,
                    GameId = gameId,
                    Rating = rating,
                    RatedAt = DateTime.UtcNow
                };
                await _context.GameRatings.AddAsync(newRating);
            }

            // Also mark game as played by the user if not already
            var isPlayed = await _context.PlayedGames
                .AnyAsync(pg => pg.UserId == userId && pg.GameId == gameId);

            if (!isPlayed)
            {
                var playedGame = new PlayedGame
                {
                    UserId = userId,
                    GameId = gameId,
                    PlayedDate = DateTime.UtcNow
                };
                await _context.PlayedGames.AddAsync(playedGame);
            }

            await _context.SaveChangesAsync();
            return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);
        }

        public async Task<MyResponse<double>> GetAverageRatingAsync(int gameId)
        {
            // Check if game exists
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
                return MyResponse<double>.Error("Game not found.");

            // Get all ratings for the game
            var ratings = await _context.GameRatings
                .Where(gr => gr.GameId == gameId && gr.Rating.HasValue)
                .Select(gr => gr.Rating.Value)
                .ToListAsync();

            if (!ratings.Any())
                return MyResponse<double>.Success("No ratings yet.", 0);

            // Calculate average
            double average = ratings.Average();
            return MyResponse<double>.Success("Average rating calculated.", average);
        }

        public async Task<MyResponse<IEnumerable<Game>>> GetAllAsync()
        {
            var games = await _context.Games
                .Include(g => g.GameRatings)
                .ToListAsync();

            return MyResponse<IEnumerable<Game>>.Success("Games retrieved successfully", games);
        }

        public async Task<MyResponse<bool>> AddAsync(GameDto gameDto)
        {
            _logger.LogInformation("New game added: {Title}", gameDto.Title);

            // Check if game with same title already exists
            var existingGame = await _context.Games
                .FirstOrDefaultAsync(g => g.Title.ToLower() == gameDto.Title.ToLower());

            if (existingGame != null)
                return MyResponse<bool>.Error("A game with this title already exists.");

            var game = new Game
            {
                Title = gameDto.Title,
                Description = gameDto.Description
            };

            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();
            return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);
        }

        public async Task<MyResponse<bool>> UpdateAsync(int id, GameDto gameDto)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
                return MyResponse<bool>.Error("Game not found.");

            // Check if there's another game with the same title
            var existingGame = await _context.Games
                .FirstOrDefaultAsync(g => g.Title.ToLower() == gameDto.Title.ToLower() && g.Id != id);

            if (existingGame != null)
                return MyResponse<bool>.Error("Another game with this title already exists.");

            game.Title = gameDto.Title;
            game.Description = gameDto.Description;

            _context.Games.Update(game);
            await _context.SaveChangesAsync();
            return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);
        }

        public async Task<MyResponse<bool>> DeleteAsync(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
                return MyResponse<bool>.Error("Game not found.");

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);
        }

        public async Task<MyResponse<IEnumerable<Game>>> SearchGamesAsync(string searchTerm, int limit = 3)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return MyResponse<IEnumerable<Game>>.Error("Search term cannot be empty.");

            var normalizedSearchTerm = searchTerm.ToLower();

            // First look for exact matches
            var exactMatches = await _context.Games
                .Where(g => g.Title.ToLower() == normalizedSearchTerm)
                .Take(limit)
                .ToListAsync();

            if (exactMatches.Any() && exactMatches.Count >= limit)
                return MyResponse<IEnumerable<Game>>.Success("Exact matches found.", exactMatches);

            // Then look for games that contain the search term
            var remainingLimit = limit - exactMatches.Count;
            var partialMatches = await _context.Games
                .Where(g => g.Title.ToLower().Contains(normalizedSearchTerm) &&
                       !exactMatches.Select(em => em.Id).Contains(g.Id))
                .Take(remainingLimit)
                .ToListAsync();

            var results = exactMatches.Concat(partialMatches).ToList();

            if (!results.Any())
                return MyResponse<IEnumerable<Game>>.Error("No games found matching search criteria.");

            return MyResponse<IEnumerable<Game>>.Success("Games found.", results);
        }
        public async Task<MyResponse<Game>> GetByIdAsync(int id)
        {
            var game = await _context.Games
                .Include(g => g.GameRatings)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
                return MyResponse<Game>.Error("Game not found");

            return MyResponse<Game>.Success("Game retrieved successfully", game);
        }
    }
}