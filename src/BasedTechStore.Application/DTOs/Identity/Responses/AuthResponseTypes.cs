namespace BasedTechStore.Application.DTOs.Identity.Responses
{
    public sealed record AuthTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
    }
    public sealed record AuthStatusResponse
    {
        public bool IsAuthenticated { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
    public sealed record CurrentUserResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
