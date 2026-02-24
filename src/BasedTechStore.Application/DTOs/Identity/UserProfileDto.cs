namespace BasedTechStore.Application.DTOs.Identity
{
    public sealed record UserProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }
}
