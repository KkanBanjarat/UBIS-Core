namespace UBIS.HR.Application.Dtos;

public class BenefitPlanDto
{
    public Guid Id { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

}
public class CreateBenefitPlanDto
{
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    
}