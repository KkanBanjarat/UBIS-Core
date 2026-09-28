using Microsoft.EntityFrameworkCore;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Data;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Repositories.Implementations;

public class UserRepos : BaseRepos<TbUser>, IUserRepos
{
    public UserRepos(AccessDbContext context) : base(context) { }

    public async Task<TbUser?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDelete);
    }

    public async Task<TbUser?> GetByEntraObjectIdAsync(string entraObjectId)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId && !u.IsDelete);
    }
    public async Task<(IEnumerable<TbUser> Items, int TotalCount)> GetFilteredPagedAsync(UserFilterDto filter)
    {
        var page = Math.Max(1, filter.Page);
        var size = Math.Clamp(filter.PageSize, 1, 100);

        var query = _dbSet.AsNoTracking().Where(u => !u.IsDelete);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var k = filter.Search.Trim().ToLower();
            query = query.Where(u => u.DisplayName.ToLower().Contains(k)
                                  || u.Email.ToLower().Contains(k)
                                  || (u.EmployeeCode != null && u.EmployeeCode.ToLower().Contains(k)));
        }

        if (filter.Status == "active") query = query.Where(u => u.IsActive);
        else if (filter.Status == "inactive") query = query.Where(u => !u.IsActive);

        if (filter.Source == "entra") query = query.Where(u => u.EntraObjectId != null);
        else if (filter.Source == "local") query = query.Where(u => u.EntraObjectId == null);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.CreatedAt).ThenBy(u => u.Id)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync();

        return (items, total);
    }
    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email && !u.IsDelete);
    }

    public async Task<IEnumerable<TbUser>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => u.IsActive && !u.IsDelete)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdateLastLoginAtAsync(Guid userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.Now;
            await SaveChangesAsync();
        }
    }
    public async Task<TbUser?> GetByEmployeeIdAsync(Guid employeeId)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && !x.IsDelete);
    }

    public async Task<TbUser?> GetByEmployeeCodeAsync(string employeeCode)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode && !x.IsDelete);
    }
}