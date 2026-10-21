using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;
using UBIS.HR.Infrastructure.Excel;

namespace UBIS.HR.Infrastructure.Services;

public class BenefitClaimService : IBenefitClaimService, IApprovalDocumentService
{
    private readonly IDocNumberService _docNumberService;
    private readonly IApprovalRouteResolverService _routeResolver;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITransactionService _tx;
    private readonly IBenefitClaimRepos _repos;
    private readonly IEmployeeRepos _employeeRepos;
    private readonly IPodAdminBranchRepos _podAdminBranchRepos;

    private readonly ILogger<BenefitClaimService> _logger;
    private readonly ICurrentUserService _currentUser;

    public string DocType => "BenefitClaim";

    public BenefitClaimService(IBenefitClaimRepos repos,
        IDocNumberService docNumberService,
        ITransactionService tx,
        IEmployeeRepos employeeRepos,
        IPodAdminBranchRepos podAdminBranchRepos,
        IApprovalRouteResolverService routeResolver,
        IServiceProvider serviceProvider,
        ICurrentUserService currentUser,
        ILogger<BenefitClaimService> logger)
    {
        _repos = repos;
        _docNumberService = docNumberService;
        _tx = tx;
        _employeeRepos = employeeRepos;
        _podAdminBranchRepos = podAdminBranchRepos;
        _routeResolver = routeResolver;
        _serviceProvider = serviceProvider;
        _currentUser = currentUser;
        _logger = logger;
    }

    private IApprovalService ApprovalService => _serviceProvider.GetRequiredService<IApprovalService>();

    private static BenefitClaimDto MapToDto(TbBenefitClaim s)
    {
        return new BenefitClaimDto
        {
            Id = s.Id,
            DocNum = s.DocNum,
            DocStatus = s.DocStatus,
            DocDate = s.DocDate.Date,
            EmployeeId = s.EmployeeId,
            EmployeeNameTh = s.Employee != null ? $"{s.Employee.FnameTh} {s.Employee.LnameTh}" : null,
            PositionTh = s.Employee?.Position?.NameTh,
            PositionEn = s.Employee?.Position?.NameEn,
            PositionLevel = s.Employee?.PositionLevel != null ? $"L{s.Employee.PositionLevel.Level}" : null,
            PositionLevelNameTh = s.Employee?.PositionLevel?.NameTh,
            PositionLevelNameEn = s.Employee?.PositionLevel?.NameEn,
            Branch = $"{s.Employee?.Branch.NameTh} ({s.Employee?.Branch.Code})",
            Company = s.Employee?.Company.NameTh,
            CompanyCode = s.Employee?.Company.Code,
            Remark = s.Remark,
            TotalAmount = s.TotalAmount,
            LineCount = s.TbBenefitClaimLines.Count,
            Lines = s.TbBenefitClaimLines.Select(l => new BenefitClaimLineDto
            {
                Id = l.Id,
                BenefitId = l.BenefitId,
                BenefitNameTh = l.Benefit?.NameTh,
                Detail = l.Detail,
                LimitAmount = l.LimitAmount,
                Amount = l.Amount,
                Qty = l.Qty,
                AccountCode = l.AccountCode,
            }).ToList(),
            CreatedBy = s.CreatedBy,
            CreatedAt = s.CreatedAt,
            UpdatedBy = s.UpdatedBy,
            UpdatedAt = s.UpdatedAt,
        };
    }

    private static void ValidateLines(CreateBenefitClaimDto data)
    {
        if (data.Lines.Count == 0)
            throw new InvalidOperationException("กรุณาเพิ่มรายการเบิกอย่างน้อย 1 รายการ");
        if (data.Lines.Any(l => l.BenefitId == Guid.Empty))
            throw new InvalidOperationException("ทุกรายการต้องระบุสวัสดิการ");
        if (data.Lines.Any(l => l.Amount <= 0))
            throw new InvalidOperationException("ยอดเบิกต้องมากกว่า 0");
    }

    private TbBenefitClaimLine NewLine(CreateBenefitClaimLineDto l)
    {
        var email = _currentUser.GetCurrentUserEmail();
        var now = DateTime.Now;
        return new TbBenefitClaimLine
        {
            BenefitId = l.BenefitId,
            Detail = l.Detail,
            LimitAmount = l.LimitAmount,
            Amount = l.Amount,
            Qty = l.Qty,
            AccountCode = l.AccountCode,
            CreatedAt = now,
            CreatedBy = email,
            UpdatedAt = now,
            UpdatedBy = email,
            IsDelete = false,
        };
    }

    public async Task<PagedResultDto<BenefitClaimDto>> GetAllAsync(BenefitClaimFilterDto filter)
    {
        var employeeCode = _currentUser.GetCurrentUserEmployeeCode();
        var currentEmployeeId = await _employeeRepos.GetIdByEmpIdAsync(employeeCode) ?? Guid.Empty;

        var adminBranches = await _podAdminBranchRepos.GetByUserIdWithBranchAsync(_currentUser.GetCurrentUserId());
        var adminBranchIds = adminBranches.Select(b => b.BranchId).ToList();

        filter.currentEmployeeId = currentEmployeeId;
        filter.currentUserEmail = _currentUser.GetCurrentUserEmail();

        var (items, totalCount) = await _repos.GetFilteredPagedAsync(filter, adminBranchIds);
        return new PagedResultDto<BenefitClaimDto> { Items = items.ToList(), TotalCount = totalCount };
    }

