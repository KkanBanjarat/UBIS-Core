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
            .Include(x => x.Employee).ThenInclude(e => e.Company)
            .Include(x => x.Employee).ThenInclude(e => e.Branch)
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

    public async Task<BenefitClaimSummaryDto> GetSummaryAsync(
    BenefitClaimSummaryFilterDto filter, List<Guid> adminBranchIds)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(x => !x.IsDelete && adminBranchIds.Contains(x.Employee.BranchId));

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

        if (filter.CompanyId.HasValue)
            query = query.Where(x => x.Employee.Company.Id == filter.CompanyId.Value);

        query = string.IsNullOrEmpty(filter.DocStatus)
            ? query.Where(x => x.DocStatus != "Draft")
            : query.Where(x => x.DocStatus == filter.DocStatus);

        var branchDocs = await query
            .GroupBy(x => new
            {
                Company = x.Employee.Company.NameTh,
                Branch = x.Employee.Branch.NameTh
            })
            .Select(g => new { g.Key.Company, g.Key.Branch, DocCount = g.Count() })
            .ToListAsync();

        var branchBenefits = await query
            .SelectMany(
                x => x.TbBenefitClaimLines.Where(l => !l.IsDelete),
                (x, l) => new
                {
                    Company = x.Employee.Company.NameTh,
                    Branch = x.Employee.Branch.NameTh,
                    ClaimId = x.Id,
                    Benefit = l.Benefit.NameTh,
                    l.Amount
                })
            .GroupBy(z => new { z.Company, z.Branch, z.Benefit })
            .Select(g => new
            {
                g.Key.Company,
                g.Key.Branch,
                g.Key.Benefit,
                DocCount = g.Select(z => z.ClaimId).Distinct().Count(),
                LineCount = g.Count(),
                Amount = g.Sum(z => z.Amount)
            })
            .ToListAsync();

        var byCompany = branchDocs
            .GroupBy(b => b.Company)
            .Select(cg =>
            {
                var branches = cg.OrderBy(b => b.Branch).Select(b =>
                {
                    var benefits = branchBenefits
                        .Where(x => x.Company == b.Company && x.Branch == b.Branch)
                        .OrderBy(x => x.Benefit)
                        .Select(x => new CompanyBenefitDto
                        {
                            Benefit = x.Benefit,
                            DocCount = x.DocCount,
                            LineCount = x.LineCount,
                            Amount = x.Amount
                        }).ToList();

                    return new ClaimBranchSummaryDto
                    {
                        Branch = string.IsNullOrWhiteSpace(b.Branch) ? "ไม่ระบุสาขา" : b.Branch,
                        DocCount = b.DocCount,
                        LineCount = benefits.Sum(x => x.LineCount),
                        Amount = benefits.Sum(x => x.Amount),
                        Benefits = benefits
                    };
                }).ToList();

                return new CompanySummaryDto
                {
                    Company = string.IsNullOrWhiteSpace(cg.Key) ? "ไม่ระบุบริษัท" : cg.Key,
                    DocCount = branches.Sum(x => x.DocCount),
                    LineCount = branches.Sum(x => x.LineCount),
                    Amount = branches.Sum(x => x.Amount),
                    Branches = branches
                };
            })
            .OrderBy(c => c.Company)
            .ToList();

        return new BenefitClaimSummaryDto { ByCompany = byCompany };
    }
    public async Task<List<BenefitClaimExportRowDto>> GetExportRowsAsync(
    BenefitClaimSummaryFilterDto filter, List<Guid> adminBranchIds, Guid? employeeId = null)
    {
        var q = _dbSet
            .AsNoTracking()
            .Where(x => !x.IsDelete && adminBranchIds.Contains(x.Employee.BranchId));

        if (filter.DateFrom.HasValue)
        {
            var from = filter.DateFrom.Value.Date;
            q = q.Where(x => x.DocDate >= from);
        }
        if (filter.DateTo.HasValue)
        {
            var toExclusive = filter.DateTo.Value.Date.AddDays(1);
            q = q.Where(x => x.DocDate < toExclusive);
        }
        if (employeeId.HasValue)
            q = q.Where(x => x.EmployeeId == employeeId.Value);

        q = string.IsNullOrEmpty(filter.DocStatus)
            ? q.Where(x => x.DocStatus != "Draft")
            : q.Where(x => x.DocStatus == filter.DocStatus);

        return await q
            .SelectMany(
                x => x.TbBenefitClaimLines.Where(l => !l.IsDelete),
                (x, l) => new BenefitClaimExportRowDto
                {
                    Company = x.Employee.Company.NameTh,
                    DocNum = x.DocNum,
                    DocDate = x.DocDate,
                    DocStatus = x.DocStatus,
                    EmployeeCode = x.Employee.EmpId,
                    EmployeeName = x.Employee.FnameTh + " " + x.Employee.LnameTh,
                    BenefitName = l.Benefit.NameTh,
                    Detail = l.Detail,
                    Qty = l.Qty,
                    Amount = l.Amount
                })
            .ToListAsync();
    }
    public async Task<(List<EmployeeSummaryDto> Items, int TotalCount)> GetEmployeeSummaryPagedAsync(
    BenefitClaimEmployeeFilterDto filter, List<Guid> adminBranchIds)
    {
        var q = _dbSet
            .AsNoTracking()
            .Where(x => !x.IsDelete && adminBranchIds.Contains(x.Employee.BranchId));

        if (filter.DateFrom.HasValue)
        {
            var from = filter.DateFrom.Value.Date;
            q = q.Where(x => x.DocDate >= from);
        }
        if (filter.DateTo.HasValue)
        {
            var toExclusive = filter.DateTo.Value.Date.AddDays(1);
            q = q.Where(x => x.DocDate < toExclusive);
        }
        if (filter.CompanyId.HasValue)
            q = q.Where(x => x.Employee.Company.Id == filter.CompanyId.Value);

        q = string.IsNullOrEmpty(filter.DocStatus)
            ? q.Where(x => x.DocStatus != "Draft")
            : q.Where(x => x.DocStatus == filter.DocStatus);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            q = q.Where(x => x.Employee.EmpId.Contains(s)
                || x.Employee.FnameTh.Contains(s)
                || x.Employee.LnameTh.Contains(s));
        }

        var grouped = q
            .GroupBy(x => new
            {
                x.EmployeeId,
                x.Employee.EmpId,
                x.Employee.FnameTh,
                x.Employee.LnameTh,
                Company = x.Employee.Company.NameTh
            })
            .Select(g => new EmployeeSummaryDto
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeCode = g.Key.EmpId,
                EmployeeName = g.Key.FnameTh + " " + g.Key.LnameTh,
                Company = g.Key.Company,
                DocCount = g.Count(),
                Amount = g.Sum(x => x.TotalAmount)
            });

        var total = await grouped.CountAsync();

        var page = Math.Max(1, filter.Page);
        var size = Math.Clamp(filter.PageSize, 1, 100);

        var items = await grouped
            .OrderBy(x => x.Company).ThenBy(x => x.EmployeeCode)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (items, total);
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