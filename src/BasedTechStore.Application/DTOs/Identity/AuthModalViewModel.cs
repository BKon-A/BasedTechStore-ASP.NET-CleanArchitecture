namespace BasedTechStore.Application.DTOs.Identity
{
    public class AuthModalViewModel
    {
        public SignInDto SignInRequest { get; set; } = new ();
        public SignUpDto SignUpRequest { get; set; } = new ();
    }
}
