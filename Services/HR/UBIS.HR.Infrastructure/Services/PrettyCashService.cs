using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class PrettyCashService : IPrettyCashService
{
    private readonly IPrettyCashRepos _repos;
    private readonly IDocNumberService _docNumberService;
    private readonly IEmployeeRepos _employeeRepos;
    private readonly IPodAdminBranchRepos _podAdminBranchRepos;
    private readonly IApprovalRouteResolverService _routeResolver;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PrettyCashService> _logger;
    private readonly ICurrentUserService _currentUser;

    public string DocType => "PrettyCash";

    public PrettyCashService(IPrettyCashRepos repos,
        IDocNumberService docNumberService,
        IEmployeeRepos employeeRepos,
        IPodAdminBranchRepos podAdminBranchRepos,
        IApprovalRouteResolverService routeResolver,
        IServiceProvider serviceProvider,
        ICurrentUserService currentUser,
        ILogger<PrettyCashService> logger)
    {
        _repos = repos;
        _docNumberService = docNumberService;
        _employeeRepos = employeeRepos;
        _podAdminBranchRepos = podAdminBranchRepos;
        _routeResolver = routeResolver;
        _serviceProvider = serviceProvider;
        _currentUser = currentUser;
        _logger = logger;
    }
    private IApprovalService ApprovalService => _serviceProvider.GetRequiredService<IApprovalService>();
    private static PrettyCashDto MapToDto(TbPrettyCashRequest s)
    {
        return new PrettyCashDto
        {
            Id = s.Id,
            DocNum = s.DocNum,
            DocStatus = s.DocStatus,
            DocDate = s.DocDate.Date,
            EmployeeId = s.EmployeeId,
            EmployeeNameTh = s.Employee != null ? $"{s.Employee.FnameTh} {s.Employee.LnameTh}" : null,
            Remark = s.Remark,
            TotalAmount = s.TotalAmount,
            Lines = s.TbPrettyCashLines.Select(l => new PrettyCashLineDto
            {
                Id = l.Id,
                BenefitId = l.BenefitId,
                BenefitNameTh = l.Benefit != null ? l.Benefit.NameTh : null,
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

    public async Task<PagedResultDto<PrettyCashDto>> GetAllAsync(PrettyCashFilterDto filter)
    {
        var employeeCode = _currentUser.GetCurrentUserEmployeeCode();
        var currentEmployeeId = await _employeeRepos.GetIdByEmpIdAsync(employeeCode) ?? Guid.Empty;
        var currentUserEmail = _currentUser.GetCurrentUserEmail();

        var adminBranches = await _podAdminBranchRepos.GetByUserIdWithBranchAsync(_currentUser.GetCurrentUserId());
        var adminBranchIds = adminBranches.Select(b => b.BranchId).ToList();

        var (items, totalCount) = await _repos.GetFilteredPagedAsync(filter, currentEmployeeId, currentUserEmail, adminBranchIds);
        return new PagedResultDto<PrettyCashDto> { Items = items.Select(MapToDto).ToList(), TotalCount = totalCount };
    }

    public async Task<PrettyCashDto?> GetByIdAsync(Guid id)
    {
        var s = await _repos.GetDetailByIdAsync(id);
        return s == null ? null : MapToDto(s);
    }

    public async Task<PrettyCashDto?> GetByDocNumAsync(string docNum)
    {
        var entity = await _repos.GetByDocNumAsync(docNum);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<PrettyCashDto> CreateAsync(CreatePrettyCashDto data)
    {
        try
        {
            var newEntity = new TbPrettyCashRequest
            {
                DocNum = await _docNumberService.GenerateAsync("PrettyCash"),
                DocStatus = "Draft",
                DocDate = data.DocDate.Date,
                EmployeeId = data.EmployeeId,
                Remark = data.Remark,
                TotalAmount = data.Lines.Sum(l => l.Amount),
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false,
                TbPrettyCashLines = data.Lines.Select(l => new TbPrettyCashLine
                {
                    BenefitId = l.BenefitId,
                    Detail = l.Detail,
                    LimitAmount = l.LimitAmount,
                    Amount = l.Amount,
                    Qty = l.Qty,
                    AccountCode = l.AccountCode,
                    CreatedAt = DateTime.Now,
                    CreatedBy = _currentUser.GetCurrentUserEmail(),
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = _currentUser.GetCurrentUserEmail(),
                    IsDelete = false
                }).ToList()
            };

            await _repos.AddAsync(newEntity);
            await _repos.SaveChangesAsync();

            return (await GetByIdAsync(newEntity.Id))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Pretty Cash: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PrettyCashDto?> UpdateAsync(Guid id, CreatePrettyCashDto data)
    {
        try
        {
            var existingEntity = await _repos.GetByIdAsync(id);
            if (existingEntity == null) return null;

            if (existingEntity.DocStatus != "Draft")
                throw new InvalidOperationException("แก้ไขได้เฉพาะเอกสารสถานะ Draft เท่านั้น");

            existingEntity.DocDate = data.DocDate;
            existingEntity.EmployeeId = data.EmployeeId;
            existingEntity.Remark = data.Remark;
            existingEntity.TotalAmount = data.Lines.Sum(l => l.Amount);
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.DeleteLinesAsync(id);

            foreach (var l in data.Lines)
            {
                existingEntity.TbPrettyCashLines.Add(new TbPrettyCashLine
                {
                    BenefitId = l.BenefitId,
                    Detail = l.Detail,
                    LimitAmount = l.LimitAmount,
                    Amount = l.Amount,
                    Qty = l.Qty,
                    AccountCode = l.AccountCode,
                    CreatedAt = DateTime.Now,
                    CreatedBy = _currentUser.GetCurrentUserEmail(),
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = _currentUser.GetCurrentUserEmail(),
                    IsDelete = false
                });
            }

            await _repos.SaveChangesAsync();
            return await GetByIdAsync(id);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการแก้ไขข้อมูล Pretty Cash: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var existingEntity = await _repos.GetByIdAsync(id);
            if (existingEntity == null) return false;

            if (existingEntity.DocStatus != "Draft")
                throw new InvalidOperationException("ลบได้เฉพาะเอกสารสถานะ Draft เท่านั้น");

            existingEntity.IsDelete = true;
            existingEntity.DeletedBy = _currentUser.GetCurrentUserEmail();
            existingEntity.DeletedAt = DateTime.Now;

            await _repos.SaveChangesAsync();
            return true;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล Pretty Cash: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PrettyCashDto?> SubmitAsync(Guid id)
    {
        try
        {
            var existingEntity = await _repos.GetByIdAsync(id);
            if (existingEntity == null) return null;

            if (existingEntity.DocStatus != "Draft")
                throw new InvalidOperationException("ส่งอนุมัติได้เฉพาะเอกสารสถานะ Draft เท่านั้น");

            var resolvedApprovers = await _routeResolver.ResolveAsync("PrettyCash", existingEntity.EmployeeId);

            existingEntity.DocStatus = "WaitApprove";
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.SaveChangesAsync();

            await ApprovalService.CreateApprovalAsync(
                "PrettyCash",
                existingEntity.DocNum,
                1,
                resolvedApprovers.OrderBy(x => x.StepNo).Select(x => x.ApproverEmployeeId).ToList()
            );

            return await GetByIdAsync(id);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการส่งอนุมัติ Pretty Cash: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PrettyCashDto?> RecallAsync(Guid id)
    {
        try
        {
            var existingEntity = await _repos.GetByIdAsync(id);
            if (existingEntity == null) return null;

            if (existingEntity.DocStatus != "WaitApprove" && existingEntity.DocStatus != "Rejected")
                throw new InvalidOperationException("ดึงกลับได้เฉพาะเอกสารสถานะ รออนุมัติ หรือ ปฏิเสธ เท่านั้น");

            existingEntity.DocStatus = "Draft";
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.SaveChangesAsync();

            await ApprovalService.RecallAsync("PrettyCash", existingEntity.DocNum!, 1);

            return await GetByIdAsync(id);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงเอกสารกลับ Pretty Cash: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<(DateTime DocDate, string EmployeeNameTh, string? DocumentDetail)?> GetSummaryAsync(string docNumber)
    {
        var entity = await _repos.GetByDocNumAsync(docNumber);
        if (entity == null) return null;

        var DocDetail = $"เอกสารเบิกสวัสดิการ หรือ เงินสดย่อย จำนวนเงินรวม {entity.TotalAmount} บาท";
        var employeeName = entity.Employee != null ? $"{entity.Employee.FnameTh} {entity.Employee.LnameTh}" : "-";

        return (entity.DocDate, employeeName, DocDetail);
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