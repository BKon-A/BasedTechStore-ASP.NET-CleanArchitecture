using BasedTechStore.Application.DTOs.Identity.Request;

namespace BasedTechStore.Application.DTOs.Identity
{
    public class AuthModalViewModel
    {
        public SignInRequest SignInRequest { get; set; } = new ();
        public SignUpRequest SignUpRequest { get; set; } = new ();
    }
}
