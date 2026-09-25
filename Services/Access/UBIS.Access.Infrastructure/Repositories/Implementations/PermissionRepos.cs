using Microsoft.EntityFrameworkCore;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Data;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Repositories.Implementations;

public class PermissionRepos : BaseRepos<TbPermission>, IPermissionRepos
{
    public PermissionRepos(AccessDbContext context) : base(context)
    {
    }

    public async Task<TbPermission?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == code && !p.IsDelete);
    }

    public async Task<bool> IsCodeExistsAsync(string code)
    {
        return await _dbSet.AnyAsync(p => p.Code == code && !p.IsDelete);
    }
}