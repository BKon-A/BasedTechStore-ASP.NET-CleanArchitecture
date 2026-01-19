using BasedTechStore.Application.Common.Interfaces.Persistence.Seed;
using BasedTechStore.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace BasedTechStore.Infrastructure.Persistence.Seed
{
    public class ManagerSeeder : IManagerSeeder
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppUserRole> _roleManager;

        public ManagerSeeder(UserManager<AppUser> userManager,
            RoleManager<AppUserRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedManagerAsync()
        {
            const string managerEmail = "manager@gmail.com";
            const string password = "Manager123!";
            const string roleName = "Manager";

            // Check if the role exists, if not create it
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new AppUserRole { Name = roleName });
            }

            // Check if the user exists
            var user = await _userManager.FindByEmailAsync(managerEmail);
            if (user is null)
            {
                var mainManager = new AppUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    FullName = "Main Manager",
                };

                var result = await _userManager.CreateAsync(mainManager, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(mainManager, roleName);
                }
                else
                {
                    throw new Exception($"Failed to create manager: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

        }
    }
}
