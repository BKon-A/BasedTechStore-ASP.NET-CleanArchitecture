using Microsoft.AspNetCore.Identity;

namespace BasedTechStore.Domain.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
