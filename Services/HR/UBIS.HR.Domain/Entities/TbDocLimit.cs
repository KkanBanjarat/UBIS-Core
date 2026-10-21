using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UBIS.HR.Domain.Entities;

public partial class TbDocLimit
{
    public Guid Id { get; set; }

    public string DocType { get; set; } = null!;

    public decimal LimitAmount { get; set; }

    [Column(TypeName = "date")]
    public DateTime EffectiveFrom { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public bool IsDelete { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }
}
