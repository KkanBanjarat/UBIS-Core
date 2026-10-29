using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IBenefitClaimService
{
    Task<PagedResultDto<BenefitClaimDto>> GetAllAsync(BenefitClaimFilterDto filter);
    Task<BenefitClaimDto?> GetByIdAsync(Guid id);
    Task<BenefitClaimDto?> GetByDocNumAsync(string docNum);
    Task<List<DocFormDto>> GetDocFormsAsync();
    Task<CalculateLineResultDto> CalculateLineAsync(CalculateLineRequestDto req);
    Task<List<AvailableBenefitDto>> GetAvailableBenefitsAsync(Guid employeeId, int docFormId);
    Task<BenefitClaimSummaryDto> GetSummaryAsync(BenefitClaimSummaryFilterDto filter);
    Task<List<BenefitClaimExportRowDto>> GetDetailRowsAsync(BenefitClaimSummaryFilterDto filter);
    Task<PagedResultDto<EmployeeSummaryDto>> GetEmployeeSummaryPagedAsync(BenefitClaimEmployeeFilterDto filter);
    Task<List<BenefitClaimExportRowDto>> GetEmployeeRowsAsync(BenefitClaimEmployeeRowsFilterDto filter);
    Task<byte[]> ExportSummaryAsync(BenefitClaimSummaryFilterDto filter);
    Task<BenefitClaimDto> CreateAsync(CreateBenefitClaimDto data);
    Task<BenefitClaimDto?> UpdateAsync(Guid id, CreateBenefitClaimDto data);
    Task<bool> DeleteAsync(Guid id);
    Task<BenefitClaimDto?> SubmitAsync(Guid id);
    Task<BenefitClaimDto?> RecallAsync(Guid id);
}