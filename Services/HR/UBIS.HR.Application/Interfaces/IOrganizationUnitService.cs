using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IOrganizationUnitService
{
    Task<IEnumerable<OrganizationUnitDto>> GetAllAsync(string? type = null);
    Task<OrganizationUnitDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<OrganizationUnitDto> Items, int TotalCount)> GetFilteredPagedAsync(OrganizationUnitFilterDto filter);
    Task<OrganizationUnitDto?> CreateAsync(CreateOrganizationUnitDto data);
    Task<OrganizationUnitDto?> UpdateAsync(Guid id, CreateOrganizationUnitDto data);
    Task<bool> DeleteAsync(Guid id);

    #region  Org Unit Path
    Task<List<OrganizationUnitPathItemDto>> GetPathAsync(Guid organizationUnitId);
    #endregion Org Unit Path
}