using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IBenefitService
{
    Task<IEnumerable<BenefitDto>> GetAllAsync();
    Task<BenefitDto?> GetByIdAsync(Guid id);
    Task<BenefitDto> CreateAsync(CreateBenefitDto data);
    Task<BenefitDto?> UpdateAsync(Guid id ,CreateBenefitDto data);
    Task<bool> DeleteAsync(Guid id);
}