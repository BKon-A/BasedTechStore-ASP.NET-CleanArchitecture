using BasedTechStore.Application.DTOs.Identity;
using System.Security.Claims;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface IAuthService
    {
        string? GetUserId(ClaimsPrincipal user);
        Task<(string accessToken, string refreshToken)> SignInAsync(SignInDto dto, string ipAddress);
        Task<(string accessToken, string refreshToken)> SignUpAsync(SignUpDto dto, string ipAddress);
        Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task SignOutAsync(string userId, string? refreshToken, string ipAddress);
        Task SignOutAllDevicesAsync(string userId, string ipAddress);
    }
}
