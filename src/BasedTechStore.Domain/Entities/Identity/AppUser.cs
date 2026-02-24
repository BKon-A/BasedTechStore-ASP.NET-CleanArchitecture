using BasedTechStore.Domain.Constants;
using BasedTechStore.Domain.Entities.Orders;

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Domain.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Primary role
        /// </summary>
        public string Role { get; set; } = Roles.Customer;

        /// <summary>
        /// Additional custom permissions.
        /// Stored as comma-separated enum values ("44,22,11")
        /// </summary>
        [MaxLength(2000)]
        public string? CustomPermissions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<RefreshToken> RefreshTokens { get;set; } = new List<RefreshToken>();
    }
}
