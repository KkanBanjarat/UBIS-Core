using System;
using System.Collections.Generic;

namespace UBIS.Access.Domain.Entities;

public partial class TbRoleMenu
{
    public Guid Id { get; set; }

    public Guid RoleId { get; set; }

    public Guid MenuId { get; set; }

    public bool IsDelete { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public short AccessLevel { get; set; }

    public virtual TbMenu Menu { get; set; } = null!;

    public virtual TbRole Role { get; set; } = null!;
}
