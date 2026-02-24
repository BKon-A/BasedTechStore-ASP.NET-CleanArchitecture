using BasedTechStore.Domain.Entities.Identity;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string jwtId, string ipAddress);
        Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task RevokeTokenAsync(string refreshToken, string ipAddress, string reason = "Revoked by user");
        Task RevokeAllUserTokensAsync(string userId, string ipAddress);
        /// <summary>
        /// Clean expired tokens (background job)
        /// </summary>
        Task CleanupExpiredTokensAsync();
        Task<IEnumerable<RefreshToken>> GetUserActiveTokensAsync(string userId);
    }
}
