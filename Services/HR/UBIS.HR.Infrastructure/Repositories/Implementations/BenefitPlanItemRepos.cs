using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class BenefitPlanItemRepos : BaseRepos<TbBenefitPlanItem>, IBenefitPlanItemRepos
{
    public BenefitPlanItemRepos(HrDbContext context) : base(context)
    {
    }

    public override async Task<TbBenefitPlanItem?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(x => x.Benefit)
            .Include(x => x.BenefitPlan)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
    }

    public async Task<IEnumerable<TbBenefitPlanItem>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(x => x.Benefit)
            .Include(x => x.BenefitPlan)
            .Where(x => !x.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<IEnumerable<TbBenefitPlanItem>> GetByBenefitPlanIdAsync(Guid benefitPlanId)
    {
        return await _dbSet
            .Include(x => x.Benefit)
            .Include(x => x.BenefitPlan)
            .Where(x => x.BenefitPlanId == benefitPlanId && !x.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<TbBenefitPlanItem>> GetByBenefitIdAsync(Guid benefitId)
    {
        return await _dbSet
            .Include(x => x.Benefit)
            .Include(x => x.BenefitPlan)
            .Where(x => x.BenefitId == benefitId && !x.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> IsBenefitInPlanAsync(Guid benefitPlanId, Guid benefitId)
    {
        return await _dbSet
            .AnyAsync(x => x.BenefitPlanId == benefitPlanId && 
                          x.BenefitId == benefitId && 
                          !x.IsDelete);
    }
}