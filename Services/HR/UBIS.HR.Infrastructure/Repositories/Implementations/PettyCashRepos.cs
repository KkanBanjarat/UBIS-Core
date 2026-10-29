using Microsoft.EntityFrameworkCore;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class PettyCashRepos : BaseRepos<TbPettyCashRequest>, IPettyCashRepos
{
    public PettyCashRepos(HrDbContext context) : base(context)
    {
    }

    private IQueryable<TbPettyCashRequest> IncludeAll()
    {
        return _dbSet
            .Include(x => x.Employee).ThenInclude(e => e.Position)
            .Include(x => x.Employee).ThenInclude(e => e.PositionLevel)
            .Include(x => x.Employee).ThenInclude(e => e.Company)
            .Include(x => x.TbPettyCashLines.Where(l => !l.IsDelete))
                .ThenInclude(l => l.Benefit);
    }
    public async Task<TbPettyCashRequest?> GetByDocNumAsync(string docNum)
    {
        return await IncludeAll()
            .FirstOrDefaultAsync(x => x.DocNum == docNum && !x.IsDelete);
    }
    public async Task<List<TbPettyCashRequest>> GetSummariesByDocNumsAsync(
    IEnumerable<string> docNums, MyApprovalFilterDto? filter = null)
    {
        var list = docNums.Distinct().ToList();
        var query = _dbSet
            .AsNoTracking()
            .Include(x => x.Employee)
            .Where(x => list.Contains(x.DocNum) && !x.IsDelete);

        if (filter != null)
        {
            if (filter.DateFrom.HasValue)
            {
                var from = filter.DateFrom.Value.Date;
                query = query.Where(x => x.DocDate >= from);
            }
            if (filter.DateTo.HasValue)
            {
                var toExclusive = filter.DateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.DocDate < toExclusive);
            }
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var pattern = $"%{filter.Search.Trim()}%";
                query = query.Where(x =>
                    EF.Functions.ILike(x.DocNum, pattern)
                    || (x.Remark != null && EF.Functions.ILike(x.Remark, pattern))
                    || EF.Functions.ILike(x.Employee.FnameTh, pattern)
                    || EF.Functions.ILike(x.Employee.LnameTh, pattern)
                    || EF.Functions.ILike(x.Employee.EmpId, pattern));
            }
        }

        return await query.ToListAsync();
    }
    public async Task<TbPettyCashRequest?> GetDetailByIdAsync(Guid id)
    {
        return await IncludeAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
    }

    public async Task<(IEnumerable<PettyCashDto> Items, int TotalCount)> GetFilteredPagedAsync(PettyCashFilterDto filter, List<Guid> adminBranchIds)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(x => !x.IsDelete);

        // เห็นเฉพาะ: เอกสารของตัวเอง (ผู้ขอ) หรือ เอกสารที่ตัวเองสร้าง หรือ (HR Admin) เอกสารของพนักงานในสาขาที่ดูแล
        query = query.Where(x =>
            x.EmployeeId == filter.currentEmployeeId ||
            x.CreatedBy == filter.currentUserEmail ||
            adminBranchIds.Contains(x.Employee.BranchId));

        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(x => x.DocNum.Contains(filter.Search)
             || x.TbPettyCashLines.Any(l => l.Detail.Contains(filter.Search))
             || x.Employee.LnameTh.Contains(filter.Search)
             || x.Employee.FnameTh.Contains(filter.Search)
             || x.Employee.LnameEn.Contains(filter.Search)
             || x.Employee.FnameEn.Contains(filter.Search)
             || x.Employee.EmpId.Contains(filter.Search)
            );

        if (!string.IsNullOrEmpty(filter.DocStatus))
            query = query.Where(x => x.DocStatus == filter.DocStatus);


        var totalCount = await query.CountAsync();

        var page = Math.Max(1, filter.Page);
        var size = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(x => new PettyCashDto
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
                LineCount = x.TbPettyCashLines.Count(l => !l.IsDelete),
                HasBenefitLine = x.TbPettyCashLines.Any(l => !l.IsDelete && l.BenefitId != null),
                HasCashLine = x.TbPettyCashLines.Any(l => !l.IsDelete && l.BenefitId == null),
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt,
            })
            .ToListAsync();
        return (items, totalCount);
    }
    public async Task DeleteLinesAsync(Guid pettyCashId)
    {
        var lines = await _context.TbPettyCashLines
            .Where(l => l.PettyCashId == pettyCashId)
            .ToListAsync();

        _context.TbPettyCashLines.RemoveRange(lines);
        await _context.SaveChangesAsync();
    }
}