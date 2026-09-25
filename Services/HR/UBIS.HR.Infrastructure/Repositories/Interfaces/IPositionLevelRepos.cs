using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IPositionLevelRepos : IBaseRepos<TbPositionLevel>
{
    Task<(IEnumerable<TbPositionLevel> Items, int TotalCount)> GetFilteredPagedAsync(PositionLevelFilterDto filter);
}