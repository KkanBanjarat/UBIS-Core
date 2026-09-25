using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class BenefitRepos : BaseRepos<TbBenefit>, IBenefitRepos
{
    public BenefitRepos(HrDbContext context) : base(context)
    {
    }

    public override async Task<TbBenefit?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
    }

    public override async Task<IEnumerable<TbBenefit>> GetAllAsync()
    {
        return await _dbSet
            .Where(x => !x.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }

    public override async Task<IEnumerable<TbBenefit>> FindAsync(
        Expression<Func<TbBenefit, bool>> predicate)
    {
        return await _dbSet
            .Where(predicate)
            .Where(x => !x.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }

    public override async Task<bool> AnyAsync(
        Expression<Func<TbBenefit, bool>> predicate)
    {
        return await _dbSet
            .Where(x => !x.IsDelete)
            .AnyAsync(predicate);
    }
}