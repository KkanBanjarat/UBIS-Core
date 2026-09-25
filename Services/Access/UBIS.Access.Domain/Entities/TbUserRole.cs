using System;
using System.Collections.Generic;

namespace UBIS.Access.Domain.Entities;

public partial class TbUserRole
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public string Scope { get; set; } = null!;

    public bool IsDelete { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TbRole Role { get; set; } = null!;

    public virtual TbUser User { get; set; } = null!;
}
