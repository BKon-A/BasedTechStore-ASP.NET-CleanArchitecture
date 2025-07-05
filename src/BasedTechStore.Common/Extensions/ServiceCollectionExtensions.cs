using Microsoft.Extensions.Configuration;
using BasedTechStore.Common.Constants;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors;

namespace BasedTechStore.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommonCors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(AppConstants.CorsPolicy.AllowMultipleFrontends, policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:3000",    // React default
                            "http://localhost:5173",    // Vite default  
                            "http://localhost:4200",    // Angular default
                            "http://localhost:8080",    // Vue default
                            "http://localhost:5000",    // Local development HTTP
                            "http://localhost:5001",    // Local development HTTP
                            "https://localhost:5001",   // Local development HTTPS
                            "https://localhost:7000",   // Local development HTTPS
                            "https://localhost:7001"    // Local development HTTPS
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetIsOriginAllowedToAllowWildcardSubdomains();
                });

                // Separate policy for production
                options.AddPolicy(AppConstants.CorsPolicy.Production, policy =>
                {
                    policy.WithOrigins(
                        configuration.GetSection("AllowedOrigins").Value?.Split(',') ?? Array.Empty<string>()
                    )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return services;
        }
    }
}