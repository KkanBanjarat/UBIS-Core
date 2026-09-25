using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbCompany
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string NameTh { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool IsSubsidiary { get; set; }

    public virtual ICollection<TbBranch> TbBranches { get; set; } = new List<TbBranch>();

    public virtual ICollection<TbEmployee> TbEmployees { get; set; } = new List<TbEmployee>();
}
