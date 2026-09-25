using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class RouteApproveService : IRouteApproveService
{
    private readonly IRouteApproveRepos _repos;
    private readonly IEmployeeRepos _employeeRepos;
    private readonly IOrganizationUnitRepos _orgUnitRepos;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RouteApproveService> _logger;

    public RouteApproveService(IRouteApproveRepos repos,
        IEmployeeRepos employeeRepos,
        IOrganizationUnitRepos orgUnitRepos,
        ICurrentUserService currentUser,
        ILogger<RouteApproveService> logger)
    {
        _repos = repos;
        _employeeRepos = employeeRepos;
        _orgUnitRepos = orgUnitRepos;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<RouteApproveDto>> GetAllAsync()
    {
        var routes = await _repos.GetAllRoutesAsync();

        // ดึงชื่อพนักงานที่ถูกกำหนดเป็นผู้อนุมัติแบบ Fixed มาแสดง (Query เดียวจบ ไม่ N+1)
        var employeeIds = routes
            .Where(x => x.FixedEmployeeId.HasValue)
            .Select(x => x.FixedEmployeeId!.Value)
            .Distinct()
            .ToList();

        var nameMap = employeeIds.Count > 0
            ? await _employeeRepos.GetNamesByIdsAsync(employeeIds)
            : new Dictionary<Guid, string>();

        return routes.Select(x => new RouteApproveDto
        {
            Id = x.Id,
            DocType = x.DocType,
            StepNo = x.StepNo,
            StepName = x.StepName,
            ApproverType = x.ApproverType,
            MinPositionLevel = x.MinPositionLevel,
            FixedEmployeeId = x.FixedEmployeeId,
            FixedEmployeeNameTh = x.FixedEmployeeId.HasValue
                ? nameMap.GetValueOrDefault(x.FixedEmployeeId.Value, "(ไม่พบข้อมูลพนักงาน)")
                : null,
            OrganizationUnitId = x.OrganizationUnitId,
            IsActive = x.IsActive,
            UpdatedBy = x.UpdatedBy,
            UpdatedAt = x.UpdatedAt,
        }).ToList();
    }

    public async Task<List<string>> GetDocTypesAsync()
    {
        return await _repos.GetDocTypesAsync();
    }

    public async Task<RouteApproveDto> CreateAsync(CreateRouteApproveDto data)
    {
        try
        {
            ValidateApproverType(data);

            if (await _repos.ExistsStepNoAsync(data.DocType, data.StepNo))
                throw new InvalidOperationException($"มีขั้นตอนที่ {data.StepNo} ของเอกสารประเภทนี้อยู่แล้ว");

            var entity = new TbRouteApprove
            {
                DocType = data.DocType,
                StepNo = data.StepNo,
                StepName = data.StepName,
                ApproverType = data.ApproverType,
                MinPositionLevel = data.MinPositionLevel,
                FixedEmployeeId = data.FixedEmployeeId,
                OrganizationUnitId = data.OrganizationUnitId,
                IsActive = data.IsActive,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
            };

            await _repos.AddAsync(entity);
            await _repos.SaveChangesAsync();

            var all = await GetAllAsync();
            return all.First(x => x.Id == entity.Id);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มสายอนุมัติ: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<RouteApproveDto?> UpdateAsync(Guid id, CreateRouteApproveDto data)
    {
        try
        {
            var entity = await _repos.GetByIdAsync(id);
            if (entity == null) return null;

            ValidateApproverType(data);

            if (await _repos.ExistsStepNoAsync(data.DocType, data.StepNo, id))
                throw new InvalidOperationException($"มีขั้นตอนที่ {data.StepNo} ของเอกสารประเภทนี้อยู่แล้ว");

            entity.DocType = data.DocType;
            entity.StepNo = data.StepNo;
            entity.StepName = data.StepName;
            entity.ApproverType = data.ApproverType;
            entity.MinPositionLevel = data.MinPositionLevel;
            entity.FixedEmployeeId = data.FixedEmployeeId;
            entity.OrganizationUnitId = data.OrganizationUnitId;
            entity.IsActive = data.IsActive;
            entity.UpdatedAt = DateTime.Now;
            entity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.SaveChangesAsync();

            var all = await GetAllAsync();
            return all.First(x => x.Id == id);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการแก้ไขสายอนุมัติ: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _repos.GetByIdAsync(id);
            if (entity == null) return false;

            // ตารางนี้ไม่มี IsDelete จึงลบจริง (Config ไม่ใช่ Transaction ไม่ต้องเก็บประวัติ)
            await _repos.DeleteAsync(entity);
            await _repos.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบสายอนุมัติ: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// เช็คว่า ApproverType แต่ละแบบมีข้อมูลที่จำเป็นครบไหม
    /// ถ้าปล่อยผ่านไป จะไปพังตอน Submit เอกสารจริงซึ่งแก้ยากกว่ามาก
    /// </summary>
    private static void ValidateApproverType(CreateRouteApproveDto data)
    {
        switch (data.ApproverType)
        {
            case "ManagerChain":
                if (!data.MinPositionLevel.HasValue)
                    throw new InvalidOperationException("กรุณาระบุระดับตำแหน่งขั้นต่ำ สำหรับประเภท 'ไล่ตามสายบังคับบัญชา'");
                break;

            case "FixedEmployee":
                if (!data.FixedEmployeeId.HasValue)
                    throw new InvalidOperationException("กรุณาเลือกพนักงานผู้อนุมัติ สำหรับประเภท 'ระบุบุคคล'");
                break;

            case "BranchAdmin":
                // ไม่ต้องมีข้อมูลเพิ่ม (ใช้ผู้ดูแลสาขาหลักของผู้ขอเบิกอัตโนมัติ)
                break;

            default:
                throw new InvalidOperationException($"ไม่รู้จักประเภทผู้อนุมัติ: {data.ApproverType}");
        }
    }
}