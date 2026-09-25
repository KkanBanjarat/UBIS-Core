using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Repositories.Interfaces;

public interface IRoleRepos : IBaseRepos<TbRole>
{
    Task<TbRole?> GetByNameAsync(string name);
    Task<bool> IsNameExistsAsync(string name);
    Task<IEnumerable<TbRole>> GetActiveRolesAsync();
}