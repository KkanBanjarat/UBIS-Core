using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IBenefitClaimRepos : IBaseRepos<TbBenefitClaim>
{
    Task<TbBenefitClaim?> GetDetailByIdAsync(Guid id);
    Task<TbBenefitClaim?> GetByDocNumAsync(string docNum);
    Task<List<TbBenefitClaim>> GetSummariesByDocNumsAsync(IEnumerable<string> docNums);
    Task<BenefitClaimSummaryDto> GetSummaryAsync(BenefitClaimSummaryFilterDto filter, List<Guid> adminBranchIds);
    Task<List<BenefitClaimExportRowDto>> GetExportRowsAsync(
    BenefitClaimSummaryFilterDto filter, List<Guid> adminBranchIds, Guid? employeeId = null);
    Task<(List<EmployeeSummaryDto> Items, int TotalCount)> GetEmployeeSummaryPagedAsync(
    BenefitClaimEmployeeFilterDto filter, List<Guid> adminBranchIds);

    Task<(IEnumerable<BenefitClaimDto> Items, int TotalCount)> GetFilteredPagedAsync(
        BenefitClaimFilterDto filter, List<Guid> adminBranchIds);
    Task DeleteLinesAsync(Guid benefitClaimId);
}