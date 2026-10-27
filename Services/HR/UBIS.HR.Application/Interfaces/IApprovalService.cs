using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IApprovalService
{
    Task CreateApprovalAsync(string docType, string docNumber, int docRev, List<ResolvedApprover> resolvedApprovers);
    Task<List<MyApprovalDto>> GetMyPendingApprovalsAsync(MyApprovalFilterDto? filter = null);
    Task<List<ApprovalSummaryDto>> GetPendingApprovalsAsync(Guid actor);
    Task<bool> ApproveAsync(int transApproveId);
    Task<bool> DenyAsync(int transApproveId, string reason, bool isDisapprove);
    Task RecallAsync(string docType, string docNumber, int docRev);
    Task<ApprovalTrailDto> GetTrailAsync(string docType, string docNumber);
}