using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class VwTransApprove
{
    public int? Id { get; set; }

    public string? DocType { get; set; }

    public string? DocNumber { get; set; }

    public string? RequesterEmpId { get; set; }

    public string? RequesterNameTh { get; set; }

    public int? DocRev { get; set; }

    public int? Round { get; set; }

    public int? StepNo { get; set; }

    public string? ApproverEmpId { get; set; }

    public string? ApproverNameTh { get; set; }

    public string? Status { get; set; }

    public string? ActualApproveEmpId { get; set; }

    public string? ActualApproveNameTh { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
