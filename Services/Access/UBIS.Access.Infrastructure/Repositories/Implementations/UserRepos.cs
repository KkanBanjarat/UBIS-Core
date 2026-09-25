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