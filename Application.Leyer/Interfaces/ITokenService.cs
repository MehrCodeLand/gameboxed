using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Leyer.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(int userId, string username, IEnumerable<string> roles);
        bool ValidateToken(string token);
        ClaimsPrincipal GetPrincipalFromToken(string token);
        string GenerateRefreshToken();
        Task<bool> SaveRefreshTokenAsync(int userId, string refreshToken);
        Task<bool> RevokeTokenAsync(string token);
        Task<bool> IsTokenActive(string token);
    }
}
