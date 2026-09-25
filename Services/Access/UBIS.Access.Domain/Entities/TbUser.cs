using System;
using System.Collections.Generic;

namespace UBIS.Access.Domain.Entities;

public partial class TbUser
{
    public Guid Id { get; set; }

    public string? EntraObjectId { get; set; }

    public string Email { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public Guid? EmployeeId { get; set; }

    public string? EmployeeCode { get; set; }

    public string? PasswordHash { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public bool IsDelete { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<TbUserRole> TbUserRoles { get; set; } = new List<TbUserRole>();
}
