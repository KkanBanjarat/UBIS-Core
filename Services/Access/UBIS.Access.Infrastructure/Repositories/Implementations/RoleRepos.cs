using Microsoft.EntityFrameworkCore;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Data;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Repositories.Implementations;

public class RoleRepos : BaseRepos<TbRole>, IRoleRepos
{
    public RoleRepos(AccessDbContext context) : base(context){ }

    public async Task<TbRole?> GetByNameAsync(string name)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == name && !r.IsDelete);
    }


    public async Task<bool> IsNameExistsAsync(string name)
    {
        return await _dbSet.AnyAsync(r => r.Name == name && !r.IsDelete);
    }

    public async Task<IEnumerable<TbRole>> GetActiveRolesAsync()
    {
        return await _dbSet
            .Where(r => !r.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }
}