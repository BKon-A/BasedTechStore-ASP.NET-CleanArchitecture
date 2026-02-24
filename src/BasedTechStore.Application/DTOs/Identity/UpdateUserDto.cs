using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Application.DTOs.Identity
{
    public sealed record UpdateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [EmailAddress]
        [MaxLength(256)]
        public string? Email { get; set; }
        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
