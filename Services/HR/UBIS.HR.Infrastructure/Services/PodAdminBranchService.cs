using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class PodAdminBranchService : IPodAdminBranchService
{
    private readonly IPodAdminBranchRepos _repos;
    private readonly IBranchRepos _branchRepos;
    private readonly IEmployeeRepos _employeeRepos;
    private readonly IAccessServiceClient _accessClient;
    private readonly ILogger<PodAdminBranchService> _logger;
    private readonly ICurrentUserService _currentUser;

    public PodAdminBranchService(IPodAdminBranchRepos repos,
        IBranchRepos branchRepos,
        IEmployeeRepos employeeRepos,
        IAccessServiceClient accessClient,
        ILogger<PodAdminBranchService> logger,
        ICurrentUserService currentUser)
    {
        _repos = repos;
        _branchRepos = branchRepos;
        _employeeRepos = employeeRepos;
        _accessClient = accessClient;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<List<PodAdminBranchGroupDto>> GetAllAsync()
    {
        try
        {
            var raw = await _repos.GetAllWithBranchAsync();

            var result = raw
                .GroupBy(x => x.UserId)
                .Select(g => new PodAdminBranchGroupDto
                {
                    UserId = g.Key,
                    Branches = g.Select(x => new BranchSummaryDto
                    {
                        Id = x.BranchId,
                        Code = x.Branch.Code,
                        NameTh = x.Branch.NameTh,
                        NameEn = x.Branch.NameEn,
                    }).ToList()
                })
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล POD Admin (GetAllAsync) : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<List<BranchSummaryDto>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var raw = await _repos.GetByUserIdWithBranchAsync(userId);

            return raw.Select(s => new BranchSummaryDto
            {
                Id = s.BranchId,
                Code = s.Branch.Code,
                NameEn = s.Branch.NameEn,
                NameTh = s.Branch.NameTh,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล POD Admin (GetByUserId) : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PodAdminBranchDto> CreateAsync(CreatePodAdminBranchDto data)
    {
        try
        {
            // 1) หาข้อมูลพนักงานใน HR ก่อน
            var employee = await _employeeRepos.GetByIdAsync(data.EmployeeId);
            if (employee == null)
                throw new InvalidOperationException("ไม่พบข้อมูลพนักงาน");

            // 2) หา UserId จาก Access Service (ผ่าน API ไม่แตะ Database ข้าม Service)
            var userId = await _accessClient.LookupUserIdAsync(data.EmployeeId, employee.EmpId, employee.Email);
            if (userId == null)
                throw new InvalidOperationException(
                    $"ไม่พบบัญชีผู้ใช้งานของ {employee.FnameTh} {employee.LnameTh} ({employee.EmpId}) ในระบบ กรุณาสร้างบัญชีผู้ใช้งานก่อน");

            // 3) กันซ้ำ
            var isDuplicate = await _repos.AnyAsync(x =>
                x.UserId == userId.Value && x.BranchId == data.BranchId && !x.IsDelete);
            if (isDuplicate)
                throw new InvalidOperationException("พนักงานคนนี้ถูกกำหนดให้ดูแลสาขานี้อยู่แล้ว");

            // 4) ถ้าตั้งเป็นผู้ดูแลหลัก ต้องปลดคนเดิมออกก่อน (1 สาขามีได้คนเดียว)
            if (data.IsPrimary)
                await ClearPrimaryAsync(data.BranchId);

            var newEntity = new TbPodAdminBranch
            {
                BranchId = data.BranchId,
                UserId = userId.Value,
                EmployeeId = data.EmployeeId,
                IsPrimary = data.IsPrimary,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false
            };

            await _repos.AddAsync(newEntity);
            await _repos.SaveChangesAsync();

            var branch = await _branchRepos.GetByIdAsync(data.BranchId);

            return new PodAdminBranchDto
            {
                Id = newEntity.Id,
                UserId = newEntity.UserId,
                BranchId = newEntity.BranchId,
                BranchNameEn = branch!.NameEn,
                BranchNameTh = branch.NameTh,
                CreatedAt = newEntity.CreatedAt,
                CreatedBy = newEntity.CreatedBy,
                UpdatedAt = newEntity.UpdatedAt,
                UpdatedBy = newEntity.UpdatedBy
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มผู้ดูแลสาขา: {Message}", ex.Message);
            throw;
        }
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var existingEntity = await _repos.GetByIdAsync(id);
            if (existingEntity == null)
            {
                return false;
            }

            existingEntity.IsDelete = true;
            existingEntity.DeletedBy = _currentUser.GetCurrentUserEmail();
            existingEntity.DeletedAt = DateTime.Now;

            await _repos.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล POD Admin (DeleteAsync) : {Message}", ex.Message);
            throw;
        }
    }
    public async Task<List<BranchAdminGroupDto>> GetGroupedByBranchAsync()
    {
        try
        {
            var raw = await _repos.GetAllWithBranchAsync();
            var allBranches = await _branchRepos.GetAllAsync();

            // ดึงชื่อพนักงานทีเดียว ไม่ N+1
            var employeeIds = raw
                .Where(x => x.EmployeeId.HasValue)
                .Select(x => x.EmployeeId!.Value)
                .Distinct()
                .ToList();

            var nameMap = employeeIds.Count > 0
                ? await _employeeRepos.GetNamesByIdsAsync(employeeIds)
                : new Dictionary<Guid, string>();

            var adminsByBranch = raw.GroupBy(x => x.BranchId).ToDictionary(g => g.Key, g => g.ToList());

            // แสดงทุกสาขา รวมสาขาที่ยังไม่มีผู้ดูแล เพื่อให้ IT เห็นว่าต้องไปเพิ่ม
            return allBranches
                .Where(b => !b.IsDelete)
                .OrderBy(b => b.Code)
                .Select(b => new BranchAdminGroupDto
                {
                    BranchId = b.Id,
                    BranchCode = b.Code,
                    BranchNameTh = b.NameTh,
                    BranchNameEn = b.NameEn,
                    Admins = adminsByBranch.TryGetValue(b.Id, out var list)
                        ? list.Select(x => new BranchAdminItemDto
                        {
                            Id = x.Id,
                            UserId = x.UserId,
                            EmployeeId = x.EmployeeId,
                            EmployeeNameTh = x.EmployeeId.HasValue
                                ? nameMap.GetValueOrDefault(x.EmployeeId.Value, "(ไม่พบข้อมูลพนักงาน)")
                                : "(ยังไม่ได้ผูกข้อมูลพนักงาน)",
                            IsPrimary = x.IsPrimary,
                            UpdatedAt = x.UpdatedAt,
                            UpdatedBy = x.UpdatedBy,
                        })
                        .OrderByDescending(a => a.IsPrimary)
                        .ThenBy(a => a.EmployeeNameTh)
                        .ToList()
                        : new List<BranchAdminItemDto>()
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูลผู้ดูแลสาขา: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> SetPrimaryAsync(Guid id)
    {
        try
        {
            var target = await _repos.GetByIdAsync(id);
            if (target == null || target.IsDelete) return false;

            if (target.EmployeeId == null)
                throw new InvalidOperationException("รายการนี้ยังไม่ได้ผูกกับข้อมูลพนักงาน จึงตั้งเป็นผู้ดูแลหลักไม่ได้");

            await ClearPrimaryAsync(target.BranchId);

            target.IsPrimary = true;
            target.UpdatedAt = DateTime.Now;
            target.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.SaveChangesAsync();
            return true;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการตั้งผู้ดูแลหลัก: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// ปลดผู้ดูแลหลักคนเดิมของสาขานี้ออกก่อน (Unique Index บังคับให้มีได้แค่คนเดียวต่อสาขา)
    /// </summary>
    private async Task ClearPrimaryAsync(Guid branchId)
    {
        var existing = await _repos.GetByBranchIdAsync(branchId);
        foreach (var item in existing.Where(x => x.IsPrimary))
        {
            item.IsPrimary = false;
            item.UpdatedAt = DateTime.Now;
            item.UpdatedBy = _currentUser.GetCurrentUserEmail();
        }
        await _repos.SaveChangesAsync();
    }
}