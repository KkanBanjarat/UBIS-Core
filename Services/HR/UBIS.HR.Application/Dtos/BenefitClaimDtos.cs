namespace UBIS.HR.Application.Dtos;

public class BenefitClaimLineDto
{
    public Guid Id { get; set; }
    public Guid BenefitId { get; set; }
    public string? BenefitNameTh { get; set; }
    public string Detail { get; set; }
    public decimal LimitAmount { get; set; }
    public decimal Amount { get; set; }
    public decimal Qty { get; set; }
    public string? AccountCode { get; set; }
}

public class BenefitClaimDto
{
    public Guid Id { get; set; }
    public string DocNum { get; set; }
    public string DocStatus { get; set; }
    public DateTime DocDate { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeNameTh { get; set; }
    public string? Affiliation { get; set; }
    public string? AffiliationCode { get; set; }
    public string? PositionTh { get; set; }
    public string? PositionEn { get; set; }
    public string? PositionLevel { get; set; }
    public string? PositionLevelNameTh { get; set; }
    public string? PositionLevelNameEn { get; set; }
    public string? Company { get; set; }
    public string? CompanyCode { get; set; }
    public string? Branch { get; set; }
    public string? Remark { get; set; }
    public decimal TotalAmount { get; set; }
    public int LineCount { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<BenefitClaimLineDto> Lines { get; set; } = new();
}

public class CreateBenefitClaimLineDto
{
    public Guid BenefitId { get; set; }          // บังคับทุกบรรทัด
    public string Detail { get; set; }
    public decimal LimitAmount { get; set; }
    public decimal Amount { get; set; }
    public decimal Qty { get; set; } = 1;
    public string? AccountCode { get; set; }
}

public class CreateBenefitClaimDto
{
    public DateTime DocDate { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Remark { get; set; }
    public List<CreateBenefitClaimLineDto> Lines { get; set; } = new();
}

public class BenefitClaimFilterDto
{
    public string? Search { get; set; }
    public string? DocStatus { get; set; }
    public Guid currentEmployeeId { get; set; }
    public string? currentUserEmail { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class BenefitClaimSummaryFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public Guid? CompanyId { get; set; }
    public string? DocStatus { get; set; }
}



public class BenefitClaimSummaryDto
{
    public List<CompanySummaryDto> ByCompany { get; set; } = new();
}

public class BenefitClaimExportRowDto
{
    public string Company { get; set; } = "";
    public string DocNum { get; set; } = "";
    public DateTime DocDate { get; set; }
    public string DocStatus { get; set; } = "";
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";
    public string BenefitName { get; set; } = "";
    public string? Detail { get; set; }
    public decimal Qty { get; set; }
    public decimal Amount { get; set; }
}

public class CompanyBenefitDto
{
    public string Benefit { get; set; } = "";
    public int DocCount { get; set; }
    public int LineCount { get; set; }
    public decimal Amount { get; set; }
}

public class ClaimBranchSummaryDto
{
    public string Branch { get; set; } = "";
    public int DocCount { get; set; }
    public int LineCount { get; set; }
    public decimal Amount { get; set; }
    public List<CompanyBenefitDto> Benefits { get; set; } = new();
}

public class CompanySummaryDto
{
    public string Company { get; set; } = "";
    public int DocCount { get; set; }
    public int LineCount { get; set; }
    public decimal Amount { get; set; }
    public List<ClaimBranchSummaryDto> Branches { get; set; } = new();
}

public class BenefitClaimEmployeeFilterDto : BenefitClaimSummaryFilterDto
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class EmployeeSummaryDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";
    public string Company { get; set; } = "";
    public int DocCount { get; set; }
    public decimal Amount { get; set; }
}
public class BenefitClaimEmployeeRowsFilterDto : BenefitClaimSummaryFilterDto
{
    public Guid EmployeeId { get; set; }
}