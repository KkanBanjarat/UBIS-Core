using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbReasonApprove
{
    public int Id { get; set; }

    public string DocType { get; set; } = null!;

    public string DocNumber { get; set; } = null!;

    public int DocRev { get; set; }

    public int Round { get; set; }

    public string Status { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }
}
