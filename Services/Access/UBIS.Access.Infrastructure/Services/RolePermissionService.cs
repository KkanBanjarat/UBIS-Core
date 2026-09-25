using Microsoft.Extensions.Logging;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Services;

public class RolePermissionService : IRolePermissionService
{
    private readonly IRolePermissionRepos _repo;
    private readonly IRoleRepos _roleRepo;
    private readonly IPermissionRepos _permissionRepo;
    private readonly ILogger<RolePermissionService> _logger;
    private readonly ICurrentUserService _currentUser;

    public RolePermissionService(
        IRolePermissionRepos repo,
        IRoleRepos roleRepo,
        IPermissionRepos permissionRepo,
        ILogger<RolePermissionService> logger,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _roleRepo = roleRepo;
        _permissionRepo = permissionRepo;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<List<RolePermissionDto>> GetAllAsync()
    {
        try
        {
            // ✅ ดึงข้อมูล RolePermission ทั้งหมด พร้อม Role และ Permission
            var raw = await _repo.FindAsync(w => !w.IsDelete);

            // ✅ GroupBy ตาม RoleId - แบ่งกลุ่มตาม Role
            var result = raw
                .GroupBy(x => x.RoleId)
                .Select(g => new RolePermissionDto
                {
                    RoleId = g.Key,
                    RoleName = g.First().Role.Name,  // ✅ ดึงชื่อ Role จากตัวแรกของกลุ่ม
                    Permissions = g.Select(x => new PermissionSummaryDto
                    {
                        Id = x.Permission.Id,
                        Code = x.Permission.Code,
                        Description = x.Permission.Description,
                    }).ToList()
                })
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Role Permission : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<RolePermissionDto> AssignAsync(CreateRolePermissionDto data)
    {
        try
        {
            // ✅ เช็คว่า Role มี Permission นี้หรือยัง
            if (await _repo.IsPermissionAssignedAsync(data.RoleId, data.PermissionId))
                throw new InvalidOperationException("Role นี้มี Permission นี้อยู่แล้ว");

            // ✅ สร้าง entity ใหม่
            var newEntity = new TbRolePermission
            {
                RoleId = data.RoleId,
                PermissionId = data.PermissionId,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false
            };

            // ✅ บันทึกลงฐานข้อมูล
            await _repo.AddAsync(newEntity);
            await _repo.SaveChangesAsync();

            // ✅ ดึง Role และ Permission details
            var role = await _roleRepo.FirstOrDefaultAsync(r => r.Id == newEntity.RoleId && !r.IsDelete);
            var permission = await _permissionRepo.FirstOrDefaultAsync(p =>
                p.Id == newEntity.PermissionId && !p.IsDelete);

            if (permission == null)
                throw new InvalidOperationException("ไม่พบ Permssion");
            // ✅ return ผลลัพธ์
            var result = new RolePermissionDto
            {
                RoleId = newEntity.RoleId,
                RoleName = role?.Name,
                Permissions = new List<PermissionSummaryDto>
                {
                    new PermissionSummaryDto
                    {
                        Id = permission.Id,
                        Code = permission.Code,
                        Description = permission.Description
                    }
                }
            };

            return result;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Role Permission : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> RevokeAsync(Guid id)
    {
        try
        {
            // ✅ ดึง RolePermission
            var existingEntity = await _repo.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (existingEntity == null)
                return false;

            // ✅ Soft Delete
            existingEntity.IsDelete = true;
            existingEntity.DeletedBy = _currentUser.GetCurrentUserEmail();
            existingEntity.DeletedAt = DateTime.Now;

            // ✅ บันทึก
            await _repo.UpdateAsync(existingEntity);
            await _repo.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล Role Permission : {Message}", ex.Message);
            throw;
        }
    }
}