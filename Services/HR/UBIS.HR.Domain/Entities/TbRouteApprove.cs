using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbRouteApprove
{
    public Guid Id { get; set; }

    public string DocType { get; set; } = null!;

    public int StepNo { get; set; }

    public string StepName { get; set; } = null!;

    public string ApproverType { get; set; } = null!;

    public int? MinPositionLevel { get; set; }

    public Guid? FixedEmployeeId { get; set; }

    public Guid? OrganizationUnitId { get; set; }

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }
}
