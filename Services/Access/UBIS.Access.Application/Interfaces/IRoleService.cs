using UBIS.Access.Application.Dtos;

namespace UBIS.Access.Application.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync();
    Task<RoleDto?> GetByIdAsync(Guid id);
    Task<RoleDto> CreateAsync(CreateRoleDto data);
    Task<RoleDto?> UpdateAsync(Guid id ,CreateRoleDto data);
    Task<bool> DeleteAsync(Guid id);
}