using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Domain.Constants;
using BasedTechStore.Domain.Entities.Identity;
using BasedTechStore.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace BasedTechStore.Infrastructure.Services.Auth
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPermissionService _permissionService;
        private readonly IMapper _mapper;

        public UserManagementService(UserManager<AppUser> userManager, IPermissionService permissionService, IMapper mapper)
        {
            _userManager = userManager;
            _permissionService = permissionService;
            _mapper = mapper;
        }

        public async Task<AppUserDto> GetByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException(nameof(AppUser), userId);

            var dto = _mapper.Map<AppUserDto>(user);
            dto.Permissions = _permissionService.GetAllPermissions(user.Role, user.CustomPermissions);

            return dto;
        }

        public async Task<AppUserDto> GetByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new NotFoundException(nameof(AppUser), email);
           
            var dto = _mapper.Map<AppUserDto>(user);
            dto.Permissions = _permissionService.GetAllPermissions(user.Role, user.CustomPermissions);

            return _mapper.Map<AppUserDto>(user);
        }

        public async Task<IEnumerable<AppUserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();

            var dtos = users.Select(user =>
            {
                var dto = _mapper.Map<AppUserDto>(user);
                dto.Permissions = _permissionService.GetAllPermissions(user.Role, user.CustomPermissions);
                return dto;
            });

            return dtos;
        }

        public async Task<AppUserDto> UpdateUserAsync(string userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException(nameof(AppUser), userId);

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                var emailExists = await _userManager.FindByEmailAsync(dto.Email);
                if (emailExists != null && emailExists.Id != userId)
                    throw new ConflictException($"Email {dto.Email} is already in use");
                
                user.Email = dto.Email;
                user.UserName = dto.Email;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
                throw new ValidationException(errors);
            }

            return await GetByIdAsync(userId);
        }

        /// <summary>
        /// Soft user delete
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException(nameof(AppUser), userId);

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
        }

        /// <summary>
        /// Hard user delete
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="ValidationException"></exception>
        public async Task DeleteUserPermanentAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException(nameof(AppUser), userId);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
                throw new ValidationException(errors);
            }
        }

        public async Task ChangeRoleAsync(string userId, string newRole)
        {
            if (!Roles.GetAll().Contains(newRole))
                throw new ValidationException("Role", $"Invalid role: {newRole}");

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException(nameof(AppUser), userId);

            user.Role = newRole;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
                throw new ValidationException(errors);
            }
        }

        public async Task<IReadOnlySet<string>> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException(nameof(AppUser), userId);

            return _permissionService.GetAllPermissions(user.Role, user.CustomPermissions);
        }

        public async Task GrantPermissionsAsync(string userId, IEnumerable<string> permissions)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException(nameof(AppUser), userId);

            var currentCustomPermissions = _permissionService.ParseCustomPermissions(user.CustomPermissions).ToList();
            var validNewPermissions = permissions
                .Where(p => _permissionService.IsValidPermission(p))
                .Where(p => !currentCustomPermissions.Contains(p))
                .ToList();

            if (!validNewPermissions.Any())
                throw new ValidationException("Permissions", "No valid new permissions to grant");

            currentCustomPermissions.AddRange(validNewPermissions);
            user.CustomPermissions = _permissionService.SerializePermissions(currentCustomPermissions);
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
                throw new ValidationException(errors);
            }
        }

        public async Task RevokePermissionsAsync(string userId, IEnumerable<string> permissions)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException(nameof(AppUser), userId);

            var currentCustomPermissions = _permissionService.ParseCustomPermissions(user.CustomPermissions).ToList();
            var permissionsToRevoke = permissions.ToHashSet();

            currentCustomPermissions.RemoveAll(p => permissionsToRevoke.Contains(p));

            user.CustomPermissions = _permissionService.SerializePermissions(currentCustomPermissions);
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
                throw new ValidationException(errors);
            }
        }

        public async Task<bool> ToogleUserStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException(nameof(AppUser), userId);

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
                throw new ValidationException(errors);
            }

            return user.IsActive;
        }
    }
}
