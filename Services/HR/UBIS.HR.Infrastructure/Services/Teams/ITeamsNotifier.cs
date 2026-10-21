namespace UBIS.HR.Infrastructure.Services.Teams;

public record ApprovalNotification(
    string ApproverEmail,
    string DocType,
    string DocNumber,
    string Requester,
    string Detail,
    DateTime DocDate,
    int Step,
    int TotalSteps);
public interface ITeamsNotifier
{
    Task NotifyApprovalRequestAsync(ApprovalNotification n, CancellationToken ct = default);
}