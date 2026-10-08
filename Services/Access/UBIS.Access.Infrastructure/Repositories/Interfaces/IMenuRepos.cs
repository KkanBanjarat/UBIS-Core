using UBIS.Access.Domain.Entities;

namespace UBIS.Access.Infrastructure.Repositories.Interfaces;

public interface IMenuRepos
{
    /// เมนูที่ active ทั้งหมด + AccessLevel สูงสุดของผู้ใช้ต่อเมนู (key = MenuId)
    Task<(List<TbMenu> Menus, Dictionary<Guid, short> Access, bool IsSuperAdmin)> GetUserMenuAccessAsync(Guid userId);
    Task<string?> GetRoleNameAsync(Guid roleId);
    Task<List<TbMenu>> GetActiveMenusAsync();
    Task<Dictionary<Guid, short>> GetRoleAccessAsync(Guid roleId);
    Task ReplaceRoleMenusAsync(Guid roleId, Dictionary<Guid, short> desired, string actor);
    Task<List<TbMenu>> GetAllMenusAsync();                       // รวม inactive
    Task<TbMenu?> GetMenuByIdAsync(Guid id);                     // tracked
    Task<bool> CodeExistsAsync(string code, Guid? excludeId);
    Task<bool> HasChildrenAsync(Guid id);
    Task AddMenuAsync(TbMenu menu);
    Task SaveMenuChangesAsync();
    Task SoftDeleteMenuAsync(Guid id, string actor);
}