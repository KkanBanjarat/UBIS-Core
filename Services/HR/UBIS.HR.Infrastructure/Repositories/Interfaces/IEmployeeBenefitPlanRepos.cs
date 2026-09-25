using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IEmployeeBenefitPlanRepos : IBaseRepos<TbEmployeeBenefitPlan>
{
    Task<List<TbEmployeeBenefitPlan>> GetByEmployeeIdAsync(Guid employeeId);
}