using Application.Leyer.Interfaces;
using Domain.Leyer.Entities;
using Infrastructure.Leyer.Helper;
using Infrastructure.Leyer.MyDbSetting;
using Microsoft.EntityFrameworkCore;
using Api.Leyer.DTOs;
using Api.Leyer.Strcuts;
using Microsoft.Extensions.Configuration;


namespace Infrastructure.Leyer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public UserRepository(AppDbContext context, IConfiguration configuration, ITokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            _tokenService = tokenService;
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
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == dto.Username.ToLower());
                
            if (user == null)
                return MyResponse<string>.Error(MyMessageHelper.NotFound);

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return MyResponse<string>.Error(MyMessageHelper.InvalidCredentials);

            // Get user roles
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            Console.WriteLine($"Found roles for user: {string.Join(", ", roles)}");

            // Generate JWT token using token service
            string tokenString = _tokenService.GenerateAccessToken(user.Id, user.Username, roles);

            // Create and save a new login session
            var session = new UserSession
            {
                UserId = user.Id,
                Token = tokenString,
                IsLoggedIn = true,
                LoginTime = DateTime.UtcNow
            };

            await _context.UserSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            return MyResponse<string>.Success(MyMessageHelper.TaskDone, tokenString);
        }

        public async Task<MyResponse<bool>> LogoutAsync(string token)
        {
            bool result = await _tokenService.RevokeTokenAsync(token);

            if (!result)
                return MyResponse<bool>.Error("Active session not found.");

            return MyResponse<bool>.Success("Logout successful.", true);
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



        public async Task<User> GetUserWithFavoriteGames(int userId)
        {
            return await _context.Users
                .Include(u => u.FavoriteGames)
                .ThenInclude(fg => fg.Game)
                .SingleOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<MyResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return MyResponse<bool>.Error(MyMessageHelper.NotFound);

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return MyResponse<bool>.Error("Current password is incorrect");

            // Verify new password and confirmation match
            if (dto.NewPassword != dto.ConfirmPassword)
                return MyResponse<bool>.Error("New password and confirmation do not match");

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await UpdateAsync(user);

            return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);
        }


        // Add these methods to the UserRepository class

        public async Task<MyResponse<bool>> AddGameToPlayedAsync(int userId, PlayedGameDto dto)
        {
            // Check if user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return MyResponse<bool>.Error(MyMessageHelper.NotFound);

            // Check if game exists
            var game = await _context.Games.FindAsync(dto.GameId);
            if (game == null)
                return MyResponse<bool>.Error("Game not found.");

            // Check if the game is already in played list
            bool alreadyPlayed = await _context.PlayedGames
                .AnyAsync(pg => pg.UserId == userId && pg.GameId == dto.GameId);

            if (alreadyPlayed)
                return MyResponse<bool>.Error("Game already in played list.");

            // Validate review length
            if (dto.Review != null && dto.Review.Length > 200)
                return MyResponse<bool>.Error("Review cannot exceed 200 characters.");

            // Add played record
            var playedGame = new PlayedGame
            {
                UserId = userId,
                GameId = dto.GameId,
                PlayedDate = DateTime.UtcNow,
                Review = dto.Review
            };

            await _context.PlayedGames.AddAsync(playedGame);
            await _context.SaveChangesAsync();

            return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);
        }

        public async Task<MyResponse<bool>> RemoveGameFromPlayedAsync(int userId, int gameId)
        {
            var playedGame = await _context.PlayedGames
                .SingleOrDefaultAsync(pg => pg.UserId == userId && pg.GameId == gameId);

            if (playedGame == null)
                return MyResponse<bool>.Error("Played game record not found.");

            _context.PlayedGames.Remove(playedGame);
            await _context.SaveChangesAsync();

            return MyResponse<bool>.Success(MyMessageHelper.TaskDone, true);
        }

        public async Task<MyResponse<IEnumerable<PlayedGame>>> GetPlayedGamesAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.PlayedGames)
                    .ThenInclude(pg => pg.Game)
                .SingleOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return MyResponse<IEnumerable<PlayedGame>>.Error(MyMessageHelper.NotFound);

            return MyResponse<IEnumerable<PlayedGame>>.Success(
                MyMessageHelper.TaskDone,
                user.PlayedGames);
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
