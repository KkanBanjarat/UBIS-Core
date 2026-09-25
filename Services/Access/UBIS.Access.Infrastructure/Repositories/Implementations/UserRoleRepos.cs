using Microsoft.EntityFrameworkCore;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Data;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Repositories.Implementations;

public class UserRoleRepos : BaseRepos<TbUserRole>, IUserRoleRepos
{
    public UserRoleRepos(AccessDbContext context) : base(context)
    {
    }

    public async Task<bool> IsRoleAssignedAsync(Guid userId, Guid roleId)
    {
        return await _dbSet.AnyAsync(x => 
            x.UserId == userId && 
            x.RoleId == roleId && 
            !x.IsDelete);
    }

    public async Task<IEnumerable<TbUserRole>> GetByUserIdWithDetailsAsync(Guid userId)
    {
        return await _dbSet
            .Where(x => x.UserId == userId && !x.IsDelete)
            .Include(x => x.Role)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<TbUserRole>> GetEffectiveRolesAsync(Guid userId)
    {
        return await _dbSet
            .Include(ur => ur.Role)
                .ThenInclude(r => r.TbRolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId && !ur.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }
}