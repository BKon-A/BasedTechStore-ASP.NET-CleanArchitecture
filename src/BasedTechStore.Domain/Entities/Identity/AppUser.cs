using BasedTechStore.Domain.Entities.Orders;
using Microsoft.AspNetCore.Identity;

namespace BasedTechStore.Domain.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
