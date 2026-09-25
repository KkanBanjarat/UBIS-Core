using Microsoft.Extensions.Logging;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepos _repo;
    private readonly ILogger<PermissionService> _logger;
    private readonly ICurrentUserService _currentUser;

    public PermissionService(IPermissionRepos repo,
    ILogger<PermissionService> logger,
    ICurrentUserService currentUser)
    {
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<List<PermissionDto>> GetAllAsync()
    {
        try
        {
            // ✅ ใช้ repository
            var perms = await _repo.FindAsync(w => w.IsDelete == false);

            return perms.Select(s => new PermissionDto
            {
                Id = s.Id,
                Code = s.Code,
                Description = s.Description,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Permission: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PermissionDto?> GetByIdAsync(Guid id)
    {
        try
        {
            // ✅ ใช้ repository
            var perm = await _repo.FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete);

            if (perm == null)
                return null;

            return new PermissionDto
            {
                Id = perm.Id,
                Description = perm.Description,
                Code = perm.Code,
                CreatedAt = perm.CreatedAt,
                CreatedBy = perm.CreatedBy,
                UpdatedAt = perm.UpdatedAt,
                UpdatedBy = perm.UpdatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Permission : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PermissionDto> CreateAsync(CreatePermissionDto data)
    {
        try
        {
            // ✅ ใช้ repository เช็ค Code ซ้ำ
            if (await _repo.IsCodeExistsAsync(data.Code))
                throw new InvalidOperationException($"ข้อมูลซ้ำในช่อง: Code");

            var newEntity = new TbPermission
            {
                Code = data.Code,
                Description = data.Description,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false
            };

            // ✅ ใช้ repository
            await _repo.AddAsync(newEntity);
            await _repo.SaveChangesAsync();

            return (await GetByIdAsync(newEntity.Id))!;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Permissions : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PermissionDto?> UpdateAsync(Guid id, CreatePermissionDto data)
    {
        try
        {
            // ✅ ดึงข้อมูล Entity
            var existingEntity = await _repo.FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete);
            if (existingEntity == null)
                return null;

            // ✅ เช็ค Code ซ้ำ (ยกเว้นตัวเอง)
            var isDuplicate = (await _repo.FindAsync(x =>
                x.Code == data.Code && x.Id != id && !x.IsDelete)).Any();

            if (isDuplicate)
                throw new InvalidOperationException($"ข้อมูลซ้ำในช่อง: Code");

            existingEntity.Code = data.Code;
            existingEntity.Description = data.Description;
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            // ✅ ใช้ repository
            await _repo.UpdateAsync(existingEntity);
            await _repo.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการแก้ไขข้อมูล Permission : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            // ✅ ดึงข้อมูล Entity
            var existingEntity = await _repo.FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete);
            if (existingEntity == null)
                return false;

            existingEntity.IsDelete = true;
            existingEntity.DeletedBy = _currentUser.GetCurrentUserEmail();
            existingEntity.DeletedAt = DateTime.Now;

            // ✅ ใช้ repository
            await _repo.UpdateAsync(existingEntity);
            await _repo.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล Permission : {Message}", ex.Message);
            throw;
        }
    }
}