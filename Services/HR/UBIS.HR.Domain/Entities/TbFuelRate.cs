using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbFuelRate
{
    public int Id { get; set; }

    public string FuelType { get; set; } = null!;

    public decimal MinPrice { get; set; }

    public decimal MaxPrice { get; set; }

    public decimal RatePerKm { get; set; }

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
