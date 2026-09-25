using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IPositionRepos : IBaseRepos<TbPosition>
{
    Task<(IEnumerable<TbPosition> Items, int TotalCount)> GetFilteredPagedAsync(PositionFilterDto filter);
}