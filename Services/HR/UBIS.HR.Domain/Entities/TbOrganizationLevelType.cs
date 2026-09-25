using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbOrganizationLevelType
{
    public Guid Id { get; set; }

    public string NameTh { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int Sequence { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public bool IsDelete { get; set; }
}
