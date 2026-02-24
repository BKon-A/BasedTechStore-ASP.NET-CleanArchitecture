using BasedTechStore.Application.DTOs.Identity;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface IUserManagementService
    {
        Task<AppUserDto> GetByIdAsync(string userId);
        Task<AppUserDto> GetByEmailAsync(string email);
        Task<IEnumerable<AppUserDto>> GetAllUsersAsync();

        Task<AppUserDto> UpdateUserAsync(string userId, UpdateUserDto dto);
        Task DeleteUserAsync(string userId);
        Task ChangeRoleAsync(string userId, string newRole);

        Task<IReadOnlySet<string>> GetUserPermissionsAsync(string userId);
        Task GrantPermissionsAsync(string userId, IEnumerable<string> permissions);
        Task RevokePermissionsAsync(string userId, IEnumerable<string> permissions);
        Task<bool> ToogleUserStatusAsync(string userId);
    }
}
