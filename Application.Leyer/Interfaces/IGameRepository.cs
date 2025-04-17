using Api.Leyer.Strcuts;
using Domain.Leyer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Leyer.Interfaces
{
    public interface IGameRepository
    {
        public interface IGameRepository
        {
            /// <summary>
            /// Gets all games.
            /// </summary>
            Task<IEnumerable<Game>> GetAllAsync();

            /// <summary>
            /// Gets a game by its ID.
            /// </summary>
            Task<Game> GetByIdAsync(int id);

            /// <summary>
            /// Rates a game.
            /// </summary>
            Task<MyResponse<bool>> RateGameAsync(int userId, int gameId, int rating);

            /// <summary>
            /// Adds a new game.
            /// </summary>
            Task AddAsync(Game game);

            /// <summary>
            /// Updates an existing game.
            /// </summary>
            Task UpdateAsync(Game game);

            /// <summary>
            /// Deletes a game.
            /// </summary>
            Task DeleteAsync(int id);
        }
    }
}
