using System;
using System.Collections.Generic;

namespace UBIS.Access.Domain.Entities;

public partial class TbMenu
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string NodeType { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Path { get; set; }

    public string? ComponentPath { get; set; }

    public string? Icon { get; set; }

    public string? PermissionCode { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public bool IsDelete { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<TbMenu> InverseParent { get; set; } = new List<TbMenu>();

    public virtual TbMenu? Parent { get; set; }

    public virtual ICollection<TbRoleMenu> TbRoleMenus { get; set; } = new List<TbRoleMenu>();
}
