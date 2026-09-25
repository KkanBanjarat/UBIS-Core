using Microsoft.Extensions.Logging;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Services;

public class UserRoleService : IUserRoleService
{
    private readonly IUserRoleRepos _repo;
    private readonly IRoleRepos _roleRepo;
    private readonly ILogger<UserRoleService> _logger;
    private readonly ICurrentUserService _currentUser;

    public UserRoleService(
        IUserRoleRepos repo,
        IRoleRepos roleRepo,
        ILogger<UserRoleService> logger,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _roleRepo = roleRepo;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<List<UserRoleDto>> GetAllAsync()
    {
        try
        {
            // ✅ ดึงข้อมูล UserRole ทั้งหมด
            var raw = await _repo.FindAsync(w => !w.IsDelete);

            // ✅ GroupBy ตาม UserId - แบ่งกลุ่มตาม User
            var result = raw
                .GroupBy(x => x.UserId)
                .Select(g => new UserRoleDto
                {
                    UserId = g.Key,
                    Roles = g.Select(x => new RoleScopeSummaryDto
                    {
                        RoleId = x.RoleId,
                        RoleName = x.Role.Name,
                        Scope = x.Scope
                    }).ToList()
                })
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล User Role : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<List<RoleScopeSummaryDto>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            // ✅ ใช้ repository
            var userRoles = await _repo.GetByUserIdWithDetailsAsync(userId);

            var roles = userRoles.Select(x => new RoleScopeSummaryDto
            {
                RoleId = x.RoleId,
                RoleName = x.Role.Name,
                Scope = x.Scope
            }).ToList();

            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล User Role : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<UserRoleDto> AssignAsync(CreateUserRoleDto data)
    {
        try
        {
            // ✅ เช็คว่า User มี Role นี้หรือยัง
            if (await _repo.IsRoleAssignedAsync(data.UserId, data.RoleId))
                throw new InvalidOperationException("User นี้ถือ Role นี้อยู่แล้ว");

            // ✅ สร้าง entity ใหม่
            var newEntity = new TbUserRole
            {
                UserId = data.UserId,
                RoleId = data.RoleId,
                Scope = data.Scope,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false
            };

            // ✅ บันทึกลงฐานข้อมูล
            await _repo.AddAsync(newEntity);
            await _repo.SaveChangesAsync();

            // ✅ ดึง Role details
            var role = await _roleRepo.FirstOrDefaultAsync(r => r.Id == newEntity.RoleId && !r.IsDelete);
            // if (role == null)
            //     throw new InvalidOperationException("ไม่พบ Role ที่ต้องการกำหนดให้ User");

            // ✅ return ผลลัพธ์
            var result = new UserRoleDto
            {
                UserId = newEntity.UserId,
                Roles = new List<RoleScopeSummaryDto>
                {
                    new RoleScopeSummaryDto
                    {
                        RoleId = newEntity.RoleId,
                        RoleName = role?.Name,
                        Scope = newEntity.Scope
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล User Role : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> RevokeAsync(Guid id)
    {
        try
        {
            // ✅ ดึง UserRole
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล User Role : {Message}", ex.Message);
            throw;
        }
    }

    private static int ScopeRank(string scope) => scope switch
    {
        "Self" => 1,
        "Subordinate" => 2,
        "Branch" => 3,
        "All" => 4,
        _ => 0
    };

    public async Task<List<EffectivePermissionDto>> GetEffectivePermissionsAsync(Guid userId)
    {
        try
        {
            // ✅ ดึง UserRole พร้อม Role -> RolePermission -> Permission
            var userRoles = await _repo.GetEffectiveRolesAsync(userId);

            // STEP 1: Flatten - แปลง nested structure เป็น flat list
            // จาก: UserRole > Role > RolePermissions > Permission
            // เป็น: {Code, Scope} pairs
            var flat = userRoles
                .SelectMany(ur => ur.Role.TbRolePermissions
                    .Select(rp => new { Code = rp.Permission.Code, Scope = ur.Scope }));

            // STEP 2: GroupBy Code - แบ่งกลุ่มตาม Permission Code
            // เหตุผล: User อาจมี Role หลายตัว ซึ่งอาจมี Permission เดียวกัน แต่ Scope ต่างกัน
            var result = flat
                .GroupBy(x => x.Code)
                .Select(g => new EffectivePermissionDto
                {
                    PermissionCode = g.Key,
                    // STEP 3: เลือก Scope ที่สูงสุด
                    // เหตุผล: "All" > "Branch" > "Subordinate" > "Self"
                    // ถ้า User มี Role ที่มี Permission "READ" Scope "Self" และ Role อื่นมี "READ" Scope "All"
                    // ให้ใช้ "All" (มากกว่า)
                    Scope = g.OrderByDescending(x => ScopeRank(x.Scope)).First().Scope
                })
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงสิทธิ์ของ User : {Message}", ex.Message);
            throw;
        }
    }
}