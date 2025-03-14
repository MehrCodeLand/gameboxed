using Api.Leyer.Strcuts;
using Application.Leyer.Interfaces;
using Domain.Leyer.Entities;
using Infrastructure.Leyer.Helper;
using Infrastructure.Leyer.MyDbSetting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Leyer.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _context;
        public GameRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MyResponse<bool>> RateGameAsync(int userId, int gameId, int rating)
        {
            // Validate rating (assuming rating is 1-5)
            if (rating < 1 || rating > 5)
                return MyResponse<bool>.Error("Rating must be between 1 and 5.");

            // Check if game exists
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
                return MyResponse<bool>.Error("Game not found.");

            // Check if the user has already rated this game
            var existingRating = await _context.GameRatings.SingleOrDefaultAsync(gr => gr.UserId == userId && gr.GameId == gameId);

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

            await _context.SaveChangesAsync();
            return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);
        }



        public async Task<IEnumerable<Game>> GetAllAsync() =>
            await _context.Games.ToListAsync();

        public async Task<Game> GetByIdAsync(int id) =>
            await _context.Games.FindAsync(id);

        public async Task AddAsync(Game game)
        {
            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Game game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var game = await GetByIdAsync(id);
            if (game != null)
            {
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
            }
        }
    }
}
