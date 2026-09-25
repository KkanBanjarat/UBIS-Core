using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IPodAdminBranchService
{
    Task<List<PodAdminBranchGroupDto>> GetAllAsync();
    Task<List<BranchSummaryDto>> GetByUserIdAsync(Guid userId);
    Task<PodAdminBranchDto> CreateAsync(CreatePodAdminBranchDto data);
    Task<bool> DeleteAsync(Guid id);

    // สำหรับหน้าจัดการผู้ดูแลสาขา
    Task<List<BranchAdminGroupDto>> GetGroupedByBranchAsync();
    Task<bool> SetPrimaryAsync(Guid id);
}