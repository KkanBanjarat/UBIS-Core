namespace UBIS.HR.Application.Dtos;

public class PositionDto
{
    public Guid Id { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public bool IsSubsidiary  { get; set; }
    public bool IsActive  { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

}
public class CreatePositionDto
{
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public bool IsSubsidiary  { get; set; }
    public bool IsActive  { get; set; }
    
}

public class PositionFilterDto
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}