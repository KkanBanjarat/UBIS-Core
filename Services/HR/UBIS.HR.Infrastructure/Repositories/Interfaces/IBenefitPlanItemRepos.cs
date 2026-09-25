using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IBenefitPlanItemRepos : IBaseRepos<TbBenefitPlanItem>
{
    Task<IEnumerable<TbBenefitPlanItem>> GetAllWithDetailsAsync();
    Task<IEnumerable<TbBenefitPlanItem>> GetByBenefitPlanIdAsync(Guid benefitPlanId);

    Task<IEnumerable<TbBenefitPlanItem>> GetByBenefitIdAsync(Guid benefitId);

    Task<bool> IsBenefitInPlanAsync(Guid benefitPlanId, Guid benefitId);
}