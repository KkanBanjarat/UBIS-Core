using UBIS.Access.Application.Dtos;

namespace UBIS.Access.Application.Interfaces;

public interface IRolePermissionService
{
    Task<List<RolePermissionDto>> GetAllAsync();
    Task<RolePermissionDto> AssignAsync(CreateRolePermissionDto data);
    Task<bool> RevokeAsync(Guid id);
}