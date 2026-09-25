using Microsoft.EntityFrameworkCore;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class PositionLevelRepos : BaseRepos<TbPositionLevel>, IPositionLevelRepos
{
    public PositionLevelRepos(HrDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<TbPositionLevel>> GetAllAsync()
    {
        return await _dbSet.Where(x => !x.IsDelete).AsNoTracking().ToListAsync();
    }

    public async Task<(IEnumerable<TbPositionLevel> Items, int TotalCount)> GetFilteredPagedAsync(PositionLevelFilterDto filter)
    {
        var query = _dbSet.Where(x => !x.IsDelete);

        
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            var isNumeric = int.TryParse(search, out var searchLevel);
            query = query.Where(x => x.Code.Contains(search)
                || x.NameTh.Contains(search)
                || x.NameEn.Contains(search)
                ||(isNumeric && x.Level == searchLevel));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Level)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }
}