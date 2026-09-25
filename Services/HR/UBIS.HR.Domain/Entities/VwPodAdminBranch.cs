using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class VwPodAdminBranch
{
    public Guid? Id { get; set; }

    public string? UserEmail { get; set; }

    public string? UserName { get; set; }

    public string? BranchCode { get; set; }

    public string? BranchNameTh { get; set; }

    public string? BranchNameEn { get; set; }

    public bool? IsPrimary { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }
}
