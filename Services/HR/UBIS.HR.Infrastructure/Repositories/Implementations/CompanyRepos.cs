using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces; // สังเกต: โปรเจกต์นี้ (ตามแบบ Branch) วาง Implementation ใน namespace .Interfaces ด้วย เพื่อความสม่ำเสมอกับของเดิม

public class CompanyRepos : BaseRepos<TbCompany>, ICompanyRepos
{
    public CompanyRepos(HrDbContext context) : base(context)
    {
    }

    // Override GetAllAsync/FindAsync ให้ Filter !IsDelete เหมือน BranchRepos ทำไว้
    public override async Task<IEnumerable<TbCompany>> GetAllAsync()
    {
        return await _dbSet.Where(x => !x.IsDelete).AsNoTracking().ToListAsync();
    }

    public override async Task<TbCompany?> GetByIdAsync(Guid id)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
    }
}