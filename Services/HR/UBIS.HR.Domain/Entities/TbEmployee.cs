using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbEmployee
{
    public Guid Id { get; set; }

    public string EmpId { get; set; } = null!;

    public string FnameTh { get; set; } = null!;

    public string LnameTh { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime HireDate { get; set; }

    public Guid PositionId { get; set; }

    public Guid PositionLevelId { get; set; }

    public string Status { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public bool IsDelete { get; set; }

    public string FnameEn { get; set; } = null!;

    public string LnameEn { get; set; } = null!;

    public Guid? ReportToId { get; set; }

    public Guid EmployeeTypeId { get; set; }

    public string? PrefixNameTh { get; set; }

    public string? PrefixNameEn { get; set; }

    public string? Gender { get; set; }

    public Guid BranchId { get; set; }

    public Guid CompanyId { get; set; }

    public Guid? GroupId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? DivisionId { get; set; }

    public Guid? SectionId { get; set; }

    public virtual TbBranch Branch { get; set; } = null!;

    public virtual TbCompany Company { get; set; } = null!;

    public virtual TbOrganizationUnit? Department { get; set; }

    public virtual TbOrganizationUnit? Division { get; set; }

    public virtual TbEmployeeType EmployeeType { get; set; } = null!;

    public virtual TbOrganizationUnit? Group { get; set; }

    public virtual ICollection<TbEmployee> InverseReportTo { get; set; } = new List<TbEmployee>();

    public virtual TbPosition Position { get; set; } = null!;

    public virtual TbPositionLevel PositionLevel { get; set; } = null!;

    public virtual TbEmployee? ReportTo { get; set; }

    public virtual TbOrganizationUnit? Section { get; set; }

    public virtual ICollection<TbEmployeeBenefitPlan> TbEmployeeBenefitPlans { get; set; } = new List<TbEmployeeBenefitPlan>();

    public virtual ICollection<TbPrettyCashRequest> TbPrettyCashRequests { get; set; } = new List<TbPrettyCashRequest>();
}
