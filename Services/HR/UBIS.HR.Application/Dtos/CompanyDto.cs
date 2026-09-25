namespace UBIS.HR.Application.Dtos;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public string GroupName { get; set; }
    public bool IsActive { get; set; }
    public bool IsSubsidiary { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

}
public class CreateCompanyDto
{
    public string Code { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public string GroupName { get; set; }
    public bool IsActive { get; set; }    
}