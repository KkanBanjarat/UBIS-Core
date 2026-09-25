using Microsoft.EntityFrameworkCore;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class OrganizationUnitRepos : BaseRepos<TbOrganizationUnit>, IOrganizationUnitRepos
{
    public OrganizationUnitRepos(HrDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<TbOrganizationUnit> Items, int TotalCount)> GetFilteredPagedAsync(OrganizationUnitFilterDto filter)
    {
        var query = _dbSet.Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(filter.Type))
            query = query.Where(x => x.Type == filter.Type);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(x => x.NameTh.Contains(search)
                || x.NameEn.Contains(search)
                || (x.Code != null && x.Code.Contains(search))
                || (x.ShortName != null && x.ShortName.Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Type).ThenBy(x => x.NameTh)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }
}