using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class EmployeeBenefitPlanRepos : BaseRepos<TbEmployeeBenefitPlan>, IEmployeeBenefitPlanRepos
{
    public EmployeeBenefitPlanRepos(HrDbContext context) : base(context)
    {
    }

    public async Task<List<TbEmployeeBenefitPlan>> GetByEmployeeIdAsync(Guid employeeId)
    {
        return await _dbSet
            .Where(x => x.EmployeeId == employeeId && !x.IsDelete)
            .ToListAsync();
    }
}