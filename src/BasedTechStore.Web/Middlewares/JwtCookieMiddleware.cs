using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace BasedTechStore.Web.Middlewares
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public JwtCookieMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue("access-token", out var token) && !string.IsNullOrEmpty(token))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtSettings = _configuration.GetSection("JwtSettings");
                    var secret = jwtSettings["SecretKey"] ?? throw new ArgumentNullException("JwtSettings:Secret configuration is missing");
                    var issuer = jwtSettings["Issuer"] ?? throw new ArgumentNullException("JwtSettings:Issuer configuration is missing");
                    var audience = jwtSettings["Audience"] ?? throw new ArgumentNullException("JwtSettings:Audience configuration is missing");

                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                        ClockSkew = TimeSpan.Zero // Disable clock skew for immediate validation
                    };

                    var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                    context.User = principal;
                }
                catch (Exception ex)
                {
                    // Invalid token, clear the cookie
                    context.Response.Cookies.Delete("access-token");
                    Console.WriteLine($"Error processing JWT token: {ex.Message}");
                }
            }

            await _next(context);
        }
    }
}
