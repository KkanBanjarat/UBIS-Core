using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbPerDiemRate
{
    public int Id { get; set; }

    public string Region { get; set; } = null!;

    public int DayFrom { get; set; }

    public int? DayTo { get; set; }

    public decimal RatePerDay { get; set; }

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
}
