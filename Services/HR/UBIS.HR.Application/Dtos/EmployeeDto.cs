namespace UBIS.HR.Application.Dtos;


public class EmployeeFilterDto
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public Guid? EmployeeTypeId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DivisionId { get; set; }
    public Guid? SectionId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
public class EmployeeDto
{
    public Guid Id { get; set; }
    public string EmpId { get; set; }
    public string? PrefixNameTh { get; set; }
    public string? PrefixNameEn { get; set; }
    public string FNameTh { get; set; }
    public string LNameTh { get; set; }
    public string FNameEn { get; set; }
    public string LNameEn { get; set; }
    public string Email { get; set; }
    public DateTime HireDate { get; set; }
    public Guid PositionId { get; set; }
    public string PositionNameTh { get; set; }
    public string PositionNameEn { get; set; }
    public Guid PositionLevelId { get; set; }
    public int PositionLevel { get; set; }
    public string PositionLevelNameTh { get; set; }
    public string PositionLevelNameEn { get; set; }

    // Flat Organization Structure
    public Guid? CompanyId { get; set; }
    public string CompanyNameTh { get; set; }
    public string CompanyNameEn { get; set; }
    public string CompanyCode { get; set; }

    public Guid? GroupId { get; set; }
    public string? GroupNameTh { get; set; }
    public string? GroupNameEn { get; set; }

    public Guid? DepartmentId { get; set; }
    public string? DepartmentNameTh { get; set; }
    public string? DepartmentNameEn { get; set; }

    public Guid? DivisionId { get; set; }
    public string? DivisionNameTh { get; set; }
    public string? DivisionNameEn { get; set; }

    public Guid? SectionId { get; set; }
    public string? SectionNameTh { get; set; }
    public string? SectionNameEn { get; set; }

    public Guid EmployeeTypeId { get; set; }
    public string EmployeeTypeNameTh { get; set; }
    public string EmployeeTypeNameEn { get; set; }
    public Guid BranchId { get; set; }
    public string BranchCode { get; set; }
    public string BranchNameTh { get; set; }
    public string BranchNameEn { get; set; }
    public string Status { get; set; }
    public Guid? ReportToId { get; set; }
    public string? ReportToNameTh { get; set; }
    public string? ReportToNameEn { get; set; }
    public string? Gender { get; set; }
    public List<BenefitPlanSummaryDto> BenefitPlans { get; set; } = new();
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

}

public class EmployeeDetailDto : EmployeeDto
{
    public List<OrganizationUnitPathItemDto> OrganizationUnitPath { get; set; }
    public EmployeeOrgChartNodeDto? OrgChart { get; set; }
}

public class EmployeeOrgChartNodeDto
{
    public Guid Id { get; set; }
    public string EmpId { get; set; }
    public string FullNameTh { get; set; }
    public string FullNameEn { get; set; }
    public string? PositionNameTh { get; set; }
    public string? PositionNameEn { get; set; }
    public bool IsSelf { get; set; }
    public Guid? ReportToId { get; set; }
    public Guid BranchId { get; set; }
    public int PositionLevel { get; set; }   // ← เพิ่ม
    public List<EmployeeOrgChartNodeDto> Children { get; set; } = new();
}
public class CreateEmployeeDto
{
    public string EmpId { get; set; }
    public string? PrefixNameTh { get; set; }
    public string? PrefixNameEn { get; set; }
    public string FNameTh { get; set; }
    public string LNameTh { get; set; }
    public string FNameEn { get; set; }
    public string LNameEn { get; set; }
    public string Email { get; set; }
    public DateTime HireDate { get; set; }
    public Guid PositionId { get; set; }
    public Guid PositionLevelId { get; set; }

    // Required Flat Org Fields
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }

    // Optional Flat Org Fields
    public Guid? GroupId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DivisionId { get; set; }
    public Guid? SectionId { get; set; }

    public Guid EmployeeTypeId { get; set; }
    public string Status { get; set; }
    public string? Gender { get; set; }
    public Guid? ReportToId { get; set; }
    public List<Guid>? BenefitPlanIds { get; set; }
}

public class EmployeeSearchDto
{
    public Guid Id { get; set; }
    public string EmpId { get; set; }
    public string FirstNameTh { get; set; }
    public string LastNameTh { get; set; }
    public string FirstNameEn { get; set; }
    public string LastNameEn { get; set; }
    public string PositionNameTh { get; set; }
    public string PositionNameEn { get; set; }
    public string Status { get; set; }
}
public class AllowedEmployeeDto
{
    public Guid Id { get; set; }
    public string EmpId { get; set; }
    public string FullNameTh { get; set; }
    public string FullNameEn { get; set; }
    public string? PositionNameTh { get; set; }
}
public class SearchEmployeeRequestDto
{
    public string? Search { get; set; }
    public Guid? CompanyId { get; set; }
    public string? Status { get; set; } = "Active";
    public Guid? ExcludeId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}