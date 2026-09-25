using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class ApprovalRouteResolverService : IApprovalRouteResolverService
{
    private readonly IRouteApproveRepos _routeRepos;
    private readonly IEmployeeRepos _employeeRepos;
    private readonly IPodAdminBranchRepos _podAdminBranchRepos;

    public ApprovalRouteResolverService(IRouteApproveRepos routeRepos,
        IEmployeeRepos employeeRepos,
        IPodAdminBranchRepos podAdminBranchRepos)
    {
        _routeRepos = routeRepos;
        _employeeRepos = employeeRepos;
        _podAdminBranchRepos = podAdminBranchRepos;
    }

    public async Task<List<ResolvedApprover>> ResolveAsync(string docType, Guid employeeId)
    {
        var routes = await _routeRepos.GetActiveRouteAsync(docType);
        if (routes.Count == 0)
            throw new InvalidOperationException($"ยังไม่ได้ตั้งค่าสายอนุมัติสำหรับเอกสารประเภท {docType}");

        var flat = await _employeeRepos.GetOrgChartFlatDataAsync();
        var byId = flat.ToDictionary(x => x.Id);

        if (!byId.TryGetValue(employeeId, out var requester))
            throw new InvalidOperationException("ไม่พบข้อมูลพนักงานผู้ขอเบิก");

        var result = new List<ResolvedApprover>();

        foreach (var route in routes)
        {
            Guid approverId = route.ApproverType switch
            {
                "ManagerChain" => ResolveManagerChain(byId, requester, route.MinPositionLevel),
                "FixedEmployee" => route.FixedEmployeeId
                    ?? throw new InvalidOperationException($"Step {route.StepNo} ({route.StepName}) ไม่ได้ตั้งค่า FixedEmployeeId ไว้"),
                "BranchAdmin" => await ResolveBranchAdminAsync(requester.BranchId, route.StepName),
                _ => throw new InvalidOperationException($"ไม่รู้จัก ApproverType: {route.ApproverType}")
            };

            result.Add(new ResolvedApprover
            {
                StepNo = route.StepNo,
                StepName = route.StepName,
                ApproverEmployeeId = approverId,
            });
        }

        return result;
    }

    private Guid ResolveManagerChain(
        Dictionary<Guid, EmployeeOrgChartNodeDto> byId,
        EmployeeOrgChartNodeDto requester,
        int? minLevel)
    {
        var visited = new HashSet<Guid> { requester.Id };
        var currentReportToId = requester.ReportToId;

        while (currentReportToId.HasValue && byId.TryGetValue(currentReportToId.Value, out var manager))
        {
            if (!visited.Add(manager.Id))
                throw new InvalidOperationException("โครงสร้างสายบังคับบัญชาผิดพลาด (วนลูป)");

            if (!minLevel.HasValue || manager.PositionLevel >= minLevel.Value)
                return manager.Id;

            currentReportToId = manager.ReportToId;
        }

        throw new InvalidOperationException($"ไม่พบหัวหน้าที่ระดับตำแหน่งตั้งแต่ {minLevel} ขึ้นไปในสายบังคับบัญชา");
    }

    private async Task<Guid> ResolveBranchAdminAsync(Guid branchId, string stepName)
    {
        var admin = await _podAdminBranchRepos.GetPrimaryByBranchIdAsync(branchId);
        if (admin == null || admin.EmployeeId == null)
            throw new InvalidOperationException($"Step '{stepName}' ไม่พบผู้ดูแลสาขาหลัก (Primary) กรุณาตั้งค่าใน Master Data ก่อน");

        return admin.EmployeeId.Value;
    }
}