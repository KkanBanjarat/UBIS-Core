using UBIS.Access.Application.Dtos;
public interface IUserService
{
    Task<List<UserListDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> CreateAsync(CreateUserDto data);
    Task<UserDto?> UpdateAsync(Guid id, CreateUserDto data);
    Task<bool> DeleteAsync(Guid id);

    Task SyncUsersAsync(List<UserDto> users);
}

// public interface IEntraUserSyncService
// {
//     Task<EntraSyncResultDto> SyncAsync(List<UserDto> users);
// }