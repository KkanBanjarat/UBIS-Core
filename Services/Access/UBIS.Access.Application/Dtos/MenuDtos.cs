namespace UBIS.Access.Application.Dtos;

public class MenuNodeDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string NodeType { get; set; } = null!;   // Category / Module / Page
    public string Code { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string? Path { get; set; }
    public string? ComponentPath { get; set; }
    public string? Icon { get; set; }
    public string? PermissionCode { get; set; }     // resource key
    public int SortOrder { get; set; }
    public short AccessLevel { get; set; }          // เฉพาะ Page: 1 Read, 2 Write, 3 All (Category/Module = 0)
    public List<MenuNodeDto> Children { get; set; } = new();
}