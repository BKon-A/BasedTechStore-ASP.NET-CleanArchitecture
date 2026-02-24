namespace BasedTechStore.Application.DTOs.Identity.Requests
{
    public sealed record RefreshTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
