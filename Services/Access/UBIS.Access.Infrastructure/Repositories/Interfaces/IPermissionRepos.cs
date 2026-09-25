using UBIS.Access.Domain.Entities;

namespace UBIS.Access.Infrastructure.Repositories.Interfaces;

public interface IPermissionRepos : IBaseRepos<TbPermission>
{
    Task<TbPermission?> GetByCodeAsync(string code);
    Task<bool> IsCodeExistsAsync(string code);
}