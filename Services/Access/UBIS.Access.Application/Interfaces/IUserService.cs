using UBIS.Access.Application.Dtos;

public interface IUserService
{
    Task<List<UserListDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<PagedResultDto<UserListDto>> GetAllAsync(UserFilterDto filter);
    Task<UserDto?> CreateAsync(CreateUserDto data);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto data);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ResetPasswordAsync(Guid id, string newPassword);
    Task<EntraSyncResultDto> SyncUsersAsync(List<UserDto> users);
}