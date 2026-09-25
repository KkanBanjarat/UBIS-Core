using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbTransApprove
{
    public int Id { get; set; }

    public string DocType { get; set; } = null!;

    public string DocNumber { get; set; } = null!;

    public int DocRev { get; set; }

    public int Round { get; set; }

    public int StepNo { get; set; }

    public Guid ApproverId { get; set; }

    public string Status { get; set; } = null!;

    public Guid? ActualApproveId { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }
}
