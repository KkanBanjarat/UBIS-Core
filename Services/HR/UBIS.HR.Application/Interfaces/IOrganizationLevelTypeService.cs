using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IOrganizationLevelTypeService
{
    Task<IEnumerable<OrganizationLevelTypeDto>> GetAllAsync();
    Task<OrganizationLevelTypeDto?> GetByIdAsync(Guid id);
    Task<OrganizationLevelTypeDto> CreateAsync(CreateOrganizationLevelTypeDto data);
    Task<OrganizationLevelTypeDto?> UpdateAsync(Guid id, CreateOrganizationLevelTypeDto data);
    Task<bool> DeleteAsync(Guid id);
}