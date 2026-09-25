namespace UBIS.HR.Application.Interfaces;

public interface IApprovalDocumentService
{
    string DocType { get; }
    Task<(DateTime DocDate, string EmployeeNameTh, string? DocumentDetail)?> GetSummaryAsync(string docNumber);
    Task MarkApprovedAsync(string docNumber);
    Task MarkDeniedAsync(string docNumber, bool isDisapprove);
}

