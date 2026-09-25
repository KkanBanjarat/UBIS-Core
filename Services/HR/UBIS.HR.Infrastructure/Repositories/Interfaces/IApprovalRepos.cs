using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IApprovalRepos : IBaseRepos<TbTransApprove>
{
    Task<List<TbTransApprove>> GetByDocumentAsync(string docType, string docNumber, int docRev, int round);
    Task<int> GetLatestRoundAsync(string docType, string docNumber, int docRev);
    Task<List<TbTransApprove>> GetPendingByApproverAsync(Guid approverId);
    Task<TbTransApprove?> GetByIntIdAsync(int id);
}