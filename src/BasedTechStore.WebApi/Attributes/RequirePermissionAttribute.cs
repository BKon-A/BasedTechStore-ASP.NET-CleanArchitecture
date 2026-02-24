using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using BasedTechStore.Application.Common.Interfaces.Services;

namespace BasedTechStore.WebApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _requiredPermissions;
        private readonly bool _requireAll;

        public RequirePermissionAttribute(string permission)
        {
            _requiredPermissions = new[] { permission };
            _requireAll = true;
        }

        public RequirePermissionAttribute(bool requireAll, params string[] perms)
        {
            _requiredPermissions = perms;
            _requireAll = requireAll;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.Any(m => m is AllowAnonymousAttribute))
                return;

            var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
            var user = context.HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var perms = tokenService.GetPermissionsFromClaims(user);

            bool hasAccess = _requireAll
                ? _requiredPermissions.All(p => perms.Contains(p))
                : _requiredPermissions.Any(p => perms.Contains(p));

            await Task.CompletedTask;
        }
    }
}
