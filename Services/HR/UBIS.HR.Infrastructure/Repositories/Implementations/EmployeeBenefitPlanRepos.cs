using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;
using UBIS.HR.Application.Dtos;

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
    public async Task<List<AvailableBenefitDto>> GetAvailableBenefitsAsync(Guid employeeId, int docFormId)
    {
        var rows = await _dbSet
            .AsNoTracking()
            .Where(x => x.EmployeeId == employeeId
                        && !x.IsDelete && x.IsActive
                        && !x.BenefitPlan.IsDelete && x.BenefitPlan.IsActive)
            .SelectMany(x => x.BenefitPlan.TbBenefitPlanItems)
            .Where(i => !i.IsDelete && i.IsActive
                        && !i.Benefit.IsDelete && i.Benefit.IsActive
                        && i.Benefit.DocFormId == docFormId)
            .Select(i => new AvailableBenefitDto
            {
                BenefitId = i.BenefitId,
                NameTh = i.Benefit.NameTh,
                NameEn = i.Benefit.NameEn,
                LimitAmount = i.LimitAmount,
                CalcType = i.Benefit.CalcType,
                Region = i.Benefit.Region,
            })
            .ToListAsync();

        // พนักงานอยู่หลายแผนที่มีสวัสดิการซ้ำกัน → เอาวงเงินสูงสุด
        return rows
            .GroupBy(r => r.BenefitId)
            .Select(g => g.OrderByDescending(r => r.LimitAmount).First())
            .OrderBy(r => r.NameTh)
            .ToList();
    }
}