namespace UBIS.HR.Application.Dtos;

public class BenefitDto
{
    public Guid Id { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

}
public class CreateBenefitDto
{
     public string NameTh { get; set; }
    public string NameEn { get; set; }
    public bool IsActive { get; set; }
    
}

public class BenefitPlanItemSummaryDto
{
    public Guid BenefitId { get; set; }
    public string BenefitNameTh { get; set; }
    public string BenefitNameEn { get; set; }
    public decimal LimitAmount { get; set; }
    public string? Description { get; set; }
}

public class BenefitPlanSummaryDto
{
    public Guid Id { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public List<BenefitPlanItemSummaryDto> Items { get; set; } = new();
}