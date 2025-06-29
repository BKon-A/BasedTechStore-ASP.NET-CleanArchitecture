using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Application.DTOs.Identity.Request;
using BasedTechStore.Application.DTOs.Identity.Response;
using BasedTechStore.Domain.Entities.Identity;
using System;
using System.Security.Claims;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface IAuthService
    {
        string GetUserId(ClaimsPrincipal user);
        Task<AuthenticationResponse> SignInAsync(SignInRequest signInRequest);
        Task<AuthenticationResponse> SignUpAsync(SignUpRequest signUpRequest);
        Task SignOutAsync();

        Task<string> GenerateJwtTokenAsync(AppUser user);
        Task<string> GetJwtExpirationMinutes();
        Task<AuthenticationResponse> RefreshJwtTokenAsync(string token);
    }
}
