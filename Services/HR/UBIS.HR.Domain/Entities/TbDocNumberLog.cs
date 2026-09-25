using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbDocNumberLog
{
    public long Id { get; set; }

    public string DocType { get; set; } = null!;

    public string DocNumber { get; set; } = null!;

    public DateTime GeneratedAt { get; set; }

    public string GeneratedBy { get; set; } = null!;
}
