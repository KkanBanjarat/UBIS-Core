using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;
using UBIS.HR.Infrastructure.Services.Teams;

namespace UBIS.HR.Infrastructure.Services;

public class ApprovalService : IApprovalService
{
    private readonly IApprovalRepos _approvalRepos;
    private readonly IReasonApproveRepos _reasonApproveRepos;
    private readonly IEmployeeRepos _employeeRepos;
    private readonly IEnumerable<IApprovalDocumentService> _documentServices;
    private readonly ITeamsNotifier _teamsNotifier;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ApprovalService> _logger;

    public ApprovalService(IApprovalRepos approvalRepos,
        IReasonApproveRepos reasonApproveRepos,
        IEmployeeRepos employeeRepos,
        IEnumerable<IApprovalDocumentService> documentServices,
        ITeamsNotifier teamsNotifier,
        ICurrentUserService currentUser,
        ILogger<ApprovalService> logger)
    {
        _approvalRepos = approvalRepos;
        _reasonApproveRepos = reasonApproveRepos;
        _employeeRepos = employeeRepos;
        _documentServices = documentServices;
        _teamsNotifier = teamsNotifier;
        _currentUser = currentUser;
        _logger = logger;
    }

    private IApprovalDocumentService GetDocumentService(string docType)
    {
        return TryGetDocumentService(docType)
            ?? throw new InvalidOperationException($"ไม่รู้จัก DocType: {docType}");
    }

