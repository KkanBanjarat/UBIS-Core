using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbBenefitPlan
{
    public Guid Id { get; set; }

    public string NameTh { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<TbBenefitPlanItem> TbBenefitPlanItems { get; set; } = new List<TbBenefitPlanItem>();

    public virtual ICollection<TbEmployeeBenefitPlan> TbEmployeeBenefitPlans { get; set; } = new List<TbEmployeeBenefitPlan>();
}
