using UBIS.HR.Application.Dtos;
namespace UBIS.HR.Application.Interfaces;

public interface IEmployeeService
{
    Task<PagedResultDto<EmployeeDto>> GetAllAsync(EmployeeFilterDto filter);
    Task<EmployeeDetailDto?> GetByIdAsync(Guid id, bool includeOrgChart = true);
    Task<EmployeeDetailDto?> GetByEmployeeCodeAsync(string employeeCode);
    Task<List<AllowedEmployeeDto>> GetAllowedEmployeesForDocumentAsync();
    Task<PagedResultDto<EmployeeSearchDto>> SearchEmployeesAsync(SearchEmployeeRequestDto request);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto data);
    Task<EmployeeDto?> UpdateAsync(Guid id, CreateEmployeeDto data);
    Task<bool> DeleteAsync(Guid id);

}