using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Application.DTOs.Identity
{
    public sealed record ChangeUserRoleDto
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
