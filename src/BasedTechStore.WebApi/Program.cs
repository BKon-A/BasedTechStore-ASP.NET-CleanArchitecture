using BasedTechStore.Application.Common.Interfaces.Persistence.Seed;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.Mapping;
using BasedTechStore.Domain.Entities.Identity;
using BasedTechStore.Infrastructure.Persistence;
using BasedTechStore.Infrastructure.Persistence.Seed;
using BasedTechStore.Infrastructure.Services.Carts;
using BasedTechStore.Infrastructure.Services.Products;
using BasedTechStore.Common.Extensions;
using BasedTechStore.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BasedTechStore.Common.Mapping;
using Microsoft.AspNetCore.DataProtection;
using BasedTechStore.Application.Common.Interfaces.Persistence;
using BasedTechStore.Infrastructure.Services.Auth;
using BasedTechStore.WebApi.Middlewares;
using BasedTechStore.Application.Common.Interfaces.Repositories;
using BasedTechStore.Infrastructure.Repositories;
using BasedTechStore.Infrastructure.Services.Orders;
using BasedTechStore.Application.Common.Interfaces.Authorization;
using BasedTechStore.Domain.Entities.Orders;
using BasedTechStore.Infrastructure.Policies;
using BasedTechStore.Infrastructure.Services.Categories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
    options.HttpsPort = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ? null : 7250;
});

builder.Services.AddDataProtection(option =>
{
   option.ApplicationDiscriminator = "BasedTechStore.WebApi";
})
    .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// =============== JWT Authentication ===============
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    context.Response.Headers.Append("Token-Expired", "true");

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    isSuccess = false,
                    message = "Not authorized access to resource",
                    statusCode = 401
                });
                
                return context.Response.WriteAsync(result);
            }
        };
    });

// Add Authentication using Common extensions
builder.Services.AddAuthorization();

// =============== Database ===============
var connectionString = builder.Configuration.GetConnectionString("SqlServerConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
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
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddApiEndpoints();

// Add AutoMapper
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile(typeof(MappingProfile));
    config.AddProfile(typeof(ViewModelMappingProfile));
});

// Add Application Services
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IAuthService, AuthenticationService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddScoped<IAuthorizationPolicy<Order>, OrderAuthorizationPolicy>();

builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IManagerSeeder, ManagerSeeder>();

// =============== Caching ===============
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// =============== Logging ===============
builder.Services.AddLogging();

// Build the app
var app = builder.Build();

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

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Identity API endpoints for authentication
//app.MapIdentityApi<AppUser>();

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