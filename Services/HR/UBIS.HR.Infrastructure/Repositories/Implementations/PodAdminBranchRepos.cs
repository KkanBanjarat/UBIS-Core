using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class PodAdminBranchRepos : BaseRepos<TbPodAdminBranch>, IPodAdminBranchRepos
{
    public PodAdminBranchRepos(HrDbContext context) : base(context)
    {
    }

    public async Task<List<TbPodAdminBranch>> GetAllWithBranchAsync()
    {
        return await _dbSet
            .Where(w => !w.IsDelete)
            .Include(x => x.Branch)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<TbPodAdminBranch>> GetByUserIdWithBranchAsync(Guid userId)
    {
        return await _dbSet
            .Where(w => w.UserId == userId && !w.IsDelete)
            .Include(x => x.Branch)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<TbPodAdminBranch?> GetPrimaryByBranchIdAsync(Guid branchId)
    {
        return await _dbSet
            .Where(x => x.BranchId == branchId && x.IsPrimary && !x.IsDelete)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
    public async Task<List<TbPodAdminBranch>> GetByBranchIdAsync(Guid branchId)
    {
        return await _dbSet
            .Where(x => x.BranchId == branchId && !x.IsDelete)
            .ToListAsync();
    }
}