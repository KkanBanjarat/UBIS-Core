using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbApprovalLimit
{
    public int Id { get; set; }

    public string DocType { get; set; } = null!;

    public int? DocFormId { get; set; }

    public decimal AmountFrom { get; set; }

    public decimal? AmountTo { get; set; }

    public string StepName { get; set; } = null!;

    public string ApproverType { get; set; } = null!;

    public int? MinPositionLevel { get; set; }

    public Guid? FixedEmployeeId { get; set; }

    public Guid? OrganizationUnitId { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TbDocForm? DocForm { get; set; }

    public virtual TbEmployee? FixedEmployee { get; set; }
}
