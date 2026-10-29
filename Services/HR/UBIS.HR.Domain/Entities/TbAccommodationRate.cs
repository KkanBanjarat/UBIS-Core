using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbAccommodationRate
{
    public int Id { get; set; }

    public int LevelFrom { get; set; }

    public int LevelTo { get; set; }

    public decimal DomesticRate { get; set; }

    public decimal InternationalRate { get; set; }

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
