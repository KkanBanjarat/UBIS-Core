using UBIS.Access.Application.Dtos;

namespace UBIS.Access.Application.Interfaces;

public interface IPermissionService
{
    Task<List<PermissionDto>> GetAllAsync();
    Task<PermissionDto?> GetByIdAsync(Guid id);
    Task<PermissionDto> CreateAsync(CreatePermissionDto data);
    Task<PermissionDto?> UpdateAsync(Guid id ,CreatePermissionDto data);
    Task<bool> DeleteAsync(Guid id);
}