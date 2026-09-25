using UBIS.Access.Domain.Entities;

namespace UBIS.Access.Infrastructure.Repositories.Interfaces;

public interface IUserRepos : IBaseRepos<TbUser>
{
    Task<TbUser?> GetByEmailAsync(string email);
    Task<TbUser?> GetByEntraObjectIdAsync(string entraObjectId);
    Task<TbUser?> GetByEmployeeIdAsync(Guid employeeId);
    Task<TbUser?> GetByEmployeeCodeAsync(string employeeCode);
    Task<bool> IsEmailExistsAsync(string email);
    Task<IEnumerable<TbUser>> GetActiveUsersAsync();
    Task UpdateLastLoginAtAsync(Guid userId);
}