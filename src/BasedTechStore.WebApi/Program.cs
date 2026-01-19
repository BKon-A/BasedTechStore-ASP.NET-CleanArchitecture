using BasedTechStore.Application.Common.Interfaces.Persistence.Seed;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.Mapping;
using BasedTechStore.Domain.Entities.Identity;
using BasedTechStore.Infrastructure.Persistence;
using BasedTechStore.Infrastructure.Persistence.Seed;
using BasedTechStore.Infrastructure.Services.Carts;
using BasedTechStore.Infrastructure.Services.Identity;
using BasedTechStore.Infrastructure.Services.Products;
using BasedTechStore.Infrastructure.Services.Specifications;
using BasedTechStore.Common.Extensions;
using BasedTechStore.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using BasedTechStore.Common.Middlewares;
using BasedTechStore.Common.Mapping;
using Microsoft.AspNetCore.DataProtection;
using BasedTechStore.Application.Common.Interfaces.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new BasedTechStore.Common.Utilities.QueryModelBinderProvider());
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BasedTechStore API", Version = "v1" });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    
    c.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS using Common extensions
builder.Services.AddCommonCors(builder.Configuration);

builder.Services.AddHttpsRedirection(options =>
{
    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
    {
        options.HttpsPort = null; // Standard HTTPS port
    }
    else
    {
        options.HttpsPort = 7250; // Local development HTTPS port
    }
});

builder.Services.AddDataProtection(option =>
{
   option.ApplicationDiscriminator = "BasedTechStore.WebApi";
})
    .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// Add Authentication using Common extensions
builder.Services.AddAuthorization();

// Add Database
var connectionString = builder.Configuration.GetConnectionString("SqlServerConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        );

        sqlServerOptions.CommandTimeout(120);
        sqlServerOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Add Identity
builder.Services.AddIdentity<AppUser, AppUserRole>(options =>
{
    // Password settings
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;

    //// User settings
    //options.User.RequireUniqueEmail = true;

    //// Sign in settings
    //options.SignIn.RequireConfirmedEmail = false;
    //options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddApiEndpoints();

// Add AutoMapper
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile(typeof(MappingProfile));
    config.AddProfile(typeof(ViewModelMappingProfile));
});

// Add JWT Token Handler
builder.Services.AddSingleton<JwtSecurityTokenHandler>();

// Add Application Services
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISpecificationService, SpecificationService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IManagerSeeder, ManagerSeeder>();

// Add Caching
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// Add Logging
builder.Services.AddLogging();

// Build the app
var app = builder.Build();

var logger = app.Services.GetService<ILogger<Program>>();
var connectionStringLog = app.Configuration.GetConnectionString("SqlServerConnection");

logger?.LogInformation("🔍 Configuration Sources:");
foreach (var source in builder.Configuration.Sources)
{
    logger?.LogInformation("  - {Source}: {Type}", source, source.GetType().Name);
}
logger?.LogInformation("🔍 Connection String Source: {Source}",
    string.IsNullOrEmpty(connectionStringLog) ? "NOT FOUND" : "Found");

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await dbInitializer.InitializeDbAsync(app.Services, app.Environment);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BasedTechStore API V1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// HTTPS redirection - disable in containerized environments
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    app.UseHttpsRedirection();
}

// Use CORS - must be before authentication
var corsPolicy = app.Environment.IsDevelopment() 
    ? AppConstants.CorsPolicy.AllowMultipleFrontends 
    : AppConstants.CorsPolicy.Production;
app.UseCors(corsPolicy);

app.UseMiddleware<JwtCookieMiddleware>();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Identity API endpoints for authentication
app.MapIdentityApi<AppUser>();

// Map controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow })
    .WithName("HealthCheck")
    .WithOpenApi();

// API info endpoint
app.MapGet(ApiRoutes.ApiBase + "/info", () => new 
{ 
    Name = AppConstants.ApplicationName + " API",
    Version = AppConstants.ApiVersion,
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
})
.WithName("ApiInfo")
.WithOpenApi();

app.Run();