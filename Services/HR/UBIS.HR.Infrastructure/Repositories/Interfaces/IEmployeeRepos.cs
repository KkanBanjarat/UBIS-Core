using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IEmployeeRepos : IBaseRepos<TbEmployee>
{
    Task<(IEnumerable<TbEmployee> Items, int TotalCount)> GetFilteredPagedAsync(EmployeeFilterDto filter);
    Task<TbEmployee?> GetDetailByIdAsync(Guid id);
    Task<Guid?> GetIdByEmpIdAsync(string empId);
    Task<TbEmployee?> GetDetailByEmpIdAsync(string empId);
    Task<(IEnumerable<TbEmployee> Items, int TotalCount)> SearchAsync(SearchEmployeeRequestDto request);
    Task<bool> ExistsByEmpIdAsync(string empId, Guid? excludeId = null);
    Task<List<EmployeeOrgChartNodeDto>> GetOrgChartFlatDataAsync();
    Task<Dictionary<Guid, string>> GetNamesByIdsAsync(List<Guid> ids);
}