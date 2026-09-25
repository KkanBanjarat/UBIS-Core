using System;
using System.Collections.Generic;

namespace UBIS.Access.Domain.Entities;

public partial class TbRolePermission
{
    public Guid Id { get; set; }

    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public bool IsDelete { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TbPermission Permission { get; set; } = null!;

    public virtual TbRole Role { get; set; } = null!;
}
