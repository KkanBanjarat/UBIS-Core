namespace UBIS.Access.Application.Dtos;

public class RoleMenuItemDto
{
    public Guid MenuId { get; set; }
    public Guid? ParentId { get; set; }
    public string NodeType { get; set; } = null!;   // Category / Module / Page
    public string Code { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string? Path { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public short AccessLevel { get; set; }          // 0 = ไม่มีสิทธิ์, 1 Read, 2 Write, 3 All
}

public class RoleMenuAccessDto
{
    public Guid MenuId { get; set; }
    public short AccessLevel { get; set; }
}

public class UpdateRoleMenusDto
{
    public List<RoleMenuAccessDto> Items { get; set; } = new();
}