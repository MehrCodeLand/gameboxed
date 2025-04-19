using Api.Leyer.Strcuts;
using Domain.Leyer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Leyer.Interfaces
{
    public interface IGameRatingRepository
    {
        Task<MyResponse<IEnumerable<GameRating>>> GetAllAsync();
        Task<MyResponse<GameRating>> GetByIdAsync(int id);
        Task<MyResponse<bool>> AddAsync(GameRating rating);
        Task<MyResponse<bool>> UpdateAsync(GameRating rating);
        Task<MyResponse<bool>> DeleteAsync(int id);
    }
}
