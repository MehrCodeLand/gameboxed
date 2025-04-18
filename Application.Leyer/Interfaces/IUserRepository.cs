using Api.Leyer.DTOs;
using Api.Leyer.Strcuts;
using Azure;
using Domain.Leyer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Leyer.Interfaces
{
    public interface IUserRepository
    {
        Task<MyResponse<bool>> RegisterASync(UserRegisterDto dto);
        Task<IEnumerable<User>> GetAllAsync();
        Task<MyResponse<User>> GetById(int id);
        Task<bool> IsUserExist(string username, string email);
        Task AddAsync(User user);
        Task<MyResponse<bool>> UpdateUser(int id, UserUpdateDto dto);
        Task<MyResponse<bool>> RemoveUser(int id);
        Task<MyResponse<bool>> AddGameToFavoritesAsync(int userId, int gameId);
        Task<MyResponse<bool>> RemoveGameFromFavoritesAsync(int userId, int gameId);
        Task<MyResponse<string>> LoginAsync(UserLoginDto dto);
        Task<MyResponse<bool>> LogoutAsync(string token);
        Task<MyResponse<bool>> RemoveGameFromPlayedAsync(int userId, int gameId);
        Task<MyResponse<bool>> AddGameToPlayedAsync(int userId, PlayedGameDto dto);
        Task<MyResponse<IEnumerable<PlayedGame>>> GetPlayedGamesAsync(int userId);
        Task<User> GetUserWithFavoriteGames(int userId);
        Task<MyResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    }
}

