using UBIS.Access.Application.Dtos;

namespace UBIS.Access.Application.Interfaces;

public interface IMenuService
{
    Task<List<MenuNodeDto>> GetMyMenusAsync(Guid userId);
}