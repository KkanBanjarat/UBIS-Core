using System.Linq.Expressions;

namespace UBIS.Access.Infrastructure.Repositories.Interfaces;

public interface IBaseRepos<T> where T : class
{
    #region Read Operations
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T> FirstAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> GetAllOrderByAsync<TKey>(Expression<Func<T, object>> orderBy);
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    #endregion

    #region Create Operations
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    #endregion

    #region Update Operations
    Task UpdateAsync(T entity);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    #endregion

    #region Delete Operations
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    Task<int> DeleteWhereAsync(Expression<Func<T, bool>> predicate);
    #endregion

    #region Save Operations
    Task SaveChangesAsync();
    #endregion

    #region Count Operations
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    #endregion

    #region Exists Operations
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    #endregion
}