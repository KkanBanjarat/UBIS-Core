using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class OrganizationLevelTypeRepos : BaseRepos<TbOrganizationLevelType>, IOrganizationLevelTypeRepos
{
    public OrganizationLevelTypeRepos(HrDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<TbOrganizationLevelType>> GetAllAsync()
    {
        return await _dbSet.Where(x => !x.IsDelete).AsNoTracking().ToListAsync();
    }

}