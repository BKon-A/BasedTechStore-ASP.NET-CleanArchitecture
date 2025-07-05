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
using BasedTechStore.WebЬМVC.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<JwtSecurityTokenHandler>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
     {
         options.LoginPath = "/";
         options.AccessDeniedPath = "/";
         options.ExpireTimeSpan = TimeSpan.FromHours(24);
         options.SlidingExpiration = true;
         options.Events.OnRedirectToLogin = context =>
         {
             if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
             {
                 context.Response.StatusCode = 401;
                 return Task.CompletedTask;
             }
             context.Response.Redirect(context.RedirectUri);
             return Task.CompletedTask;
         };
     });

builder.Services.AddAuthorization();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".BasedTechStore.Session";
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddLogging();

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly, typeof(ViewModelMappingProfile).Assembly);

builder.Services.AddIdentity<AppUser, AppUserRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISpecificationService, SpecificationService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IManagerSeeder, ManagerSeeder>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

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

app.UseMiddleware<JwtCookieMiddleware>();

app.UseRouting();

app.UseCors("AllowReact");

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();

