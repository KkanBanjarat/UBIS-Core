using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbDocForm
{
    public int Id { get; set; }

    public string DocType { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string NameTh { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? Layout { get; set; }

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<TbApprovalLimit> TbApprovalLimits { get; set; } = new List<TbApprovalLimit>();

    public virtual ICollection<TbBenefitClaim> TbBenefitClaims { get; set; } = new List<TbBenefitClaim>();

    public virtual ICollection<TbBenefit> TbBenefits { get; set; } = new List<TbBenefit>();
}
