using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Common.Constants;
using BasedTechStore.Common.Models.Api;
using BasedTechStore.Domain.Constants;
using BasedTechStore.WebApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.WebApi.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Users.Base)]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagmentService;
        private readonly IPermissionService _permissionService;

        public UsersController(IUserManagementService userManagmentService, IPermissionService permissionService)
        {
            _userManagmentService = userManagmentService;
            _permissionService = permissionService;
        }

        [HttpGet(ApiRoutes.Users.GetAll)]
        [RequirePermission(Permissions.UsersView)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppUserDto>>>> GetAllUsers()
        {
            var users = await _userManagmentService.GetAllUsersAsync();
            return Ok(ApiResponse<IEnumerable<AppUserDto>>.Success(users));
        }

        [HttpGet(ApiRoutes.Users.GetById)]
        [RequirePermission(Permissions.UsersView)]
        public async Task<ActionResult<ApiResponse<AppUserDto>>> GetUserById(string userId)
        {
            var user = await _userManagmentService.GetByIdAsync(userId);
            return Ok(ApiResponse<AppUserDto>.Success(user));
        }

        [HttpPut(ApiRoutes.Users.Update)]
        [RequirePermission(Permissions.UsersEdit)]
        public async Task<ActionResult<ApiResponse<AppUserDto>>> UpdateUser(string userId, [FromBody] UpdateUserDto dto)
        {
            var user = await _userManagmentService.UpdateUserAsync(userId, dto);
            return Ok(ApiResponse<AppUserDto>.Success(user, "User updated successfully"));
        }

        [HttpDelete(ApiRoutes.Users.Delete)]
        [RequirePermission(Permissions.UsersDelete)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(string userId)
        {
            await _userManagmentService.DeleteUserAsync(userId);
            return Ok(ApiResponse<object>.Success(null, "User deleted successfully"));
        }

        [HttpPut(ApiRoutes.Users.ChangeRole)]
        [RequirePermission(Permissions.UsersManageRoles)]
        public async Task<ActionResult<ApiResponse<object>>> ChangeUserRole(string userId, [FromBody] ChangeUserRoleDto dto)
        {
            await _userManagmentService.ChangeRoleAsync(userId, dto.Role);
            return Ok(ApiResponse<object>.Success(null, "User role changed successfully"));
        }

        [HttpGet(ApiRoutes.Users.GetPermissions)]
        [RequirePermission(Permissions.UsersView)]
        public async Task<ActionResult<ApiResponse<UserPermissionsDto>>> GetUserPermissions(string userId)
        {
            var user = await _userManagmentService.GetByIdAsync(userId);
            var allPermissions = await _userManagmentService.GetUserPermissionsAsync(userId);

            var rolePermissioons = RolePermissions.GetPermissionsForRole(user.Role);
            var customPermissions = _permissionService.ParseCustomPermissions(user.CustomPermissions);

            var dto = new UserPermissionsDto
            {
                UserId = user.Id,
                Role = user.Role,
                RolePermissions = rolePermissioons.ToList(),
                CustomPermissions = customPermissions.ToList(),
                AllPermissions = allPermissions.ToList()
            };

            return Ok(ApiResponse<UserPermissionsDto>.Success(dto));
        }

        [HttpPost(ApiRoutes.Users.GrantPermissions)]
        [RequirePermission(Permissions.UsersManageRoles)]
        public async Task<ActionResult<ApiResponse<object>>> GrantPermissions(string userId, [FromBody] ManagePermissionsDto dto)
        {
            await _userManagmentService.GrantPermissionsAsync(userId, dto.Permissions);
            return Ok(ApiResponse<object>.Success(null, "Permissions granted successfully"));
        }

        [HttpPost(ApiRoutes.Users.RevokePermissions)]
        [RequirePermission(Permissions.UsersManageRoles)]
        public async Task<ActionResult<ApiResponse<object>>> RevokePermissions(string userId, [FromBody] ManagePermissionsDto dto)
        {
            await _userManagmentService.RevokePermissionsAsync(userId, dto.Permissions);
            return Ok(ApiResponse<object>.Success(null, "Permissions revoked successfully"));
        }

        [HttpPost(ApiRoutes.Users.GrantPermissions)]
        [RequirePermission(Permissions.UsersManageRoles)]
        public async Task<ActionResult<ApiResponse<object>>> ToggleUserStatus(string userId)
        {
            var isActive = await _userManagmentService.ToogleUserStatusAsync(userId);
            var message = isActive ? "User activated" : "User deactivated";

            return Ok(ApiResponse<object>.Success(new { isActive }, message));
        }

        // TODO: Refactor or delete
        [HttpGet(ApiRoutes.Users.GetRoles)]
        [RequirePermission(Permissions.UsersView)]
        public ActionResult<ApiResponse<IEnumerable<string>>> GetAvailableRoles()
        {
            var roles = Roles.GetAll(); // domain constant in controller!!!
            return Ok(ApiResponse<IEnumerable<string>>.Success(roles));
        }

        // TODO: Refactor or delete
        [HttpGet(ApiRoutes.Users.GetAvailablePermissions)]
        [RequirePermission(Permissions.UsersView)]
        public ActionResult<ApiResponse<IEnumerable<string>>> GetAvailablePermissions()
        {
            var permissions = Permissions.GetAll(); // domain constant in controller!!!
            return Ok(ApiResponse<IEnumerable<string>>.Success(permissions));
        }
    }
}
