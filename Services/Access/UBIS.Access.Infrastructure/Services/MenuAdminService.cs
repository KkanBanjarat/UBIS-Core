using Microsoft.Extensions.Logging;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Services;

public class MenuAdminService : IMenuAdminService
{
    private static readonly string[] NodeTypes = { "Category", "Module", "Page" };

    private readonly IMenuRepos _repo;
    private readonly ILogger<MenuAdminService> _logger;

    public MenuAdminService(IMenuRepos repo, ILogger<MenuAdminService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<MenuAdminDto>> GetAllAsync()
    {
        var list = await _repo.GetAllMenusAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<MenuAdminDto> CreateAsync(SaveMenuDto dto, string actor)
    {
        await ValidateAsync(dto, null);

        var now = DateTime.Now;
        var menu = new TbMenu { Id = Guid.NewGuid(), IsDelete = false, CreatedBy = actor, CreatedAt = now };
        Apply(menu, dto);
        menu.UpdatedBy = actor;
        menu.UpdatedAt = now;

        await _repo.AddMenuAsync(menu);
        await _repo.SaveMenuChangesAsync();
        return ToDto(menu);
    }

    public async Task<MenuAdminDto> UpdateAsync(Guid id, SaveMenuDto dto, string actor)
    {
        var menu = await _repo.GetMenuByIdAsync(id)
            ?? throw new KeyNotFoundException("ไม่พบเมนู");

        await ValidateAsync(dto, id);

        // เปลี่ยนเป็น Page ไม่ได้ถ้ามีลูกอยู่
        if (dto.NodeType == "Page" && await _repo.HasChildrenAsync(id))
            throw new InvalidOperationException("เมนูนี้มีเมนูย่อย ไม่สามารถเปลี่ยนเป็น Page ได้");

        Apply(menu, dto);
        menu.UpdatedBy = actor;
        menu.UpdatedAt = DateTime.Now;

        await _repo.SaveMenuChangesAsync();
        return ToDto(menu);
    }

    public async Task DeleteAsync(Guid id, string actor)
    {
        var menu = await _repo.GetMenuByIdAsync(id)
            ?? throw new KeyNotFoundException("ไม่พบเมนู");

        if (await _repo.HasChildrenAsync(id))
            throw new InvalidOperationException("ไม่สามารถลบได้ เพราะมีเมนูย่อยอยู่ กรุณาลบหรือย้ายเมนูย่อยก่อน");

        await _repo.SoftDeleteMenuAsync(id, actor);
        _logger.LogInformation("ลบเมนู {Code} โดย {Actor}", menu.Code, actor);
    }

    // ---------- helpers ----------

    private async Task ValidateAsync(SaveMenuDto dto, Guid? selfId)
    {
        dto.Code = (dto.Code ?? "").Trim();
        dto.Label = (dto.Label ?? "").Trim();
        dto.Path = string.IsNullOrWhiteSpace(dto.Path) ? null : dto.Path.Trim();
        dto.ComponentPath = string.IsNullOrWhiteSpace(dto.ComponentPath) ? null : dto.ComponentPath.Trim();

        if (dto.Code.Length == 0) throw new InvalidOperationException("กรุณาระบุ Code");
        if (dto.Label.Length == 0) throw new InvalidOperationException("กรุณาระบุชื่อเมนู");
        if (!NodeTypes.Contains(dto.NodeType)) throw new InvalidOperationException("NodeType ไม่ถูกต้อง");

        if (await _repo.CodeExistsAsync(dto.Code, selfId))
            throw new InvalidOperationException($"Code '{dto.Code}' ถูกใช้แล้ว");

        if (dto.NodeType == "Page")
        {
            if (dto.Path == null) throw new InvalidOperationException("Page ต้องระบุ Path");
            if (dto.ComponentPath == null) throw new InvalidOperationException("Page ต้องระบุ ComponentPath");
        }
        else
        {
            dto.Path = null;
            dto.ComponentPath = null;
            dto.PermissionCode = null;
        }

        // ตรวจ parent
        if (dto.NodeType == "Category")
        {
            if (dto.ParentId != null) throw new InvalidOperationException("Category ต้องเป็นระดับบนสุด");
            return;
        }

        if (dto.ParentId == null) return;   // Module/Page อยู่ root ได้

        if (selfId.HasValue && dto.ParentId == selfId)
            throw new InvalidOperationException("ไม่สามารถตั้งตัวเองเป็นเมนูแม่ได้");

        var all = await _repo.GetAllMenusAsync();
        var byId = all.ToDictionary(m => m.Id);

        if (!byId.TryGetValue(dto.ParentId.Value, out var parent))
            throw new InvalidOperationException("ไม่พบเมนูแม่");
        if (parent.NodeType == "Page")
            throw new InvalidOperationException("Page ไม่สามารถเป็นเมนูแม่ได้");
        if (dto.NodeType == "Module" && parent.NodeType != "Category")
            throw new InvalidOperationException("Module ต้องอยู่ใต้ Category");

        // กันวนลูป
        if (selfId.HasValue)
        {
            var cur = parent;
            while (true)
            {
                if (cur.Id == selfId) throw new InvalidOperationException("ไม่สามารถย้ายเมนูไปอยู่ใต้เมนูลูกของตัวเองได้");
                if (cur.ParentId == null || !byId.TryGetValue(cur.ParentId.Value, out cur)) break;
            }
        }
    }

    private static void Apply(TbMenu m, SaveMenuDto d)
    {
        m.ParentId = d.ParentId;
        m.NodeType = d.NodeType;
        m.Code = d.Code;
        m.Label = d.Label;
        m.Path = d.Path;
        m.ComponentPath = d.ComponentPath;
        m.Icon = string.IsNullOrWhiteSpace(d.Icon) ? null : d.Icon.Trim();
        m.PermissionCode = string.IsNullOrWhiteSpace(d.PermissionCode) ? null : d.PermissionCode.Trim();
        m.SortOrder = d.SortOrder;
        m.IsActive = d.IsActive;
    }

    private static MenuAdminDto ToDto(TbMenu m) => new()
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
        IsActive = m.IsActive,
    };
}