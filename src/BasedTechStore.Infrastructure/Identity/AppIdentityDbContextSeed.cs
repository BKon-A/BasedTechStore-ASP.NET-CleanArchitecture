using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BasedTechStore.Infrastructure.Identity
{
    public class AppIdentityDbContextSeed
    {
        public static async Task SeedAsync(AppIdentityDbContext identityDbContext, UserManager<AppUser> userManager, RoleManager<AppUserRole> roleManager)
        {
            if (identityDbContext.Database.IsSqlServer())
            {
                identityDbContext.Database.Migrate();
            }

            await roleManager.CreateAsync(new AppUserRole { Name = "Admin" });

            var defaultUser = new AppUser { UserName = "demoUser", Email = "demoUser@demo.com" };
            await userManager.CreateAsync(defaultUser, "123Ac!");

            var adminUser = new AppUser { UserName = "demoAdmin", Email = "demoAdmin@demo.com" };
            await userManager.CreateAsync(adminUser, "321Ac!");

            adminUser = await userManager.FindByNameAsync("demoAdmin");

            if (adminUser != null)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
