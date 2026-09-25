namespace UBIS.HR.Application.Dtos;

public class BenefitPlanItemDto
{
    public Guid Id { get; set; }
    public Guid BenefitPlanId { get; set; }
    public string BenefitPlanNameTh {get; set;}
    public string BenefitPlanNameEn {get; set;}
    public Guid BenefitId { get; set; }
    public string BenefitNameTh {get; set;}
    public string BenefitNameEn {get; set;}
    public decimal LimitAmount { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class CreateBenefitPlanItemDto
{
   public Guid BenefitPlanId { get; set; }
    public Guid BenefitId { get; set; }
    public decimal LimitAmount { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    
}