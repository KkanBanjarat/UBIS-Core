using Microsoft.Extensions.Logging;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Services;

public class RoleMenuService : IRoleMenuService
{
    private readonly IMenuRepos _repo;
    private readonly ILogger<RoleMenuService> _logger;
    private readonly ICurrentUserService _currentUser;

    public RoleMenuService(IMenuRepos repo, ILogger<RoleMenuService> logger, ICurrentUserService currentUser)
    {
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<List<RoleMenuItemDto>?> GetAsync(Guid roleId)
    {
        try
        {
            if (await _repo.GetRoleNameAsync(roleId) == null) return null;

            var menus = await _repo.GetActiveMenusAsync();
            var access = await _repo.GetRoleAccessAsync(roleId);

            return menus
                .OrderBy(m => m.SortOrder)
                .Select(m => new RoleMenuItemDto
                {
                    MenuId = m.Id,
                    ParentId = m.ParentId,
                    NodeType = m.NodeType,
                    Code = m.Code,
                    Label = m.Label,
                    Path = m.Path,
                    Icon = m.Icon,
                    SortOrder = m.SortOrder,
                    AccessLevel = access.TryGetValue(m.Id, out var lv) ? lv : (short)0,
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงเมนูของ Role : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Guid roleId, UpdateRoleMenusDto data)
    {
        try
        {
            var roleName = await _repo.GetRoleNameAsync(roleId);
            if (roleName == null) return false;

            if (roleName == "SuperAdmin")
                throw new InvalidOperationException("SuperAdmin เข้าถึงทุกเมนูอัตโนมัติ ไม่ต้องกำหนดสิทธิ์");

            if (data.Items.Any(i => i.AccessLevel < 0 || i.AccessLevel > 3))
                throw new ArgumentException("ระดับสิทธิ์ต้องอยู่ระหว่าง 0 ถึง 3");

            var desired = data.Items
                .GroupBy(i => i.MenuId)
                .ToDictionary(g => g.Key, g => g.Last().AccessLevel);

            await _repo.ReplaceRoleMenusAsync(roleId, desired, _currentUser.GetCurrentUserEmail());
            return true;
        }
        catch (Exception ex) when (ex is not (InvalidOperationException or ArgumentException))
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการบันทึกเมนูของ Role : {Message}", ex.Message);
            throw;
        }
    }
}