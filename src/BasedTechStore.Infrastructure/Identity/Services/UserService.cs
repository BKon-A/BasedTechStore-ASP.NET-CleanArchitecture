using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Application.DTOs.Identity.Response;
using BasedTechStore.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BasedTechStore.Infrastructure.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppUserRole> _roleManager;
        private readonly IMapper _mapper;

        public UserService(UserManager<AppUser> userManager,
            RoleManager<AppUserRole> roleManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<AppUserDto> FindByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            return _mapper.Map<AppUserDto>(user);
        }

        public async Task<AppUserDto> FindByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return _mapper.Map<AppUserDto>(user);
        }

        public async Task<IEnumerable<AppUserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            return users.Select(u => _mapper.Map<AppUserDto>(u));
        }

        public async Task<OperationResult> UpdateUserAsync(AppUserDto userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id.ToString());
            if (user == null)
            {
                return OperationResult.CreateFailure(new[] { "User not found" });
            }

            user.FullName = userDto.FullName;
            user.Email = userDto.Email;
            user.PhoneNumber = userDto.PhoneNumber;
            user.UserName = userDto.UserName;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded
                ? OperationResult.CreateSuccess()
                : OperationResult.CreateFailure(result.Errors.Select(e => e.Description));
        }

        public async Task<OperationResult> DeleteUserAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return OperationResult.CreateFailure(new[] { "User not found" });
            }
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded
                ? OperationResult.CreateSuccess()
                : OperationResult.CreateFailure(result.Errors.Select(e => e.Description));
        }

        public async Task<OperationResult> CreateRoleAsync(string roleName)
        {
            var role = new AppUserRole { Name = roleName };
            var result = await _roleManager.CreateAsync(role);
            return result.Succeeded
                ? OperationResult.CreateSuccess()
                : OperationResult.CreateFailure(result.Errors.Select(e => e.Description));
        }

        public async Task<OperationResult> AssignUserToRolesAsync(AppUserDto userDto, IEnumerable<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id.ToString());
            if (user == null)
            {
                return OperationResult.CreateFailure(new[] { "User not found" });
            }

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    return OperationResult.CreateFailure(new[] { $"Role {role} not found" });
                }
            }

            var result = await _userManager.AddToRolesAsync(user, roles);
            return result.Succeeded
                ? OperationResult.CreateSuccess()
                : OperationResult.CreateFailure(result.Errors.Select(e => e.Description));
        }
    }
}
