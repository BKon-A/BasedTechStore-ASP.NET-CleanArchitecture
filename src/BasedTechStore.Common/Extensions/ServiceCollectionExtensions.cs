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
                        // http production
                        "http://basedtech-store.com:3000",
                        "http://192.168.0.108:3000",
                        "http://localhost:3000",
                        // http localhost
                        "http://localhost:80",      // Nginx HTTP
                        "http://localhost:3000",    // React default
                        "http://localhost:4200",    // Angular default
                        "http://localhost:5000",    // Local development HTTP
                        "http://localhost:5001",    // Local development HTTP
                        "http://localhost:5173",    // Vite default  
                        "http://localhost:8080",    // Vue default
                                                    // https localhost
                        "https://localhost:5001",   // Local development HTTPS
                        "https://localhost:5173",   // Local development HTTPS
                        "https://localhost:7000",   // Local development HTTPS
                        "https://localhost:7001",   // Local development HTTPS
                        "https://localhost:7250"    // WebApi HTTPS
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();

                    //policy.SetIsOriginAllowed(_ => true)
                    //    .AllowAnyHeader()
                    //    .AllowAnyMethod()
                    //    .AllowCredentials();
                });

                // Separate policy for production
                options.AddPolicy(AppConstants.CorsPolicy.Production, policy =>
                {
                    var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            return services;
        }
    }
}