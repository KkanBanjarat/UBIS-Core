using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class EmployeeTypeRepos : BaseRepos<TbEmployeeType>, IEmployeeTypeRepos
{
    public EmployeeTypeRepos(HrDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<TbEmployeeType>> GetAllAsync()
    {
        return await _dbSet.Where(x => !x.IsDelete).AsNoTracking().ToListAsync();
    }

    public override async Task<TbEmployeeType?> GetByIdAsync(Guid id)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
    }
}