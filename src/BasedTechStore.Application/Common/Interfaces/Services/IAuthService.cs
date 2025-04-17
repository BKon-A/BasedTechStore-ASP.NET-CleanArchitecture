using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Application.DTOs.Identity.Request;
using BasedTechStore.Application.DTOs.Identity.Response;
using BasedTechStore.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthenticationResponse> SignInAsync(SignInRequest signInRequest);
        Task<AuthenticationResponse> SignUpAsync(SignUpRequest signUpRequest);
        Task SignOutAsync();

        Task<string> GenerateJwtTokenAsync(AppUser user);
        Task<string> GetJwtExpirationMinutes();
        Task<AuthenticationResponse> RefreshJwtTokenAsync(string token);
    }
}
