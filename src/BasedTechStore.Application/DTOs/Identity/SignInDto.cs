using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Application.DTOs.Identity
{
    public sealed record SignInDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
