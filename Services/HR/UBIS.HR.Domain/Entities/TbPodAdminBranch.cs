using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbPodAdminBranch
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid BranchId { get; set; }

    public bool IsDelete { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public bool IsPrimary { get; set; }

    public Guid? EmployeeId { get; set; }

    public virtual TbBranch Branch { get; set; } = null!;
}
