using UBIS.HR.Application.Dtos;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IOrganizationUnitRepos : IBaseRepos<TbOrganizationUnit>
{
    Task<(IEnumerable<TbOrganizationUnit> Items, int TotalCount)> GetFilteredPagedAsync(OrganizationUnitFilterDto filter);
}