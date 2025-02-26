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
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _context;
        public GameRepository(AppDbContext context)
        {
            _context = context;
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
