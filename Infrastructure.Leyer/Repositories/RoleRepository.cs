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
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;
        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync() =>
            await _context.Roles.ToListAsync();

        public async Task<Role> GetByIdAsync(int id) =>
            await _context.Roles.FindAsync(id);

        public async Task AddAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Role role)
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var role = await GetByIdAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }
        }
    }
}
