using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbBenefitPlanItem
{
    public Guid Id { get; set; }

    public Guid BenefitPlanId { get; set; }

    public Guid BenefitId { get; set; }

    public decimal LimitAmount { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TbBenefit Benefit { get; set; } = null!;

    public virtual TbBenefitPlan BenefitPlan { get; set; } = null!;
}
