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
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Leyer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public UserRepository(AppDbContext context , IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // fave game part
        public async Task<MyResponse<bool>> AddGameToFavoritesAsync(int userId, int gameId)
        {
            // Check if user exists
            var user = await _context.Users.Include(u => u.FavoriteGames)
                                           .SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return MyResponse<bool>.Error(MyMessageHelper.NotFound);

            // Check if game exists
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
                return MyResponse<bool>.Error("Game not found.");

            // Check if the favorite already exists
            bool alreadyFavorited = await _context.FavoriteGames.AnyAsync(fg => fg.UserId == userId && fg.GameId == gameId);
            if (alreadyFavorited)
                return MyResponse<bool>.Error("Game already in favorites.");

            // Add favorite record
            var favorite = new FavoriteGame { UserId = userId, GameId = gameId };
            await _context.FavoriteGames.AddAsync(favorite);
            await _context.SaveChangesAsync();

            return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);
        }

        public async Task<MyResponse<bool>> RemoveGameFromFavoritesAsync(int userId, int gameId)
        {
            var favorite = await _context.FavoriteGames.SingleOrDefaultAsync(fg => fg.UserId == userId && fg.GameId == gameId);
            if (favorite == null)
                return MyResponse<bool>.Error("Favorite record not found.");

            _context.FavoriteGames.Remove(favorite);
            await _context.SaveChangesAsync();

            return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);
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

            return MyResponse<bool>.Success(MyMessageHelper.TaskDone , true);
        }


        // ----- New Login Implementation -----
        public async Task<MyResponse<string>> LoginAsync(UserLoginDto dto)
        {
            // Find the user by username (assuming case-insensitive)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username.ToLower());
            if (user == null)
                return MyResponse<string>.Error(MyMessageHelper.NotFound);

            // Verify the password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return MyResponse<string>.Error(MyMessageHelper.InvalidCredentials);

            // Generate JWT Token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
                // Optionally add role claims here
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: creds
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return MyResponse<string>.Success(MyMessageHelper.TaskDone, tokenString);
        }

        public async Task<IEnumerable<User>> GetAllAsync() =>
            await _context.Users.ToListAsync();

        public async Task<MyResponse<User>> GetById(int id)
        {
            var user = await _context.Users.SingleOrDefaultAsync(r => r.Id == id);
            if (user == null)
                return MyResponse<User>.Error(MyMessageHelper.NotFound);

            return MyResponse<User>.Success(MyMessageHelper.TaskDone , user );
        }

        public async Task<bool> IsUserExist(string username , string email)
        {
            return await _context.Users.AnyAsync(u => u.Username == username  ||  u.Email == email);
        }
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<MyResponse<bool>> UpdateUser(int id, UserUpdateDto dto)
        {
            var userResponse = await GetById(id);

            if (userResponse.Data == null)
                return MyResponse<bool>.Error(MyMessageHelper.NotFound);

            // Optional: Check if new username/email already exists for another user
            var isExist = await _context.Users.AnyAsync(u =>
                (u.Username == dto.Username.ToLower() || u.Email == dto.Email.ToLower()) &&
                u.Id != id
            );

            if (isExist)
                return MyResponse<bool>.Error(MyMessageHelper.EmailAndUsername);

            var user = userResponse.Data;

            user.Username = dto.Username.ToLower();
            user.Email = dto.Email.ToLower();

            // Optional: Update password if provided (you can change this condition)
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }
            await UpdateAsync(user);

            return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);
        }

        public async Task<MyResponse<bool>> RemoveUser(int id)
        {

            await DeleteAsync(id);
            var userResponse = await GetById(id);

            if (userResponse.Data == null)
                return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);

            return MyResponse<bool>.Error(MyMessageHelper.NotFound);
        }


        // db part 
        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await GetById(id);
            if (user.Data != null)
            {
                _context.Users.Remove(user.Data);
                await _context.SaveChangesAsync();
            }
        }

    }
}
