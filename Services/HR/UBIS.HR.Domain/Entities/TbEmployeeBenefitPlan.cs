using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbEmployeeBenefitPlan
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid BenefitPlanId { get; set; }

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TbBenefitPlan BenefitPlan { get; set; } = null!;

    public virtual TbEmployee Employee { get; set; } = null!;
}
