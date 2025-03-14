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
            Task<MyResponse<bool>> RateGameAsync(int userId, int gameId, int rating);
        }
    }
}
