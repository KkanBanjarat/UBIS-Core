using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IPrettyCashRepos : IBaseRepos<TbPrettyCashRequest>
{
    Task<TbPrettyCashRequest?> GetDetailByIdAsync(Guid id);
    Task<TbPrettyCashRequest?> GetByDocNumAsync(string docNum);
    Task<(IEnumerable<TbPrettyCashRequest> Items, int TotalCount)> GetFilteredPagedAsync(
    PrettyCashFilterDto filter, Guid currentEmployeeId, string currentUserEmail, List<Guid> adminBranchIds);
    Task DeleteLinesAsync(Guid prettyCashId);
}