    public async Task<BenefitClaimDto?> GetByIdAsync(Guid id)
    {
        var s = await _repos.GetDetailByIdAsync(id);
        return s == null ? null : await WithAffiliationAsync(MapToDto(s));
    }

    public async Task<BenefitClaimDto?> GetByDocNumAsync(string docNum)
    {
        var s = await _repos.GetByDocNumAsync(docNum);
        return s == null ? null : await WithAffiliationAsync(MapToDto(s));
    }
    public async Task<BenefitClaimSummaryDto> GetSummaryAsync(BenefitClaimSummaryFilterDto filter)
    {
        var adminBranches = await _podAdminBranchRepos.GetByUserIdWithBranchAsync(_currentUser.GetCurrentUserId());
        var adminBranchIds = adminBranches.Select(b => b.BranchId).ToList();

        if (adminBranchIds.Count == 0)
            throw new UnauthorizedAccessException("เฉพาะ HR Admin เท่านั้นที่ดูรายงานสรุปได้");

        return await _repos.GetSummaryAsync(filter, adminBranchIds);
    }
    public async Task<List<BenefitClaimExportRowDto>> GetDetailRowsAsync(BenefitClaimSummaryFilterDto filter)
    {
        var adminBranchIds = await GetAdminBranchIdsOrThrowAsync();
        return await _repos.GetExportRowsAsync(filter, adminBranchIds);
    }
    public async Task<PagedResultDto<EmployeeSummaryDto>> GetEmployeeSummaryPagedAsync(
    BenefitClaimEmployeeFilterDto filter)
    {
        var adminBranchIds = await GetAdminBranchIdsOrThrowAsync();
        var (items, total) = await _repos.GetEmployeeSummaryPagedAsync(filter, adminBranchIds);
        return new PagedResultDto<EmployeeSummaryDto> { Items = items, TotalCount = total };
    }
    public async Task<List<BenefitClaimExportRowDto>> GetEmployeeRowsAsync(
    BenefitClaimEmployeeRowsFilterDto filter)
    {
        var adminBranchIds = await GetAdminBranchIdsOrThrowAsync();
        var rows = await _repos.GetExportRowsAsync(filter, adminBranchIds, filter.EmployeeId);
        return rows.OrderBy(x => x.DocDate).ThenBy(x => x.DocNum).ToList();
    }

    private async Task<List<Guid>> GetAdminBranchIdsOrThrowAsync()
    {
        var adminBranches = await _podAdminBranchRepos.GetByUserIdWithBranchAsync(_currentUser.GetCurrentUserId());
        var ids = adminBranches.Select(b => b.BranchId).ToList();
        if (ids.Count == 0)
            throw new UnauthorizedAccessException("เฉพาะ HR Admin เท่านั้นที่ดูรายงานสรุปได้");
        return ids;
    }

    public async Task<byte[]> ExportSummaryAsync(BenefitClaimSummaryFilterDto filter)
    {
        var adminBranchIds = await GetAdminBranchIdsOrThrowAsync();
        var summary = await _repos.GetSummaryAsync(filter, adminBranchIds);
        var rows = await _repos.GetExportRowsAsync(filter, adminBranchIds);
        return BenefitClaimSummaryExcelBuilder.Build(summary, rows, filter);
    }
    private async Task<BenefitClaimDto> WithAffiliationAsync(BenefitClaimDto dto)
    {
        var aff = await _employeeRepos.GetAffiliationAsync(dto.EmployeeId);
        if (aff != null)
        {
            dto.Affiliation = aff.Value.Name;
            dto.AffiliationCode = aff.Value.Code;
        }
        return dto;
    }

