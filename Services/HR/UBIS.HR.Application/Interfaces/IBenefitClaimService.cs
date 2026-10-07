using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IBenefitClaimService
{
    Task<PagedResultDto<BenefitClaimDto>> GetAllAsync(BenefitClaimFilterDto filter);
    Task<BenefitClaimDto?> GetByIdAsync(Guid id);
    Task<BenefitClaimDto?> GetByDocNumAsync(string docNum);
    Task<BenefitClaimDto> CreateAsync(CreateBenefitClaimDto data);
    Task<BenefitClaimDto?> UpdateAsync(Guid id, CreateBenefitClaimDto data);
    Task<bool> DeleteAsync(Guid id);
    Task<BenefitClaimDto?> SubmitAsync(Guid id);
    Task<BenefitClaimDto?> RecallAsync(Guid id);
}