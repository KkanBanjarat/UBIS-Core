using Microsoft.EntityFrameworkCore;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class PositionRepos : BaseRepos<TbPosition>, IPositionRepos
{
    public PositionRepos(HrDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<TbPosition>> GetAllAsync()
    {
        return await _dbSet.Where(x => !x.IsDelete).AsNoTracking().ToListAsync();
    }

    public async Task<(IEnumerable<TbPosition> Items, int TotalCount)> GetFilteredPagedAsync(PositionFilterDto filter)
    {
        var query = _dbSet.Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(x => x.NameTh.Contains(search) || x.NameEn.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.NameTh)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }
}