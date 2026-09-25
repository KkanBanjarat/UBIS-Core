using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UBIS.Access.Infrastructure.Data;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Repositories.Implementations;

/// <summary>
/// Generic Repository Implementation
/// </summary>
public class BaseRepos<T> : IBaseRepos<T> where T : class
{
    protected readonly AccessDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepos(AccessDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    #region Read Operations

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).AsNoTracking().ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
    }

    #endregion

    #region Create Operations

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    #endregion

    #region Update Operations

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    #endregion

    #region Delete Operations

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public async Task<int> DeleteWhereAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ExecuteDeleteAsync();
    }

    #endregion

    #region Save Operations

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Count Operations

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }

    #endregion

    #region Read Operations

    public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstAsync(predicate);
    }

    public async Task<IEnumerable<T>> GetAllOrderByAsync<TKey>(
        Expression<Func<T, object>> orderBy)
    {
        return await _dbSet
            .OrderBy(orderBy)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize)
    {
        var totalCount = await _dbSet.CountAsync();
        var items = await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
        return (items, totalCount);
    }

    #endregion

    #region Exists Operations

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(x =>
            EF.Property<Guid>(x, "Id") == id);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    #endregion
}