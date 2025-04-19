using Api.Leyer.Strcuts;
using Application.Leyer.Interfaces;
using Domain.Leyer.Entities;
using Infrastructure.Leyer.MyDbSetting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Leyer.Repositories
{
    public class GameRatingRepository : IGameRatingRepository
    {
        private readonly AppDbContext _context;
        public GameRatingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MyResponse<IEnumerable<GameRating>>> GetAllAsync()
        {
            var ratings = await _context.GameRatings.ToListAsync();
            return MyResponse<IEnumerable<GameRating>>.Success("Ratings retrieved successfully", ratings);
        }

        public async Task<MyResponse<GameRating>> GetByIdAsync(int id)
        {
            var rating = await _context.GameRatings.FindAsync(id);
            if (rating == null)
                return MyResponse<GameRating>.Error("Rating not found");

            return MyResponse<GameRating>.Success("Rating retrieved successfully", rating);
        }

        public async Task<MyResponse<bool>> AddAsync(GameRating rating)
        {
            await _context.GameRatings.AddAsync(rating);
            await _context.SaveChangesAsync();
            return MyResponse<bool>.Success("Rating added successfully", true);
        }

        public async Task<MyResponse<bool>> UpdateAsync(GameRating rating)
        {
            _context.GameRatings.Update(rating);
            await _context.SaveChangesAsync();
            return MyResponse<bool>.Success("Rating updated successfully", true);
        }

        public async Task<MyResponse<bool>> DeleteAsync(int id)
        {
            var rating = await _context.GameRatings.FindAsync(id);
            if (rating == null)
                return MyResponse<bool>.Error("Rating not found");

            _context.GameRatings.Remove(rating);
            await _context.SaveChangesAsync();
            return MyResponse<bool>.Success("Rating deleted successfully", true);
        }
    }
}
