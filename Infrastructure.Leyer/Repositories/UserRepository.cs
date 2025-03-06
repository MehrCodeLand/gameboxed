using Application.Leyer.Interfaces;
using Domain.Leyer.Entities;
using Infrastructure.Leyer.Helper;
using Infrastructure.Leyer.MyDbSetting;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Leyer.DTOs;
using Api.Leyer.Strcuts;

namespace Infrastructure.Leyer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MyResponse<bool>> RegisterASync(UserRegisterDto dto )
        {
            var IsExist = await IsUserExist(dto.Username.ToLower() , dto.Email.ToLower());
            if (IsExist) return MyResponse<bool>.Error(MyMessageHelper.EmailAndUsername) ;


            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                userRole = new Role { Name = "User" };
                await _context.Roles.AddAsync(userRole);
                await _context.SaveChangesAsync();
            }

            // time to add user 
            var user = new User
            {
                Username = dto.Username.ToLower(),
                Email = dto.Email.ToLower(),
                UserRoles = new List<UserRole> { new UserRole { Role = userRole } }, 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await AddAsync(user);

            return MyResponse<bool>.Success(MyMessageHelper.TaskDone);
        }


        public async Task<IEnumerable<User>> GetAllAsync() =>
            await _context.Users.ToListAsync();

        public async Task<User> GetByIdAsync(int id) =>
            await _context.Users.FindAsync(id);


        public async Task<bool> IsUserExist(string username , string email)
        {
            return await _context.Users.AnyAsync(u => u.Username == username  ||  u.Email == email);
        }
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
