namespace BasedTechStore.Application.DTOs.Identity
{
    public sealed record UserPermissionsDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public IReadOnlyCollection<string> RolePermissions { get; set; } = new List<string>();
        public IReadOnlyCollection<string> CustomPermissions { get; set; } = new List<string>();
        public IReadOnlyCollection<string> AllPermissions { get; set; } = new List<string>();
    }
}
