using Microsoft.Extensions.Logging;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepos _repo;
    private readonly ILogger<MenuService> _logger;

    public MenuService(IMenuRepos repo, ILogger<MenuService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<MenuNodeDto>> GetMyMenusAsync(Guid userId)
    {
        try
        {
            var (menus, access, isSuperAdmin) = await _repo.GetUserMenuAccessAsync(userId);
            var byId = menus.ToDictionary(m => m.Id);

            // หน้า (Page) ที่ผู้ใช้เข้าถึงได้ พร้อมระดับสิทธิ์
            // SuperAdmin เห็นทุกหน้าที่ active ระดับ 3 เสมอ (กันล็อกตัวเองเมื่อเพิ่มเมนูใหม่)
            var pageLevel = new Dictionary<Guid, short>();
            foreach (var m in menus.Where(m => m.NodeType == "Page"))
            {
                if (isSuperAdmin) pageLevel[m.Id] = 3;
                else if (access.TryGetValue(m.Id, out var lv) && lv > 0) pageLevel[m.Id] = lv;
            }

            // โหนดที่ต้องแสดง = หน้าที่เข้าถึงได้ + บรรพบุรุษทั้งหมด
            var visible = new HashSet<Guid>(pageLevel.Keys);
            foreach (var pageId in pageLevel.Keys)
            {
                var parentId = byId[pageId].ParentId;
                while (parentId.HasValue && byId.TryGetValue(parentId.Value, out var parent) && visible.Add(parent.Id))
                    parentId = parent.ParentId;
            }

            var nodes = visible.ToDictionary(id => id, id =>
            {
                var m = byId[id];
                return new MenuNodeDto
                {
                    Id = m.Id,
                    ParentId = m.ParentId,
                    NodeType = m.NodeType,
                    Code = m.Code,
                    Label = m.Label,
                    Path = m.Path,
                    ComponentPath = m.ComponentPath,
                    Icon = m.Icon,
                    PermissionCode = m.PermissionCode,
                    SortOrder = m.SortOrder,
                    AccessLevel = pageLevel.TryGetValue(id, out var lv) ? lv : (short)0,
                };
            });

            var roots = new List<MenuNodeDto>();
            foreach (var node in nodes.Values)
            {
                if (node.ParentId.HasValue && nodes.TryGetValue(node.ParentId.Value, out var parent))
                    parent.Children.Add(node);
                else
                    roots.Add(node);
            }

            SortTree(roots);
            return roots;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงเมนูของผู้ใช้ : {Message}", ex.Message);
            throw;
        }
    }

    private static void SortTree(List<MenuNodeDto> list)
    {
        list.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
        foreach (var n in list) SortTree(n.Children);
    }
}