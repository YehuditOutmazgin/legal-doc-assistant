using LegalDoc.Core.Models;

namespace LegalDoc.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        
        // Async methods (preferred)
        Task<int?> ValidateRefreshTokenAsync(string refreshToken);
        Task StoreRefreshTokenAsync(string token, int userId, DateTime expiry);
        Task RevokeRefreshTokenAsync(string token);
        
        // Sync methods (for backward compatibility)
        int? ValidateRefreshToken(string refreshToken);
        void StoreRefreshToken(string token, int userId, DateTime expiry);
        void RevokeRefreshToken(string token);
    }
}
