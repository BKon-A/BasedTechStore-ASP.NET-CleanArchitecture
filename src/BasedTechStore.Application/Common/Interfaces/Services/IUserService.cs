using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Application.DTOs.Identity.Response;
using BasedTechStore.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface IUserService
    {
        Task<AppUserDto> FindByIdAsync(Guid id);
        Task<AppUserDto> FindByEmailAsync(string email);
        Task<IEnumerable<AppUserDto>> GetAllUsersAsync();

        Task<OperationResult> UpdateUserAsync(AppUserDto user);
        Task<OperationResult> DeleteUserAsync(Guid id);

        Task<OperationResult> CreateRoleAsync(string roleName);
        Task<OperationResult> AssignUserToRolesAsync(AppUserDto user, IEnumerable<string> roles);
    }
}
