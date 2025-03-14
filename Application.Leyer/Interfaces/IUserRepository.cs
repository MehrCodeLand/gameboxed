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
        // USER MANAGEMENT

        /// <summary>
        /// Registers a new user.
        /// </summary>
        Task<MyResponse<bool>> RegisterASync(UserRegisterDto dto);

        /// <summary>
        /// Gets all users.
        /// </summary>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// Gets a single user by ID.
        /// </summary>
        Task<MyResponse<User>> GetById(int id);

        /// <summary>
        /// Checks if a user exists by username or email.
        /// </summary>
        Task<bool> IsUserExist(string username, string email);

        /// <summary>
        /// Adds a user entity (low-level add).
        /// </summary>
        Task AddAsync(User user);

        /// <summary>
        /// Updates a user by ID with the provided DTO.
        /// </summary>
        Task<MyResponse<bool>> UpdateUser(int id, UserUpdateDto dto);

        /// <summary>
        /// Removes a user by ID.
        /// </summary>
        Task<MyResponse<bool>> RemoveUser(int id);

        /// <summary>
        /// Adds a game to a user's favorites.
        /// </summary>
        Task<MyResponse<bool>> AddGameToFavoritesAsync(int userId, int gameId);

        /// <summary>
        /// Removes a game from a user's favorites.
        /// </summary>
        Task<MyResponse<bool>> RemoveGameFromFavoritesAsync(int userId, int gameId);
        Task<MyResponse<string>> LoginAsync(UserLoginDto dto);

    }
}

