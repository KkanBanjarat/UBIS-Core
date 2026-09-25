using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class RouteApproveRepos : BaseRepos<TbRouteApprove>, IRouteApproveRepos
{
    public RouteApproveRepos(HrDbContext context) : base(context)
    {
    }

    public async Task<List<TbRouteApprove>> GetActiveRouteAsync(string docType)
    {
        return await _dbSet
            .Where(x => x.DocType == docType && x.IsActive)
            .OrderBy(x => x.StepNo)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<TbRouteApprove>> GetAllRoutesAsync()
    {
        return await _dbSet
            .OrderBy(x => x.DocType)
            .ThenBy(x => x.StepNo)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<string>> GetDocTypesAsync()
    {
        return await _dbSet
            .Select(x => x.DocType)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
    }

    public async Task<bool> ExistsStepNoAsync(string docType, int stepNo, Guid? excludeId = null)
    {
        return await _dbSet.AnyAsync(x =>
            x.DocType == docType
            && x.StepNo == stepNo
            && x.IsActive
            && (!excludeId.HasValue || x.Id != excludeId.Value));
    }
}