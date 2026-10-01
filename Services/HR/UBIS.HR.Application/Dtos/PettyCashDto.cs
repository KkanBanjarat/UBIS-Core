namespace UBIS.HR.Application.Dtos;

public class PettyCashLineDto
{
    public Guid Id { get; set; }
    public Guid? BenefitId { get; set; }
    public string? BenefitNameTh { get; set; }
    public string Detail { get; set; }
    public decimal LimitAmount { get; set; }
    public decimal Amount { get; set; }
    public decimal Qty { get; set; }
    public string? AccountCode { get; set; }
}

public class PettyCashDto
{
    public Guid Id { get; set; }
    public string DocNum { get; set; }
    public string DocStatus { get; set; }
    public DateTime DocDate { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeNameTh { get; set; }
    public string? Remark { get; set; }
    public decimal TotalAmount { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int LineCount { get; set; }
    public bool HasBenefitLine { get; set; }
    public bool HasCashLine { get; set; }
    public List<PettyCashLineDto> Lines { get; set; } = new();
}

public class CreatePettyCashLineDto
{
    public Guid? BenefitId { get; set; }
    public string Detail { get; set; }
    public decimal LimitAmount { get; set; }
    public decimal Amount { get; set; }
    public decimal Qty { get; set; } = 1;
    public string? AccountCode { get; set; }
}

public class CreatePettyCashDto
{
    public DateTime DocDate { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Remark { get; set; }
    public List<CreatePettyCashLineDto> Lines { get; set; } = new();
}

public class PettyCashFilterDto
{
    public string? Search { get; set; }
    public string? DocStatus { get; set; }
    public Guid currentEmployeeId { get; set; }
    public string? currentUserEmail { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

}