using System.Security.Claims;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(string userId, string email, string fullName, string role, IEnumerable<string> perms);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);

        IReadOnlySet<string> GetPermissionsFromClaims(ClaimsPrincipal principal);

        string? GetUserIdFromClaims(ClaimsPrincipal principal);
        string? GetRoleFromClaims(ClaimsPrincipal principal);
    }
}
