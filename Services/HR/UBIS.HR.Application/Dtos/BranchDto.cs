namespace UBIS.HR.Application.Dtos;

public class BranchDto
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }
    public string CompanyCode { get; set; }
    public string CompanyNameTh { get; set; }
    public string CompanyNameEn { get; set; }
    public string CompanyGroupName { get; set; }
    public string Code { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class CreateBranchDto
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public bool IsActive { get; set; }   
}