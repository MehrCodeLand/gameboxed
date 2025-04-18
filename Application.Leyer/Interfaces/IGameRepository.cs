using Api.Leyer.DTOs;
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
        Task<IEnumerable<Game>> GetAllAsync();
        Task<Game> GetByIdAsync(int id);
        Task<MyResponse<bool>> RateGameAsync(int userId, int gameId, int rating);
        Task<MyResponse<double>> GetAverageRatingAsync(int gameId);
        Task<MyResponse<bool>> AddAsync(GameDto gameDto);
        Task<MyResponse<bool>> UpdateAsync(int id, GameDto gameDto);
        Task<MyResponse<bool>> DeleteAsync(int id);
        Task<MyResponse<IEnumerable<Game>>> SearchGamesAsync(string searchTerm, int limit = 3);


    }
}
