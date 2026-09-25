using UBIS.HR.Application.Dtos;
namespace UBIS.HR.Application.Interfaces;

public interface IBranchService
{
    Task<IEnumerable<BranchDto>> GetAllAsync();
    Task<BranchDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<BranchDto>> GetByCompanyIdAsync(Guid companyId);
}