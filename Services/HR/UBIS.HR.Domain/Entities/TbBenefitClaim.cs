using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbBenefitClaim
{
    public Guid Id { get; set; }

    public string DocNum { get; set; } = null!;

    public string DocStatus { get; set; } = null!;

    public DateTime DocDate { get; set; }

    public Guid EmployeeId { get; set; }

    public string? Remark { get; set; }

    public decimal TotalAmount { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int DocFormId { get; set; }

    public int PositionLevel { get; set; }

    public string? BankAccountNo { get; set; }

    public string? PhoneNo { get; set; }

    public string? TripPurpose { get; set; }

    public string? Destination { get; set; }

    public string? TripType { get; set; }

    public DateTime? DepartAt { get; set; }

    public DateTime? ReturnAt { get; set; }

    public bool? IsOvernight { get; set; }

    public virtual TbDocForm DocForm { get; set; } = null!;

    public virtual TbEmployee Employee { get; set; } = null!;

    public virtual ICollection<TbBenefitClaimLine> TbBenefitClaimLines { get; set; } = new List<TbBenefitClaimLine>();
}
