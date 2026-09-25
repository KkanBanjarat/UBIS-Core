namespace UBIS.HR.Application.Dtos;

public class ResolvedApprover
{
    public int StepNo { get; set; }
    public string StepName { get; set; }
    public Guid ApproverEmployeeId { get; set; }
}

public class RouteApproveDto
{
    public Guid Id { get; set; }
    public string DocType { get; set; }
    public int StepNo { get; set; }
    public string StepName { get; set; }
    public string ApproverType { get; set; }
    public int? MinPositionLevel { get; set; }
    public Guid? FixedEmployeeId { get; set; }
    public string? FixedEmployeeNameTh { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public string? OrganizationUnitNameTh { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRouteApproveDto
{
    public string DocType { get; set; }
    public int StepNo { get; set; }
    public string StepName { get; set; }
    public string ApproverType { get; set; }
    public int? MinPositionLevel { get; set; }
    public Guid? FixedEmployeeId { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public bool IsActive { get; set; } = true;
}