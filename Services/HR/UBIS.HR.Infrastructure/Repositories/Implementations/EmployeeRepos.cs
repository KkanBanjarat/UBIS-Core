using Microsoft.EntityFrameworkCore;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class EmployeeRepos : BaseRepos<TbEmployee>, IEmployeeRepos
{
    public EmployeeRepos(HrDbContext context) : base(context) { }

    private IQueryable<TbEmployee> IncludeList()
    {
        return _dbSet
            .Include(x => x.Position)
            .Include(x => x.PositionLevel)
            .Include(x => x.Company)
            .Include(x => x.Group)
            .Include(x => x.Department)
            .Include(x => x.Division)
            .Include(x => x.Section)
            .Include(x => x.EmployeeType)
            .Include(x => x.Branch)
            .Include(x => x.ReportTo);
    }
    private IQueryable<TbEmployee> IncludeDetail()
    {
        return IncludeList()
            .AsSplitQuery()
            .Include(x => x.TbEmployeeBenefitPlans.Where(bp => !bp.IsDelete && bp.BenefitPlan!.IsActive))
                .ThenInclude(bp => bp.BenefitPlan)
                .ThenInclude(p => p!.TbBenefitPlanItems.Where(i => !i.IsDelete && i.Benefit.IsActive))
                .ThenInclude(i => i.Benefit);
    }

    public async Task<(IEnumerable<TbEmployee> Items, int TotalCount)> GetFilteredPagedAsync(EmployeeFilterDto filter)
    {
        var query = IncludeList()
            .AsNoTracking()
            .Where(w => w.IsDelete == false);

        if (!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(w => w.FnameTh.Contains(filter.Search)
                || w.LnameTh.Contains(filter.Search)
                || w.FnameEn.Contains(filter.Search)
                || w.LnameEn.Contains(filter.Search)
                || w.EmpId.Contains(filter.Search)
                || w.Email.Contains(filter.Search)
                || w.Position.NameEn.Contains(filter.Search)
                || w.Position.NameTh.Contains(filter.Search));
        }

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(w => w.Status == filter.Status);

        if (filter.EmployeeTypeId.HasValue)
            query = query.Where(w => w.EmployeeTypeId == filter.EmployeeTypeId);

        if (filter.CompanyId.HasValue)
            query = query.Where(w => w.CompanyId == filter.CompanyId);

        if (filter.BranchId.HasValue)
            query = query.Where(w => w.BranchId == filter.BranchId);

        if (filter.GroupId.HasValue)
            query = query.Where(w => w.GroupId == filter.GroupId);

        if (filter.DepartmentId.HasValue)
            query = query.Where(w => w.DepartmentId == filter.DepartmentId);

        if (filter.DivisionId.HasValue)
            query = query.Where(w => w.DivisionId == filter.DivisionId);

        if (filter.SectionId.HasValue)
            query = query.Where(w => w.SectionId == filter.SectionId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(w => w.UpdatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<TbEmployee?> GetDetailByIdAsync(Guid id)
    {
        return await IncludeDetail()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id && w.IsDelete == false);
    }
    public async Task<Guid?> GetIdByEmpIdAsync(string empId)
    {
        return await _dbSet
            .Where(x => x.EmpId == empId && !x.IsDelete)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<TbEmployee?> GetDetailByEmpIdAsync(string empId)
    {
        return await IncludeDetail()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.EmpId == empId && w.IsDelete == false);
    }

    public async Task<List<EmployeeOrgChartNodeDto>> GetOrgChartFlatDataAsync()
    {
        return await _dbSet
            .Where(x => !x.IsDelete)
            .Select(x => new EmployeeOrgChartNodeDto
            {
                Id = x.Id,
                EmpId = x.EmpId,
                FullNameTh = x.FnameTh + " " + x.LnameTh,
                FullNameEn = x.FnameEn + " " + x.LnameEn,
                PositionNameTh = x.Position != null ? x.Position.NameTh : null,
                PositionNameEn = x.Position != null ? x.Position.NameEn : null,
                ReportToId = x.ReportToId,
                BranchId = x.BranchId,
                PositionLevel = x.PositionLevel.Level,
            })
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<(IEnumerable<TbEmployee> Items, int TotalCount)> SearchAsync(SearchEmployeeRequestDto request)
    {
        var query = _dbSet
            .Include(x => x.Position)
            .AsNoTracking()
            .Where(e => e.IsDelete == false);

        if (request.CompanyId.HasValue)
            query = query.Where(e => e.CompanyId == request.CompanyId);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(e => e.Status == request.Status);

        if (request.ExcludeId.HasValue)
            query = query.Where(e => e.Id != request.ExcludeId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e =>
                e.FnameTh.ToLower().Contains(search) ||
                e.LnameTh.ToLower().Contains(search) ||
                e.EmpId.ToLower().Contains(search) ||
                e.FnameEn.ToLower().Contains(search) ||
                e.LnameEn.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 50 : request.PageSize;

        var items = await query
            .OrderBy(e => e.FnameTh)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> ExistsByEmpIdAsync(string empId, Guid? excludeId = null)
    {
        return await _dbSet.AnyAsync(x => x.EmpId == empId
            && x.IsDelete == false
            && (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    public async Task<Dictionary<Guid, string>> GetNamesByIdsAsync(List<Guid> ids)
    {
        return await _dbSet
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, FullNameTh = x.FnameTh + " " + x.LnameTh })
            .ToDictionaryAsync(x => x.Id, x => x.FullNameTh);
    }
}