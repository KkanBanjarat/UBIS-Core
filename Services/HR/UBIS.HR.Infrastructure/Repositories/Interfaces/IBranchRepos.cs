using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IBranchRepos : IBaseRepos<TbBranch>
{
    Task<IEnumerable<TbBranch>> GetByCompanyIdAsync(Guid companyId);
}