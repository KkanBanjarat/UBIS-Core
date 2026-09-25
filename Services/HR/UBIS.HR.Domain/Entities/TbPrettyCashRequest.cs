using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbPrettyCashRequest
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

    public virtual TbEmployee Employee { get; set; } = null!;

    public virtual ICollection<TbPrettyCashLine> TbPrettyCashLines { get; set; } = new List<TbPrettyCashLine>();
}
