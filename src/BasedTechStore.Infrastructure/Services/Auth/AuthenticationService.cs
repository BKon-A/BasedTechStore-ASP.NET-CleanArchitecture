using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Domain.Constants;
using BasedTechStore.Domain.Entities.Identity;
using BasedTechStore.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BasedTechStore.Infrastructure.Services.Auth
{
    public class AuthenticationService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IPermissionService _permissionService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthenticationService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            ITokenService tokenService, IPermissionService permissionService, 
            IRefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _permissionService = permissionService;
            _refreshTokenService = refreshTokenService;
        }

        public string? GetUserId(ClaimsPrincipal user)
        {
            if (!user.Identity?.IsAuthenticated ?? true)
                return null;

            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? user.FindFirst("sub")?.Value;
        }

        public async Task<(string accessToken, string refreshToken)> SignInAsync(SignInDto dto, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new UnauthorizedException("Invalid email or password");

            if (!user.IsActive)
                throw new ForbiddenException("User account is inactive. Please contact support!");

            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, isPersistent: false, lockoutOnFailure: true);

            if (!result.Succeeded)
                throw new UnauthorizedException("Invalid email or password");

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var permissions = _permissionService.GetAllPermissions(user.Role, user.CustomPermissions);
            var jwtId = Guid.NewGuid().ToString();

            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email ?? string.Empty, user.FullName, user.Role, permissions);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, jwtId, ipAddress);

            return (accessToken, refreshToken.TokenHash);
        }

        public async Task<(string accessToken, string refreshToken)> SignUpAsync(SignUpDto dto, string ipAddress)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
                throw new ConflictException("User with this email already exists.");

            var newUser = new AppUser
            {
                FullName = dto.FullName,
                Role = Roles.Customer,
                Email = dto.Email,
                UserName = dto.UserName ?? dto.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(newUser, dto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
                throw new ValidationException(errors);
            }

            var permissions = _permissionService.GetAllPermissions(newUser.Role, newUser.CustomPermissions);
            var jwtId = Guid.NewGuid().ToString();

            var accessToken = _tokenService.GenerateAccessToken(newUser.Id, newUser.Email ?? string.Empty, newUser.FullName, newUser.Role, permissions);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(newUser.Id, jwtId, ipAddress);

            return (accessToken, refreshToken.TokenHash);
        }

        public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            return await _refreshTokenService.RefreshTokenAsync(refreshToken, ipAddress);
        }

        public async Task SignOutAsync(string userId, string? refreshToken, string ipAddress)
        {
            await _signInManager.SignOutAsync();

            if (!string.IsNullOrEmpty(refreshToken))
                await _refreshTokenService.RevokeTokenAsync(refreshToken, ipAddress, "User signed out");
        }

        public async Task SignOutAllDevicesAsync(string userId, string ipAddress)
        {
            await _signInManager.SignOutAsync();
            await _refreshTokenService.RevokeAllUserTokensAsync(userId, ipAddress);
        }
    }
}
