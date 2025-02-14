using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Application.DTOs.Identity
{
    public class RoleDto
    {
        public Guid id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<Claim> Claims { get; set; }
    }
}
