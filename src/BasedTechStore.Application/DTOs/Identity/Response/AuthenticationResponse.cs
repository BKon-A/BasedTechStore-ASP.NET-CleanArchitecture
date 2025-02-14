using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Application.DTOs.Identity.Response
{
    public class AuthenticationResponse
    {
        public bool IsSuccess { get; set; }
        public string Token { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public static AuthenticationResponse CreateSuccess(string token) => 
            new AuthenticationResponse { IsSuccess = true, Token = token };

        public static AuthenticationResponse CreateFailure(IEnumerable<string> errors) => 
            new AuthenticationResponse { IsSuccess = false, Errors = errors.ToList() };
    }
}
