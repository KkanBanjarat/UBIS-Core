using UBIS.Access.Application.Dtos;
public interface IUserService
{
    Task<List<UserListDto>> GetAllAsync();
    Task<UserListDto?> CreateAsync(CreateLocalUserDto dto, string by);
    Task<UserListDto?> UpdateAsync(Guid id, UpdateUserDto dto, string by);
    Task<bool> DeleteAsync(Guid id, string by);
}

public interface IEntraUserSyncService
{
    Task<EntraSyncResultDto> SyncAsync(string by);
}