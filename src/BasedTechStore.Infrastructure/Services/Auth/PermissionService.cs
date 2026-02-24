using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BasedTechStore.Infrastructure.Services.Auth
{
    public class PermissionService : IPermissionService
    {
        private static readonly HashSet<string> _validPerms = Permissions.GetAll().ToHashSet();

        public IReadOnlySet<string> GetAllPermissions(string role, string? customPermissionsJson)
        {
            var rolePerms = RolePermissions.GetPermissionsForRole(role);
            var customPerms = ParseCustomPermissions(customPermissionsJson);

            if(!customPerms.Any())
                return rolePerms;

            return rolePerms.Union(customPerms).ToHashSet();
        }

        public bool HasPermission(string role, string? customPermJson, string perm)
        {
            return GetAllPermissions(role, customPermJson).Contains(perm);
        }

        public bool HasAnyPermission(string role, string? customPermJson, params string[] perm)
        {
            var userPerms = GetAllPermissions(role, customPermJson);
            return perm.Any(p => userPerms.Contains(p));
        }

        public bool HasAllPermission(string role, string? customPermJson, params string[] perm)
        {
            var userPerms = GetAllPermissions(role, customPermJson);
            return perm.All(p => userPerms.Contains(p));
        }

        public IEnumerable<string> ParseCustomPermissions(string? customPermissionsJson)
        {
            if(string.IsNullOrWhiteSpace(customPermissionsJson))
                return Enumerable.Empty<string>();

            try
            {
                var perm = JsonSerializer.Deserialize<List<string>>(customPermissionsJson);
                return perm?.Where(p => _validPerms.Contains(p)) ?? Enumerable.Empty<string>();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        public string? SerializePermissions(IEnumerable<string> perms)
        {
            var validPerms = perms.Where(p => _validPerms.Contains(p))
                .Distinct()
                .ToList();

            if (!validPerms.Any())
                return null;

            return JsonSerializer.Serialize(validPerms);
        }

        public bool IsValidPermission(string perm)
        {
            return _validPerms.Contains(perm);
        }
    }
}
