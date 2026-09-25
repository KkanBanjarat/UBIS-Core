using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IBenefitPlanItemService
{
    Task<IEnumerable<BenefitPlanItemDto>> GetAllAsync();
    Task<BenefitPlanItemDto?> GetByIdAsync(Guid id);
    Task<BenefitPlanItemDto> CreateAsync(CreateBenefitPlanItemDto data);
    Task<BenefitPlanItemDto?> UpdateAsync(Guid id ,CreateBenefitPlanItemDto data);
    Task<bool> DeleteAsync(Guid id);
}