using BasedTechStore.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BasedTechStore.Infrastructure.Services.Auth
{
    public class TokenService : ITokenService
    {
        private const string PermissionClaimType = "permissions";
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _signingKey;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured");
            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        }

        /// <summary>
        /// Generates a JWT access token with user claims and permissions. Permissions are serialized into a single claim to aboid hitting claim count limits in JWTs. 
        /// The token includes standard claims 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="email"></param>
        /// <param name="fullName"></param>
        /// <param name="role"></param>
        /// <param name="perms"></param>
        /// <returns></returns>
        public string GenerateAccessToken(string userId, string email, string fullName, string role, IEnumerable<string> perms)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var permissionsList = perms.ToList();
            if (!perms.Any())
            {
                claims.Add(new Claim(PermissionClaimType, JsonSerializer.Serialize(permissionsList)));
            }

            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60")),
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issur"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = _signingKey,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                return null;
            }
        }

        public IReadOnlySet<string> GetPermissionsFromClaims(ClaimsPrincipal principal)
        {
            var permsClaim = principal.FindFirst(PermissionClaimType)?.Value;

            if (string.IsNullOrEmpty(permsClaim))
                return new HashSet<string>();

            try
            {
                var perms = JsonSerializer.Deserialize<List<string>>(permsClaim);
                return perms?.ToHashSet() ?? new HashSet<string>();
            }
            catch
            {
                return new HashSet<string>();
            }
        }

        public string? GetUserIdFromClaims(ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string? GetRoleFromClaims(ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
