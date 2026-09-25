using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IApprovalRouteResolverService
{
    Task<List<ResolvedApprover>> ResolveAsync(string docType, Guid employeeId);
}