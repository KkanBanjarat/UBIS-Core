namespace UBIS.HR.Application.Interfaces;

public interface IApprovalDocumentService
{
    string DocType { get; }
    Task<(DateTime DocDate, string EmployeeNameTh, string? DocumentDetail)?> GetSummaryAsync(string docNumber);
    Task<Dictionary<string, (DateTime DocDate, string EmployeeNameTh, string? DocumentDetail)>> GetSummariesAsync(IEnumerable<string> docNumbers);
    Task MarkApprovedAsync(string docNumber);
    Task MarkDeniedAsync(string docNumber, bool isDisapprove);
}