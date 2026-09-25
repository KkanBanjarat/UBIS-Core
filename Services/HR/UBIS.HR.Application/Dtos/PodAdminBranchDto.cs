namespace UBIS.HR.Application.Dtos;

public class PodAdminBranchDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public string BranchNameTh { get; set; }
    public string BranchNameEn { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePodAdminBranchDto
{
    public Guid EmployeeId { get; set; }
    public Guid BranchId { get; set; }
    public bool IsPrimary { get; set; }
}

public class PodAdminBranchGroupDto
{
    public Guid UserId { get; set; }
    public List<BranchSummaryDto> Branches { get; set; }
}

public class BranchSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
}

// ===== สำหรับหน้าจัดการผู้ดูแลสาขา (จัดกลุ่มตามสาขา) =====

public class BranchAdminGroupDto
{
    public Guid BranchId { get; set; }
    public string BranchCode { get; set; }
    public string BranchNameTh { get; set; }
    public string BranchNameEn { get; set; }
    public List<BranchAdminItemDto> Admins { get; set; } = new();
}

public class BranchAdminItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeNameTh { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
}