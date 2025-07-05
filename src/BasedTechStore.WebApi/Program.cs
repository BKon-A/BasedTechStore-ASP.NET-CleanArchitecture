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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower;
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

// Add Authentication using Common extensions
builder.Services.AddAuthorization();

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
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Add JWT Token Handler
builder.Services.AddSingleton<JwtSecurityTokenHandler>();

// Add Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISpecificationService, SpecificationService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IManagerSeeder, ManagerSeeder>();

// Add Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

// Add Caching
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// Add Logging
builder.Services.AddLogging();

// Build the app
var app = builder.Build();

// Database seeding in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<IManagerSeeder>();
    await seeder.SeedManagerAsync();
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

app.UseHttpsRedirection();

app.UseMiddleware<JwtCookieMiddleware>();

// Use CORS - must be before authentication
var corsPolicy = app.Environment.IsDevelopment() 
    ? AppConstants.CorsPolicy.AllowMultipleFrontends 
    : AppConstants.CorsPolicy.Production;
app.UseCors(corsPolicy);

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