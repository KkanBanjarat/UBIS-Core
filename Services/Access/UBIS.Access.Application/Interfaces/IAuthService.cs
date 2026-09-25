using UBIS.Access.Application.Dtos;

namespace UBIS.Access.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto data);
    Task SyncUsersAsync(List<UserDto> users);
    Task<LoginResponseDto> SyncCurrentUserAsync(string entraObjectId, string email, string displayName);
    Task<UserLookupDto?> GetByEmailAsync(string email);
    Task<UserLookupDto?> LookupUserAsync(Guid? employeeId, string? employeeCode, string? email);
}