    public async Task<BenefitClaimDto> CreateAsync(CreateBenefitClaimDto data)
    {
        try
        {
            ValidateLines(data);

            var email = _currentUser.GetCurrentUserEmail();
            var now = DateTime.Now;

            var entity = new TbBenefitClaim
            {
                DocNum = await _docNumberService.GenerateAsync(DocType),
                DocStatus = "Draft",
                DocDate = data.DocDate.Date,
                EmployeeId = data.EmployeeId,
                Remark = data.Remark,
                TotalAmount = data.Lines.Sum(l => l.Amount),
                CreatedAt = now,
                CreatedBy = email,
                UpdatedAt = now,
                UpdatedBy = email,
                IsDelete = false,
                TbBenefitClaimLines = data.Lines.Select(NewLine).ToList(),
            };

            await _repos.AddAsync(entity);
            await _repos.SaveChangesAsync();

            return (await GetByIdAsync(entity.Id))!;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มใบเบิกสวัสดิการ: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitClaimDto?> UpdateAsync(Guid id, CreateBenefitClaimDto data)
    {
        try
        {
            var existing = await _repos.GetByIdAsync(id);
            if (existing == null) return null;

            if (existing.DocStatus != "Draft")
                throw new InvalidOperationException("แก้ไขได้เฉพาะเอกสารสถานะ Draft เท่านั้น กรุณาตรวจสอบ");

            ValidateLines(data);

            existing.DocDate = data.DocDate;
            existing.EmployeeId = data.EmployeeId;
            existing.Remark = data.Remark;
            existing.TotalAmount = data.Lines.Sum(l => l.Amount);
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.DeleteLinesAsync(id);

            foreach (var l in data.Lines)
                existing.TbBenefitClaimLines.Add(NewLine(l));

            await _repos.SaveChangesAsync();
            return await GetByIdAsync(id);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการแก้ไขใบเบิกสวัสดิการ: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var existing = await _repos.GetByIdAsync(id);
            if (existing == null) return false;

            if (existing.DocStatus != "Draft")
                throw new InvalidOperationException("ลบได้เฉพาะเอกสารสถานะ Draft เท่านั้น กรุณาตรวจสอบ");

            existing.IsDelete = true;
            existing.DeletedBy = _currentUser.GetCurrentUserEmail();
            existing.DeletedAt = DateTime.Now;

            await _repos.SaveChangesAsync();
            return true;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบใบเบิกสวัสดิการ: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitClaimDto?> SubmitAsync(Guid id)
    {
        try
        {
            var existing = await _repos.GetByIdAsync(id);
            if (existing == null) return null;

            if (existing.DocStatus != "Draft")
                throw new InvalidOperationException("ส่งอนุมัติได้เฉพาะเอกสารสถานะ Draft เท่านั้น");

            var resolvedApprovers = await _routeResolver.ResolveAsync(DocType, existing.EmployeeId);

            return await _tx.RunAsync<BenefitClaimDto?>(async () =>
            {
                existing.DocStatus = "WaitApprove";
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedBy = _currentUser.GetCurrentUserEmail();
                await _repos.SaveChangesAsync();

                await ApprovalService.CreateApprovalAsync(
                    DocType,
                    existing.DocNum,
                    1,
                    resolvedApprovers.OrderBy(x => x.StepNo).ToList());

                return await GetByIdAsync(id);
            });
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการส่งอนุมัติใบเบิกสวัสดิการ: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitClaimDto?> RecallAsync(Guid id)
    {
        try
        {
            var existing = await _repos.GetByIdAsync(id);
            if (existing == null) return null;

            if (existing.DocStatus != "WaitApprove" && existing.DocStatus != "Rejected")
                throw new InvalidOperationException("ดึงกลับได้เฉพาะเอกสารสถานะ รออนุมัติ หรือ ตีกลับ เท่านั้น");

            return await _tx.RunAsync<BenefitClaimDto?>(async () =>
            {
                existing.DocStatus = "Draft";
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedBy = _currentUser.GetCurrentUserEmail();
                await _repos.SaveChangesAsync();

                await ApprovalService.RecallAsync(DocType, existing.DocNum, 1);

                return await GetByIdAsync(id);
            });
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงใบเบิกสวัสดิการกลับ: {Message}", ex.Message);
            throw;
        }
    }
    // ===== IApprovalDocumentService =====

    public async Task<(DateTime DocDate, string EmployeeNameTh, string? DocumentDetail)?> GetSummaryAsync(string docNumber)
    {
        var entity = await _repos.GetByDocNumAsync(docNumber);
        if (entity == null) return null;

        var name = entity.Employee != null ? $"{entity.Employee.FnameTh} {entity.Employee.LnameTh}" : "-";
        return (entity.DocDate, name, $"เอกสารเบิกสวัสดิการ จำนวนเงินรวม {entity.TotalAmount} บาท");
    }

    public async Task<Dictionary<string, (DateTime DocDate, string EmployeeNameTh, string? DocumentDetail)>> GetSummariesAsync(IEnumerable<string> docNumbers)
    {
        var entities = await _repos.GetSummariesByDocNumsAsync(docNumbers);

        return entities.ToDictionary(
            e => e.DocNum,
            e => (
                e.DocDate,
                e.Employee != null ? $"{e.Employee.FnameTh} {e.Employee.LnameTh}" : "-",
                (string?)$"เอกสารเบิกสวัสดิการ จำนวนเงินรวม {e.TotalAmount} บาท"
            ));
    }

    public async Task MarkApprovedAsync(string docNumber)
    {
        var entity = await _repos.GetByDocNumAsync(docNumber);
        if (entity == null) return;

        entity.DocStatus = "Approved";
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = _currentUser.GetCurrentUserEmail();
        await _repos.SaveChangesAsync();
    }

    public async Task MarkDeniedAsync(string docNumber, bool isDisapprove)
    {
        var entity = await _repos.GetByDocNumAsync(docNumber);
        if (entity == null) return;

        entity.DocStatus = isDisapprove ? "Disapproved" : "Rejected";
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = _currentUser.GetCurrentUserEmail();
        await _repos.SaveChangesAsync();
    }
}