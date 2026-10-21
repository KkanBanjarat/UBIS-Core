using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UBIS.HR.Infrastructure.Services.Teams;

public class TeamsNotifier : ITeamsNotifier
{
    private readonly HttpClient _http;
    private readonly TeamsOptions _opt;
    private readonly ILogger<TeamsNotifier> _logger;

    public TeamsNotifier(HttpClient http, IOptions<TeamsOptions> opt, ILogger<TeamsNotifier> logger)
    {
        _http = http;
        _opt = opt.Value;
        _logger = logger;
    }

    public async Task NotifyApprovalRequestAsync(ApprovalNotification n, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.WebhookUrl))
        {
            _logger.LogWarning("Teams:WebhookUrl is not configured. Skip notify {DocNumber}", n.DocNumber);
            return;
        }

        if (string.IsNullOrWhiteSpace(n.ApproverEmail))
        {
            _logger.LogWarning("Approver has no email. Skip notify {DocNumber}", n.DocNumber);
            return;
        }

        var url = $"{_opt.FrontendUrl.TrimEnd('/')}/approvals" +
                  $"?docType={Uri.EscapeDataString(n.DocType)}" +
                  $"&docNumber={Uri.EscapeDataString(n.DocNumber)}";

        var payload = new
        {
            approverEmail = n.ApproverEmail,
            step = $"ลำดับที่ {n.Step} จาก {n.TotalSteps}",
            docType = n.DocType,
            docNumber = n.DocNumber,
            requester = n.Requester,
            detail = string.IsNullOrWhiteSpace(n.Detail) ? "-" : n.Detail,
            docDate = n.DocDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            url
        };

        try
        {
            using var res = await _http.PostAsJsonAsync(_opt.WebhookUrl, payload, ct);
            if (!res.IsSuccessStatusCode)
                _logger.LogWarning("Teams notify failed {Status} for {DocNumber}", (int)res.StatusCode, n.DocNumber);
        }
        catch (Exception ex)
        {
            // ห้ามทำให้การส่งอนุมัติล้มเหลวเพราะ Teams ล่ม
            _logger.LogWarning(ex, "Teams notify error for {DocNumber}", n.DocNumber);
        }
    }
}