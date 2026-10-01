using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IPettyCashRepos : IBaseRepos<TbPettyCashRequest>
{
    Task<TbPettyCashRequest?> GetDetailByIdAsync(Guid id);
    Task<TbPettyCashRequest?> GetByDocNumAsync(string docNum);
    Task<List<TbPettyCashRequest>> GetSummariesByDocNumsAsync(IEnumerable<string> docNums);
    Task<(IEnumerable<PettyCashDto> Items, int TotalCount)> GetFilteredPagedAsync(
    PettyCashFilterDto filter, List<Guid> adminBranchIds);
    Task DeleteLinesAsync(Guid pettyCashId);
}