using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbPrettyCashLine
{
    public Guid Id { get; set; }

    public Guid PrettyCashId { get; set; }

    public Guid? BenefitId { get; set; }

    public string Detail { get; set; } = null!;

    public decimal LimitAmount { get; set; }

    public decimal Amount { get; set; }

    public decimal Qty { get; set; }

    public string? AccountCode { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TbBenefit? Benefit { get; set; }

    public virtual TbPrettyCashRequest PrettyCash { get; set; } = null!;
}
