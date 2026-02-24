using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Domain.Entities.Identity
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }
        /// <summary>
        /// Actual hashed refresh token value
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string TokenHash { get; set; } = string.Empty;
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string JwtId { get; set; } = string.Empty;
        /// <summary>
        /// IP address where token created
        /// </summary>
        [MaxLength(100)]
        public string? CreatedByIP { get; set; }
        /// <summary>
        /// IP address where token was revoked(if applicable)
        /// </summary>
        [MaxLength(100)]
        public string? RevokeByIP { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }

        public Guid? ReplacedByTokenId { get; set; }
        public AppUser User { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsUsed && !IsExpired;
    }
}
