using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IBenefitPlanService
{
    Task<IEnumerable<BenefitPlanDto>> GetAllAsync();
    Task<BenefitPlanDto?> GetByIdAsync(Guid id);
    Task<BenefitPlanDto> CreateAsync(CreateBenefitPlanDto data);
    Task<BenefitPlanDto?> UpdateAsync(Guid id ,CreateBenefitPlanDto data);
    Task<bool> DeleteAsync(Guid id);
}