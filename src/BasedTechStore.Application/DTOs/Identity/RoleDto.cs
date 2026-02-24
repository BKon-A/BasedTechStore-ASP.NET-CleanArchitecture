using System.Security.Claims;

namespace BasedTechStore.Application.DTOs.Identity
{
    public sealed record RoleDto
    {
        public Guid id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<Claim> Claims { get; set; }
    }
}
