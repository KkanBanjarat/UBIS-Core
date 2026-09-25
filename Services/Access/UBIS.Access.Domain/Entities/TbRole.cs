using System;
using System.Collections.Generic;

namespace UBIS.Access.Domain.Entities;

public partial class TbRole
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsDelete { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<TbRolePermission> TbRolePermissions { get; set; } = new List<TbRolePermission>();

    public virtual ICollection<TbUserRole> TbUserRoles { get; set; } = new List<TbUserRole>();
}
