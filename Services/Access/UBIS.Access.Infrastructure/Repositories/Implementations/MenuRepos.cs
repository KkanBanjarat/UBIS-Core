using Microsoft.EntityFrameworkCore;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Data;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Repositories.Implementations;

public class MenuRepos : IMenuRepos
{
    private readonly AccessDbContext _context;

    public MenuRepos(AccessDbContext context)
    {
        _context = context;
    }

    public async Task<(List<TbMenu> Menus, Dictionary<Guid, short> Access, bool IsSuperAdmin)> GetUserMenuAccessAsync(Guid userId)
    {
        var roleIds = await _context.Set<TbUserRole>()
            .Where(x => x.UserId == userId && !x.IsDelete)
            .Select(x => x.RoleId)
            .Distinct()
            .ToListAsync();

        var menus = await _context.Set<TbMenu>()
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive)
            .ToListAsync();

        var isSuperAdmin = await _context.Set<TbRole>()
            .AnyAsync(r => roleIds.Contains(r.Id) && r.Name == "SuperAdmin" && !r.IsDelete);

        var access = await _context.Set<TbRoleMenu>()
            .AsNoTracking()
            .Where(x => roleIds.Contains(x.RoleId) && !x.IsDelete)
            .GroupBy(x => x.MenuId)
            .Select(g => new { MenuId = g.Key, Level = g.Max(x => x.AccessLevel) })
            .ToDictionaryAsync(x => x.MenuId, x => x.Level);

        return (menus, access, isSuperAdmin);
    }
    public async Task<string?> GetRoleNameAsync(Guid roleId)
    {
        return await _context.Set<TbRole>()
            .Where(r => r.Id == roleId && !r.IsDelete)
            .Select(r => r.Name)
            .FirstOrDefaultAsync();
    }

    public async Task<List<TbMenu>> GetActiveMenusAsync()
    {
        return await _context.Set<TbMenu>()
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive)
            .ToListAsync();
    }

    public async Task<Dictionary<Guid, short>> GetRoleAccessAsync(Guid roleId)
    {
        return await _context.Set<TbRoleMenu>()
            .AsNoTracking()
            .Where(x => x.RoleId == roleId && !x.IsDelete)
            .ToDictionaryAsync(x => x.MenuId, x => x.AccessLevel);
    }

    public async Task ReplaceRoleMenusAsync(Guid roleId, Dictionary<Guid, short> desired, string actor)
    {
        var now = DateTime.Now;

        // รับเฉพาะโหนดชนิด Page (Category/Module ไม่ใส่สิทธิ์โดยตรง)
        var validPageIds = (await _context.Set<TbMenu>()
            .Where(m => !m.IsDelete && m.NodeType == "Page")
            .Select(m => m.Id)
            .ToListAsync()).ToHashSet();

        var existing = await _context.Set<TbRoleMenu>()
            .Where(x => x.RoleId == roleId && !x.IsDelete)
            .ToListAsync();
        var existingByMenu = existing.ToDictionary(x => x.MenuId);

        foreach (var (menuId, level) in desired)
        {
            if (!validPageIds.Contains(menuId)) continue;
            existingByMenu.TryGetValue(menuId, out var row);

            if (level <= 0)
            {
                if (row != null)
                {
                    row.IsDelete = true;
                    row.DeletedBy = actor;
                    row.DeletedAt = now;
                }
                continue;
            }

            if (row == null)
            {
                _context.Set<TbRoleMenu>().Add(new TbRoleMenu
                {
                    RoleId = roleId,
                    MenuId = menuId,
                    AccessLevel = level,
                    IsDelete = false,
                    CreatedBy = actor,
                    CreatedAt = now,
                    UpdatedBy = actor,
                    UpdatedAt = now,
                });
            }
            else if (row.AccessLevel != level)
            {
                row.AccessLevel = level;
                row.UpdatedBy = actor;
                row.UpdatedAt = now;
            }
        }

        // เมนูที่ไม่อยู่ในรายการที่ส่งมา = ถอนสิทธิ์
        foreach (var row in existing)
        {
            if (desired.ContainsKey(row.MenuId)) continue;
            row.IsDelete = true;
            row.DeletedBy = actor;
            row.DeletedAt = now;
        }

        await _context.SaveChangesAsync();   // ครั้งเดียว = transaction เดียว
    }

    public async Task<List<TbMenu>> GetAllMenusAsync()
    {
        return await _context.Set<TbMenu>()
            .AsNoTracking()
            .Where(x => !x.IsDelete)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();
    }

    public async Task<TbMenu?> GetMenuByIdAsync(Guid id)
    {
        return await _context.Set<TbMenu>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId)
    {
        return await _context.Set<TbMenu>()
            .AnyAsync(x => !x.IsDelete && x.Code == code && (excludeId == null || x.Id != excludeId));
    }

    public async Task<bool> HasChildrenAsync(Guid id)
    {
        return await _context.Set<TbMenu>().AnyAsync(x => !x.IsDelete && x.ParentId == id);
    }

    public async Task AddMenuAsync(TbMenu menu)
    {
        await _context.Set<TbMenu>().AddAsync(menu);
    }

    public async Task SaveMenuChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteMenuAsync(Guid id, string actor)
    {
        var now = DateTime.Now;

        var menu = await _context.Set<TbMenu>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
        if (menu == null) return;

        menu.IsDelete = true;
        menu.DeletedBy = actor;
        menu.DeletedAt = now;

        // ถอนสิทธิ์ role ที่ผูกกับเมนูนี้ด้วย
        var roleMenus = await _context.Set<TbRoleMenu>()
            .Where(x => x.MenuId == id && !x.IsDelete)
            .ToListAsync();
        foreach (var rm in roleMenus)
        {
            rm.IsDelete = true;
            rm.DeletedBy = actor;
            rm.DeletedAt = now;
        }

        await _context.SaveChangesAsync();   // transaction เดียว
    }
}