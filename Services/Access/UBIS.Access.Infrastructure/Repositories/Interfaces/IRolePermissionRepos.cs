using UBIS.Access.Domain.Entities;

namespace UBIS.Access.Infrastructure.Repositories.Interfaces;

public interface IRolePermissionRepos : IBaseRepos<TbRolePermission>
{
    Task<bool> IsPermissionAssignedAsync(Guid roleId, Guid permissionId);
    Task<IEnumerable<TbRolePermission>> GetByRoleIdWithDetailsAsync(Guid roleId);
}