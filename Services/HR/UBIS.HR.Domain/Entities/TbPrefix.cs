using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbPrefix
{
    public Guid Id { get; set; }

    public string DocType { get; set; } = null!;

    public string Prefix { get; set; } = null!;

    public string? DateFormat { get; set; }

    public int RunningLength { get; set; }

    public string ResetPeriod { get; set; } = null!;

    public string? LastResetKey { get; set; }

    public int LastRunningNumber { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }
}
