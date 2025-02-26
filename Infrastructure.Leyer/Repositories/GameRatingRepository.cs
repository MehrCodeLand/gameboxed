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

        public async Task<IEnumerable<GameRating>> GetAllAsync() =>
            await _context.GameRatings.ToListAsync();

        public async Task<GameRating> GetByIdAsync(int id) =>
            await _context.GameRatings.FindAsync(id);

        public async Task AddAsync(GameRating rating)
        {
            await _context.GameRatings.AddAsync(rating);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(GameRating rating)
        {
            _context.GameRatings.Update(rating);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rating = await GetByIdAsync(id);
            if (rating != null)
            {
                _context.GameRatings.Remove(rating);
                await _context.SaveChangesAsync();
            }
        }
    }
}
