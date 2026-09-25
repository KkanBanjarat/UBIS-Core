using Microsoft.EntityFrameworkCore;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class PrettyCashRepos : BaseRepos<TbPrettyCashRequest>, IPrettyCashRepos
{
    public PrettyCashRepos(HrDbContext context) : base(context)
    {
    }

    private IQueryable<TbPrettyCashRequest> IncludeAll()
    {
        return _dbSet
            .Include(x => x.Employee)
            .Include(x => x.TbPrettyCashLines.Where(l => !l.IsDelete))
                .ThenInclude(l => l.Benefit);
    }
    public async Task<TbPrettyCashRequest?> GetByDocNumAsync(string docNum)
    {
        return await IncludeAll()
            .FirstOrDefaultAsync(x => x.DocNum == docNum && !x.IsDelete);
    }
    public async Task<TbPrettyCashRequest?> GetDetailByIdAsync(Guid id)
    {
        return await IncludeAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
    }

    public async Task<(IEnumerable<TbPrettyCashRequest> Items, int TotalCount)> GetFilteredPagedAsync(
        PrettyCashFilterDto filter, Guid currentEmployeeId, string currentUserEmail, List<Guid> adminBranchIds)
    {
        var query = IncludeAll()
            .AsNoTracking()
            .Where(x => !x.IsDelete);

        // เห็นเฉพาะ: เอกสารของตัวเอง (ผู้ขอ) หรือ เอกสารที่ตัวเองสร้าง หรือ (HR Admin) เอกสารของพนักงานในสาขาที่ดูแล
        query = query.Where(x =>
            x.EmployeeId == currentEmployeeId ||
            x.CreatedBy == currentUserEmail ||
            adminBranchIds.Contains(x.Employee.BranchId));

        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(x => x.DocNum.Contains(filter.Search)
             || x.TbPrettyCashLines.Any(l => l.Detail.Contains(filter.Search))
             || x.Employee.LnameTh.Contains(filter.Search)
             || x.Employee.FnameTh.Contains(filter.Search)
             || x.Employee.LnameEn.Contains(filter.Search)
             || x.Employee.FnameEn.Contains(filter.Search)
             || x.Employee.EmpId.Contains(filter.Search)
            );

        if (!string.IsNullOrEmpty(filter.DocStatus))
            query = query.Where(x => x.DocStatus == filter.DocStatus);


        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    public async Task DeleteLinesAsync(Guid prettyCashId)
    {
        var lines = await _context.TbPrettyCashLines
            .Where(l => l.PrettyCashId == prettyCashId)
            .ToListAsync();

        _context.TbPrettyCashLines.RemoveRange(lines);
        await _context.SaveChangesAsync();
    }
}