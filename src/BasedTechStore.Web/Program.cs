using BasedTechStore.Application.Common.Interfaces.Persistence.Seed;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.Mapping;
using BasedTechStore.Domain.Entities.Identity;
using BasedTechStore.Infrastructure.Persistence;
using BasedTechStore.Infrastructure.Persistence.Seed;
using BasedTechStore.Infrastructure.Services.Identity;
using BasedTechStore.Infrastructure.Services.Products;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var issuer = jwtSettings["Issuer"] ?? throw new ArgumentNullException("JwtSettings:Issuer configuration is missing");
        var audience = jwtSettings["Audience"] ?? throw new ArgumentNullException("JwtSettings:Audience configuration is missing");
        var secret = jwtSettings["SecretKey"] ?? throw new ArgumentNullException("JwtSettings:Secret configuration is missing");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };

        options.MapInboundClaims = true;
    });
    //.AddCookie(IdentityConstants.ApplicationScheme, options =>
    //{
    //    options.LoginPath = "/Auth/SignIn";
    //    options.LogoutPath = "/Auth/SignOut";
    //});

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

//builder.Services.AddIdentityCore<AppUser>()
//   .AddRoles<AppUserRole>()
//   .AddEntityFrameworkStores<AppDbContext>()
//   .AddApiEndpoints();

builder.Services.AddIdentity<AppUser, AppUserRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IManagerSeeder, ManagerSeeder>();

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<IManagerSeeder>();
    await seeder.SeedManagerAsync();
}
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapIdentityApi<AppUser>();

app.UseStaticFiles();

app.Use(async (context, next) =>
{
    if (context.Request.Cookies.ContainsKey("access-token"))
    {
        var token = context.Request.Cookies["access-token"];
        context.Request.Headers["Authorization"] = $"Bearer {token}";
    }
    await next();
});

//app.Use(async (context, next) =>
//{
//    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
//    context.Response.Headers["Pragma"] = "no-cache";
//    context.Response.Headers["Expires"] = "0";
//    await next();
//});



app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();

