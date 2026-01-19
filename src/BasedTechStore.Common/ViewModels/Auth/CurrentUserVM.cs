using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Common.ViewModels.Auth
{
    public class CurrentUserVM
    {
        public string UserId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}
