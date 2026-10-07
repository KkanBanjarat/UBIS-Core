using Microsoft.EntityFrameworkCore;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class BenefitClaimRepos : BaseRepos<TbBenefitClaim>, IBenefitClaimRepos
{
    public BenefitClaimRepos(HrDbContext context) : base(context)
    {
    }

    private IQueryable<TbBenefitClaim> IncludeAll()
    {
        return _dbSet
            .Include(x => x.Employee).ThenInclude(e => e.Position)
            .Include(x => x.Employee).ThenInclude(e => e.PositionLevel)
            .Include(x => x.TbBenefitClaimLines.Where(l => !l.IsDelete))
                .ThenInclude(l => l.Benefit);
    }

    public async Task<TbBenefitClaim?> GetByDocNumAsync(string docNum)
    {
        return await IncludeAll()
            .FirstOrDefaultAsync(x => x.DocNum == docNum && !x.IsDelete);
    }

    public async Task<List<TbBenefitClaim>> GetSummariesByDocNumsAsync(IEnumerable<string> docNums)
    {
        var list = docNums.Distinct().ToList();
        return await _dbSet
            .AsNoTracking()
            .Include(x => x.Employee)
            .Where(x => list.Contains(x.DocNum) && !x.IsDelete)
            .ToListAsync();
    }

    public async Task<TbBenefitClaim?> GetDetailByIdAsync(Guid id)
    {
        return await IncludeAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
    }

    public async Task<(IEnumerable<BenefitClaimDto> Items, int TotalCount)> GetFilteredPagedAsync(
        BenefitClaimFilterDto filter, List<Guid> adminBranchIds)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(x => !x.IsDelete);

        // เห็นเฉพาะ: ของตัวเอง / ที่ตัวเองสร้าง / (HR Admin) พนักงานในสาขาที่ดูแล
        query = query.Where(x =>
            x.EmployeeId == filter.currentEmployeeId ||
            x.CreatedBy == filter.currentUserEmail ||
            adminBranchIds.Contains(x.Employee.BranchId));

        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(x => x.DocNum.Contains(filter.Search)
                || x.TbBenefitClaimLines.Any(l => l.Detail.Contains(filter.Search))
                || x.Employee.LnameTh.Contains(filter.Search)
                || x.Employee.FnameTh.Contains(filter.Search)
                || x.Employee.LnameEn.Contains(filter.Search)
                || x.Employee.FnameEn.Contains(filter.Search)
                || x.Employee.EmpId.Contains(filter.Search));

        if (!string.IsNullOrEmpty(filter.DocStatus))
            query = query.Where(x => x.DocStatus == filter.DocStatus);

        var totalCount = await query.CountAsync();

        var page = Math.Max(1, filter.Page);
        var size = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(x => new BenefitClaimDto
            {
                Id = x.Id,
                DocNum = x.DocNum,
                DocStatus = x.DocStatus,
                DocDate = x.DocDate.Date,
                EmployeeId = x.EmployeeId,
                EmployeeNameTh = x.Employee.FnameTh + " " + x.Employee.LnameTh,
                PositionEn = x.Employee.Position.NameEn,
                PositionTh = x.Employee.Position.NameTh,
                PositionLevel = $"L{x.Employee.PositionLevel.Level}",
                PositionLevelNameEn = x.Employee.PositionLevel.NameEn,
                PositionLevelNameTh = x.Employee.PositionLevel.NameTh,
                Remark = x.Remark,
                TotalAmount = x.TotalAmount,
                LineCount = x.TbBenefitClaimLines.Count(l => !l.IsDelete),
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt,
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task DeleteLinesAsync(Guid benefitClaimId)
    {
        var lines = await _context.TbBenefitClaimLines
            .Where(l => l.BenefitClaimId == benefitClaimId)
            .ToListAsync();

        _context.TbBenefitClaimLines.RemoveRange(lines);
        await _context.SaveChangesAsync();
    }
}