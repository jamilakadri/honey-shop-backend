using MielShop.API.DTOs.User;

namespace MielShop.API.Services
{
    public interface IUserService
    {
        Task<PaginatedUsersDto> GetUsersAsync(int pageNumber, int pageSize, string? searchTerm, string? role);
        Task<UserDetailDto?> GetUserByIdAsync(int userId);
        Task<UserOperationResult> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserOperationResult> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
        Task<UserOperationResult> DeleteUserAsync(int userId);
        Task<UserOperationResult> ChangeUserRoleAsync(int userId, string newRole);
        Task<UserOperationResult> ToggleUserStatusAsync(int userId, bool isActive);
        Task<UserOperationResult> ResetPasswordAsync(int userId, string newPassword);
        Task<UserStatsDto> GetUserStatsAsync();
    }
}