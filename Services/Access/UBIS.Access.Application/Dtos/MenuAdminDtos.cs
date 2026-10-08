namespace UBIS.Access.Application.Dtos;

public class MenuAdminDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string NodeType { get; set; } = "Page";   // Category | Module | Page
    public string Code { get; set; } = "";
    public string Label { get; set; } = "";
    public string? Path { get; set; }
    public string? ComponentPath { get; set; }
    public string? Icon { get; set; }
    public string? PermissionCode { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class SaveMenuDto
{
    public Guid? ParentId { get; set; }
    public string NodeType { get; set; } = "Page";
    public string Code { get; set; } = "";
    public string Label { get; set; } = "";
    public string? Path { get; set; }
    public string? ComponentPath { get; set; }
    public string? Icon { get; set; }
    public string? PermissionCode { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}