    private IApprovalDocumentService? TryGetDocumentService(string docType)
    {
        return _documentServices.FirstOrDefault(x => x.DocType == docType);
    }
    private async Task NotifyApproverAsync(string docType, string docNumber, Guid approverId, int stepNo, int totalSteps)
    {
        try
        {
            var emails = await _employeeRepos.GetEmailsByIdsAsync(new List<Guid> { approverId });
            if (!emails.TryGetValue(approverId, out var email) || string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("ผู้อนุมัติ {ApproverId} ไม่มีอีเมล ข้ามการแจ้งเตือน {DocNumber}", approverId, docNumber);
                return;
            }

            var docService = TryGetDocumentService(docType);
            if (docService == null) return;

            var batch = await docService.GetSummariesAsync(new[] { docNumber });
            if (!batch.TryGetValue(docNumber, out var summary))
            {
                _logger.LogWarning("ไม่พบสรุปเอกสาร {DocType} {DocNumber} ข้ามการแจ้งเตือน", docType, docNumber);
                return;
            }

            var (docDate, employeeNameTh, detail) = summary;
            await _teamsNotifier.NotifyApprovalRequestAsync(new ApprovalNotification(
                email, docType, docNumber, employeeNameTh, detail ?? "-", docDate, stepNo, totalSteps));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "แจ้งเตือนผู้อนุมัติไม่สำเร็จ {DocNumber}", docNumber);
        }
    }
    public async Task CreateApprovalAsync(string docType, string docNumber, int docRev, List<ResolvedApprover> steps)
    {
        var round = await _approvalRepos.GetLatestRoundAsync(docType, docNumber, docRev) + 1;
        var currentUserEmail = _currentUser.GetCurrentUserEmail();
        var now = DateTime.Now;

        var stepNo = 0;
        foreach (var step in steps)
        {
            stepNo++;
            await _approvalRepos.AddAsync(new TbTransApprove
            {
                DocType = docType,
                DocNumber = docNumber,
                DocRev = docRev,
                Round = round,
                StepNo = stepNo,
                StepName = step.StepName,
                ApproverType = step.ApproverType,
                ApproverId = step.ApproverEmployeeId,
                Status = stepNo == 1 ? "WaitApprove" : "Pending",
                CreatedAt = now,
                CreatedBy = currentUserEmail,
                UpdatedAt = now,
                UpdatedBy = currentUserEmail,
            });
        }

        await _approvalRepos.SaveChangesAsync();
        // throw new Exception("ทดสอบ rollback");
        if (steps.Count > 0)
            await NotifyApproverAsync(docType, docNumber, steps[0].ApproverEmployeeId, 1, steps.Count);
    }

    public async Task<List<MyApprovalDto>> GetMyPendingApprovalsAsync()
    {
        var employeeCode = _currentUser.GetCurrentUserEmployeeCode();
        var currentEmployeeId = await _employeeRepos.GetIdByEmpIdAsync(employeeCode);
        if (currentEmployeeId == null) return new List<MyApprovalDto>();

        var pendingSteps = await _approvalRepos.GetPendingByApproverAsync(currentEmployeeId.Value);
        var result = new List<MyApprovalDto>();

        // ดึงสรุปเอกสารเป็นชุดเดียวต่อ DocType แทนการยิงทีละใบ
        var summaries = new Dictionary<(string DocType, string DocNumber),
            (DateTime DocDate, string EmployeeNameTh, string? DocumentDetail)>();

        foreach (var group in pendingSteps.GroupBy(s => s.DocType))
        {
            var docService = TryGetDocumentService(group.Key);
            if (docService == null)
            {
                _logger.LogWarning("ไม่พบ Document Service สำหรับ DocType: {DocType} ข้าม {Count} รายการ",
                    group.Key, group.Count());
                continue;
            }

            var batch = await docService.GetSummariesAsync(group.Select(s => s.DocNumber));
            foreach (var kv in batch)
                summaries[(group.Key, kv.Key)] = kv.Value;
        }

        foreach (var step in pendingSteps)
        {
            if (!summaries.TryGetValue((step.DocType, step.DocNumber), out var summary))
                continue;

            result.Add(new MyApprovalDto
            {
                TransApproveId = step.Id,
                DocType = step.DocType,
                DocNumber = step.DocNumber,
                DocRev = step.DocRev,
                Round = step.Round,
                StepNo = step.StepNo,
                DocDate = summary.DocDate,
                EmployeeNameTh = summary.EmployeeNameTh,
                DocumentDetail = summary.DocumentDetail
            });
        }

        return result;
    }

    public async Task<List<ApprovalSummaryDto>> GetPendingApprovalsAsync(Guid actor)
    {
        try
        {
            var pendingSteps = await _approvalRepos.GetPendingByApproverAsync(actor);
            if (!pendingSteps.Any())
                return new List<ApprovalSummaryDto>();

            var approvalSummary = pendingSteps
                .GroupBy(s => s.DocType)
                .Select(g => new ApprovalSummaryDto
                {
                    DocType = g.Key,
                    QtyWaitApprove = g.Select(x => x.DocNumber).Distinct().Count(),
                })
                .OrderBy(x => x.DocType)
                .ToList();

            return approvalSummary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Approval : {Message}", ex.Message);
            throw;
        }
    }
    public async Task<bool> ApproveAsync(int transApproveId)
    {
        var step = await _approvalRepos.GetByIntIdAsync(transApproveId);
        if (step == null) return false;

        if (step.Status != "WaitApprove")
            throw new InvalidOperationException("รายการนี้ไม่ได้อยู่ในสถานะรออนุมัติแล้ว");

        var employeeCode = _currentUser.GetCurrentUserEmployeeCode();
        var currentEmployeeId = await _employeeRepos.GetIdByEmpIdAsync(employeeCode);
        if (currentEmployeeId != step.ApproverId)
            throw new InvalidOperationException("คุณไม่มีสิทธิ์อนุมัติรายการนี้");

        var currentUserEmail = _currentUser.GetCurrentUserEmail();

        step.Status = "Approved";
        step.ActualApproveId = currentEmployeeId;
        step.ApprovedDate = DateTime.Now;
        step.UpdatedAt = DateTime.Now;
        step.UpdatedBy = currentUserEmail;
        await _approvalRepos.SaveChangesAsync();

        var allSteps = await _approvalRepos.GetByDocumentAsync(step.DocType, step.DocNumber, step.DocRev, step.Round);
        var nextStep = allSteps.FirstOrDefault(x => x.StepNo == step.StepNo + 1);

        if (nextStep != null)
        {
            nextStep.Status = "WaitApprove";
            nextStep.UpdatedAt = DateTime.Now;
            nextStep.UpdatedBy = currentUserEmail;
            await _approvalRepos.SaveChangesAsync();
            await NotifyApproverAsync(step.DocType, step.DocNumber, nextStep.ApproverId, nextStep.StepNo, allSteps.Count);
        }
        else
        {
            await GetDocumentService(step.DocType).MarkApprovedAsync(step.DocNumber);
        }

        return true;
    }

    public async Task<bool> DenyAsync(int transApproveId, string reason, bool isDisapprove)
    {
        var denialStatus = isDisapprove ? "Disapproved" : "Rejected";
        var step = await _approvalRepos.GetByIntIdAsync(transApproveId);
        if (step == null) return false;

        if (step.Status != "WaitApprove")
            throw new InvalidOperationException("รายการนี้ไม่ได้อยู่ในสถานะรออนุมัติแล้ว");

        var employeeCode = _currentUser.GetCurrentUserEmployeeCode();
        var currentEmployeeId = await _employeeRepos.GetIdByEmpIdAsync(employeeCode);
        if (currentEmployeeId != step.ApproverId)
            throw new InvalidOperationException($"คุณไม่มีสิทธิ์{(isDisapprove ? "ไม่อนุมัติ" : "ตีกลับ")}รายการนี้");

        var currentUserEmail = _currentUser.GetCurrentUserEmail();

        step.Status = denialStatus;
        step.ActualApproveId = currentEmployeeId;
        step.ApprovedDate = DateTime.Now;
        step.UpdatedAt = DateTime.Now;
        step.UpdatedBy = currentUserEmail;
        await _approvalRepos.SaveChangesAsync();

        var allSteps = await _approvalRepos.GetByDocumentAsync(step.DocType, step.DocNumber, step.DocRev, step.Round);
        foreach (var remaining in allSteps.Where(x => x.StepNo > step.StepNo))
        {
            remaining.Status = denialStatus;
            remaining.UpdatedAt = DateTime.Now;
            remaining.UpdatedBy = currentUserEmail;
        }
        await _approvalRepos.SaveChangesAsync();

        await _reasonApproveRepos.AddAsync(new TbReasonApprove
        {
            DocType = step.DocType,
            DocNumber = step.DocNumber,
            DocRev = step.DocRev,
            Round = step.Round,
            Status = denialStatus,
            Reason = reason,
            CreatedAt = DateTime.Now,
            CreatedBy = currentUserEmail,
            UpdatedAt = DateTime.Now,
            UpdatedBy = currentUserEmail,
        });
        await _reasonApproveRepos.SaveChangesAsync();

        await GetDocumentService(step.DocType).MarkDeniedAsync(step.DocNumber, isDisapprove);

        return true;
    }

    public async Task RecallAsync(string docType, string docNumber, int docRev)
    {
        var round = await _approvalRepos.GetLatestRoundAsync(docType, docNumber, docRev);
        if (round == 0) return;

        var currentUserEmail = _currentUser.GetCurrentUserEmail();
        var steps = await _approvalRepos.GetByDocumentAsync(docType, docNumber, docRev, round);

        foreach (var step in steps.Where(x => x.Status == "WaitApprove" || x.Status == "Pending"))
        {
            step.Status = "Recalled";
            step.UpdatedAt = DateTime.Now;
            step.UpdatedBy = currentUserEmail;
        }

        await _approvalRepos.SaveChangesAsync();
    }

    public async Task<ApprovalTrailDto> GetTrailAsync(string docType, string docNumber)
    {
        var round = await _approvalRepos.GetLatestRoundAsync(docType, docNumber, 1);
        var steps = round == 0
            ? new List<TbTransApprove>()
            : await _approvalRepos.GetByDocumentAsync(docType, docNumber, 1, round);

        var reasons = round == 0
            ? new List<TbReasonApprove>()
            : await _reasonApproveRepos.GetByDocumentAsync(docType, docNumber, 1);

        var employeeIds = steps
            .SelectMany(s => new[] { s.ApproverId, s.ActualApproveId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var nameMap = employeeIds.Count > 0
            ? await _employeeRepos.GetNamesByIdsAsync(employeeIds)
            : new Dictionary<Guid, string>();

        var positionMap = employeeIds.Count > 0
        ? await _employeeRepos.GetPositionNamesByIdsAsync(employeeIds)
        : new Dictionary<Guid, string>();

        return new ApprovalTrailDto
        {
            Steps = steps.Select(s => new ApprovalStepDto
            {
                StepNo = s.StepNo,
                ApproverNameTh = nameMap.GetValueOrDefault(s.ApproverId, "-"),
                Status = s.Status,
                ActualApproveNameTh = s.ActualApproveId.HasValue ? nameMap.GetValueOrDefault(s.ActualApproveId.Value, "-") : null,
                ApproverPosition = positionMap.GetValueOrDefault(s.ApproverId, ""),
                ActualApprovePosition = s.ActualApproveId.HasValue
                ? positionMap.GetValueOrDefault(s.ActualApproveId.Value, "")
                : null,
                ApprovedDate = s.ApprovedDate,
                ApproverType = s.ApproverType,
                StepName = s.StepName
            }).ToList(),
            Reasons = reasons.Select(r => new ApprovalReasonDto
            {
                Status = r.Status,
                Reason = r.Reason,
                CreatedBy = r.CreatedBy,
                CreatedAt = r.CreatedAt,
            }).ToList(),
        };
    }
}