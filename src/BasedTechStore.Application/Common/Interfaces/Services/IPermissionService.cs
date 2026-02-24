namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface IPermissionService
    {
        IReadOnlySet<string> GetAllPermissions(string role, string? customPermissionsJson);
        bool HasPermission(string role, string? customPermissionsJson, string permission);
        bool HasAnyPermission(string role, string? customPermissionsJson, params string[] permissions);
        bool HasAllPermission(string role, string? customPermissionsJson, params string[] permissions);
        bool IsValidPermission(string permission);

        IEnumerable<string> ParseCustomPermissions(string? customPermissionsJson);
        string? SerializePermissions(IEnumerable<string> permissions);
    }
}
