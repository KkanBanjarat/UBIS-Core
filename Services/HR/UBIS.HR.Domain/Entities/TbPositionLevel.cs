using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbPositionLevel
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string NameTh { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int Level { get; set; }

    public string Track { get; set; } = null!;

    public bool IsSubsidiary { get; set; }

    public bool IsActive { get; set; }

    public bool IsDelete { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public virtual ICollection<TbEmployee> TbEmployees { get; set; } = new List<TbEmployee>();
}
