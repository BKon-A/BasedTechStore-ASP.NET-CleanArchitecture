using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Identity.Request;
using BasedTechStore.Application.DTOs.Identity.Response;
using BasedTechStore.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using BasedTechStore.Application.DTOs.Identity;

namespace BasedTechStore.Infrastructure.Services.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<string> GenerateJwtTokenAsync(AppUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiresMinutes = Convert.ToDouble(jwtSettings["ExpirationMinutes"]);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("FullName", user.FullName)
            };

            var jwtToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        public async Task<AuthenticationResponse> RefreshJwtTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email 
                || c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(emailClaim))
            {
                return AuthenticationResponse.CreateFailure(new[] { "Invalid token" });
            }

            var user = await _userManager.FindByEmailAsync(emailClaim);
            if (user == null)
            {
                return AuthenticationResponse.CreateFailure(new[] { "User not found" });
            }

            var newToken = await GenerateJwtTokenAsync(user);
            return AuthenticationResponse.CreateSuccess(newToken);
        }

        public async Task<string> GetJwtExpirationMinutes()
        {
            return _configuration["JwtSettings:ExpirationMinutes"];
        }

        public async Task<AuthenticationResponse> SignInAsync(SignInRequest signInRequest)
        {
            var user = await _userManager.FindByEmailAsync(signInRequest.Email);
            if (user == null)
            {
                return AuthenticationResponse.CreateFailure(new[] { "User not found" });
            }

            var result = await _signInManager.PasswordSignInAsync(user, signInRequest.Password, false, false);
            if (!result.Succeeded)
            {
                return AuthenticationResponse.CreateFailure(new[] { "Invalid credentials" });
            }

            var jwtToken = await GenerateJwtTokenAsync(user);
            return AuthenticationResponse.CreateSuccess(jwtToken);
        }

        public async Task<AuthenticationResponse> SignUpAsync(SignUpRequest signUpRequest)
        {
            var existingUser = await _userManager.FindByEmailAsync(signUpRequest.Email);
            if (existingUser != null)
            {
                return AuthenticationResponse.CreateFailure(new[] { "User already exists" });
            }

            var newUser = _mapper.Map<AppUser>(signUpRequest);
            var result = await _userManager.CreateAsync(newUser, signUpRequest.Password);
            if (!result.Succeeded)
            {
                return AuthenticationResponse.CreateFailure(result.Errors.Select(e => e.Description));
            }

            var jwtToken = await GenerateJwtTokenAsync(newUser);
            return AuthenticationResponse.CreateSuccess(jwtToken);
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
