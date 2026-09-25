using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IApprovalService
{
    Task CreateApprovalAsync(string docType, string docNumber, int docRev, List<Guid> approverIds);
    Task<List<MyApprovalDto>> GetMyPendingApprovalsAsync();
    Task<bool> ApproveAsync(int transApproveId);
    Task<bool> DenyAsync(int transApproveId, string reason, bool isDisapprove);
    Task RecallAsync(string docType, string docNumber, int docRev);
    Task<ApprovalTrailDto> GetTrailAsync(string docType, string docNumber);
}