using UBIS.Access.Application.Dtos;

namespace UBIS.Access.Application.Interfaces;

public interface IUserRoleService
{
    Task<List<UserRoleDto>> GetAllAsync();
   Task<List<RoleScopeSummaryDto>> GetByUserIdAsync(Guid userId);
    Task<UserRoleDto> AssignAsync(CreateUserRoleDto data);
    Task<bool> RevokeAsync(Guid id);
    Task<List<EffectivePermissionDto>> GetEffectivePermissionsAsync(Guid userId);
}