using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Common.ViewModels.Auth
{
    public class AuthStatusVM
    {
        public bool IsAuthenticated { get; set; }
        public string? UserName { get; set; }
        public List<ClaimsVM>? Claims { get; set; }
    }
}
