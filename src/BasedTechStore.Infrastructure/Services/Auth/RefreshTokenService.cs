using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Domain.Entities.Identity;
using BasedTechStore.Domain.Exceptions;
using BasedTechStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace BasedTechStore.Infrastructure.Services.Auth
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IPermissionService _permissionService;
        private readonly IConfiguration _configuration;

        public RefreshTokenService(AppDbContext context, ITokenService tokenService,
            IPermissionService permissionService, IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _permissionService = permissionService;
            _configuration = configuration;
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string jwtId, string ipAddress)
        {
            var tokenBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            var token = Convert.ToBase64String(tokenBytes);

            var tokenHash = HashToken(token);

            var refreshToken = new RefreshToken
            {
                TokenHash = tokenHash,
                UserId = userId,
                JwtId = jwtId,
                CreatedByIP = ipAddress,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7")),
                IsUsed = false,
                IsRevoked = false,
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            // return original token(not hash)
            refreshToken.TokenHash = token;
            return refreshToken;
        }

        public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var tokenHash = HashToken(refreshToken);

            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash)
                ?? throw new UnauthorizedException("Invalid refresh token");

            if (!storedToken.IsActive)
            {
                await RevokeTokenChainAsync(storedToken, ipAddress, "Attempted reuse of revoked token");
                throw new UnauthorizedException("Invalid refresh token");
            }

            if (storedToken.IsExpired)
            {
                throw new UnauthorizedException("Refresh token has expired");
            }

            // Mark old token as "used"
            storedToken.IsUsed = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokeByIP = ipAddress;

            // Generete new access token section
            var user = storedToken.User;
            var permissions = _permissionService.GetAllPermissions(user.Role, user.CustomPermissions);
            var jwtId = Guid.NewGuid().ToString();

            var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email ?? string.Empty, user.FullName, user.Role, permissions);
            var newRefreshToken = await GenerateRefreshTokenAsync(user.Id, jwtId, ipAddress);

            storedToken.ReplacedByTokenId = newRefreshToken.Id;

            await _context.SaveChangesAsync();

            return (newAccessToken, newRefreshToken.TokenHash); // TokenHash contains original token
        }

        public async Task RevokeTokenAsync(string refreshToken, string ipAddress, string reason = "Revoked by user")
        {
            var tokenHash = HashToken(refreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash)
                ?? throw new NotFoundException("Refresh token not found");

            if (!storedToken.IsActive)
            {
                return; // already revoked
            }

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokeByIP = ipAddress;

            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllUserTokensAsync(string userId, string ipAddress)
        {
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokeByIP = ipAddress;
            }

            await _context.SaveChangesAsync();
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetUserActiveTokensAsync(string userId)
        {
            return await _context.RefreshTokens
                .Where (rt => rt.UserId == userId && rt.IsActive)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        private async Task RevokeTokenChainAsync(RefreshToken token, string ipAddress, string reason)
        {
            var tokensToRevoke = await _context.RefreshTokens
                .Where(rt => rt.UserId == token.UserId && rt.IsActive)
                .ToListAsync();

            foreach (var t in tokensToRevoke)
            {
                t.IsRevoked = true;
                t.RevokedAt = DateTime.UtcNow;
                t.RevokeByIP = ipAddress;
            }

            await _context.SaveChangesAsync();
        }

        private string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
