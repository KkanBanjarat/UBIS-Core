using UBIS.Access.Application.Dtos;

namespace UBIS.Access.Application.Interfaces;

public interface IRoleMenuService
{
    /// null = ไม่พบ Role
    Task<List<RoleMenuItemDto>?> GetAsync(Guid roleId);
    /// false = ไม่พบ Role
    Task<bool> UpdateAsync(Guid roleId, UpdateRoleMenusDto data);
}