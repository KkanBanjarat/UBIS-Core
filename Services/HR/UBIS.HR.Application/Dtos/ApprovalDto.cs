namespace UBIS.HR.Application.Dtos;

public class MyApprovalDto
{
    public int TransApproveId { get; set; }
    public string DocType { get; set; }
    public string DocNumber { get; set; }
    public int DocRev { get; set; }
    public int Round { get; set; }
    public int StepNo { get; set; }
    public DateTime DocDate { get; set; }
    public string EmployeeNameTh { get; set; }
    // public decimal TotalAmount { get; set; }
    public string? DocumentDetail { get; set; }
    public bool IsUnsupported { get; set; }
}

public class RejectApprovalDto
{
    public string Reason { get; set; }
    public bool IsDisapprove { get; set; }
}

public class ApprovalStepDto
{
    public int StepNo { get; set; }
    public string ApproverNameTh { get; set; }
    public string Status { get; set; }
    public string? ActualApproveNameTh { get; set; }
    public DateTime? ApprovedDate { get; set; }
}

public class ApprovalReasonDto
{
    public string Status { get; set; }
    public string Reason { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApprovalTrailDto
{
    public List<ApprovalStepDto> Steps { get; set; } = new();
    public List<ApprovalReasonDto> Reasons { get; set; } = new();
}