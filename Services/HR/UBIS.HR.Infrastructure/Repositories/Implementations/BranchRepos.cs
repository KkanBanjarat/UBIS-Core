using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class BranchRepos : BaseRepos<TbBranch>, IBranchRepos
{
    public BranchRepos(HrDbContext context) : base(context)
    {
    }

    public override async Task<TbBranch?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(x => x.Company)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
    }

    public override async Task<IEnumerable<TbBranch>> GetAllAsync()
    {
        return await _dbSet
            .Include(x => x.Company)
            .Where(x => !x.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<TbBranch>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _dbSet
            .Include(x => x.Company)
            .Where(b => b.CompanyId == companyId && !b.IsDelete && b.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public override async Task<IEnumerable<TbBranch>> FindAsync(
        Expression<Func<TbBranch, bool>> predicate)
    {
        return await _dbSet
            .Include(x => x.Company)
            .Where(predicate)
            .Where(x => !x.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }

    public override async Task<bool> AnyAsync(
        Expression<Func<TbBranch, bool>> predicate)
    {
        return await _dbSet
            .Where(x => !x.IsDelete)
            .AnyAsync(predicate);
    }
}