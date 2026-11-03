using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public class DocLimitRepos : BaseRepos<TbDocLimit>, IDocLimitRepos
{
    public DocLimitRepos(HrDbContext context) : base(context)
    {
    }

    public async Task<TbDocLimit?> GetEffectiveAsync(string docType, DateTime date)
    {
        var day = date.Date;
        return await _dbSet
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive
                        && x.DocType == docType
                        && x.EffectiveFrom <= day)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync();
    }
}