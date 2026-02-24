using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Application.DTOs.Identity
{
    public sealed record ManagePermissionsDto
    {
        [Required]
        [MinLength(1)]
        public IReadOnlyCollection<string> Permissions { get; set; } = new List<string>();
    }
}
