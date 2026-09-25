using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IReasonApproveRepos : IBaseRepos<TbReasonApprove>
{
    Task<List<TbReasonApprove>> GetByDocumentAsync(string docType, string docNumber, int docRev);
}