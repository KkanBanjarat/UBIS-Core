using Microsoft.EntityFrameworkCore;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Data;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Repositories.Implementations;

public class RolePermissionRepos : BaseRepos<TbRolePermission>, IRolePermissionRepos
{
    public RolePermissionRepos(AccessDbContext context) : base(context)
    {
    }

    public async Task<bool> IsPermissionAssignedAsync(Guid roleId, Guid permissionId)
    {
        return await _dbSet.AnyAsync(x => 
            x.RoleId == roleId && 
            x.PermissionId == permissionId && 
            !x.IsDelete);
    }

    public async Task<IEnumerable<TbRolePermission>> GetByRoleIdWithDetailsAsync(Guid roleId)
    {
        return await _dbSet
            .Where(x => x.RoleId == roleId && !x.IsDelete)
            .Include(x => x.Role)
            .Include(x => x.Permission)
            .AsNoTracking()
            .ToListAsync();
    }
}