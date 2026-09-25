using UBIS.Access.Domain.Entities;

namespace UBIS.Access.Infrastructure.Repositories.Interfaces;

public interface IUserRoleRepos : IBaseRepos<TbUserRole>
{
    Task<bool> IsRoleAssignedAsync(Guid userId, Guid roleId);
    Task<IEnumerable<TbUserRole>> GetByUserIdWithDetailsAsync(Guid userId);
    Task<IEnumerable<TbUserRole>> GetEffectiveRolesAsync(Guid userId);
}