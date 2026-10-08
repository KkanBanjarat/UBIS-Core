using UBIS.Access.Application.Dtos;

namespace UBIS.Access.Application.Interfaces;

public interface IMenuAdminService
{
    Task<List<MenuAdminDto>> GetAllAsync();
    Task<MenuAdminDto> CreateAsync(SaveMenuDto dto, string actor);
    Task<MenuAdminDto> UpdateAsync(Guid id, SaveMenuDto dto, string actor);
    Task DeleteAsync(Guid id, string actor);
}