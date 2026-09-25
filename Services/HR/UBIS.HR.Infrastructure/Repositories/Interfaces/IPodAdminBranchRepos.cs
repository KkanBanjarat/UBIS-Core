using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IPodAdminBranchRepos : IBaseRepos<TbPodAdminBranch>
{
    Task<List<TbPodAdminBranch>> GetAllWithBranchAsync();
    Task<List<TbPodAdminBranch>> GetByUserIdWithBranchAsync(Guid userId);
    Task<TbPodAdminBranch?> GetPrimaryByBranchIdAsync(Guid branchId);
    Task<List<TbPodAdminBranch>> GetByBranchIdAsync(Guid branchId);
}