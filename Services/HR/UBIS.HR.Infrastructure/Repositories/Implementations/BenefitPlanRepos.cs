using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class BenefitPlanRepos : BaseRepos<TbBenefitPlan>, IBenefitPlanRepos
{
    public BenefitPlanRepos(HrDbContext context) : base(context)
    {
    }
    public override async Task<TbBenefitPlan?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
    }

    public override async Task<IEnumerable<TbBenefitPlan>> GetAllAsync()
    {
        return await _dbSet
            .Where(x => !x.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }
    public override async Task<IEnumerable<TbBenefitPlan>> FindAsync(
        System.Linq.Expressions.Expression<Func<TbBenefitPlan, bool>> predicate)
    {
        return await _dbSet
            .Where(predicate)
            .Where(x => !x.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }

    public override async Task<bool> AnyAsync(
        System.Linq.Expressions.Expression<Func<TbBenefitPlan, bool>> predicate)
    {
        return await _dbSet
            .Where(x => !x.IsDelete)
            .AnyAsync(predicate);
    }
}