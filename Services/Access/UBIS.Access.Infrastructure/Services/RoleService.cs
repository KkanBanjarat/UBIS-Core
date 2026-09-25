using Microsoft.Extensions.Logging;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepos _repo;
    private readonly ILogger<RoleService> _logger;
    private readonly ICurrentUserService _currentUser;

    public RoleService(IRoleRepos repos,
    ILogger<RoleService> logger,
    ICurrentUserService currentUser)
    {
        _repo = repos;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<List<RoleDto>> GetAllAsync()
    {
        try
        {
            var roles = await _repo.FindAsync(w => w.IsDelete == false);
            var perms = roles.Select(s => new RoleDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();
            return perms;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Role : {Message}", ex.Message);
            throw;
        }
    }
    public async Task<RoleDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var role = await _repo.FirstOrDefaultAsync(f => f.IsDelete == false && f.Id == id);

            if (role == null)
                return null;

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedBy,
                UpdatedAt = role.UpdatedAt,
                UpdatedBy = role.UpdatedBy,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Role : {Message}", ex.Message);
            throw;
        }
    }
    public async Task<RoleDto> CreateAsync(CreateRoleDto data)
    {
        try
        {
            if (await _repo.IsNameExistsAsync(data.Name))
                throw new InvalidOperationException($"ข้อมูลซ้ำในช่อง: Name");

            var newEntity = new TbRole
            {
                Name = data.Name,
                Description = data.Description,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false
            };

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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Roles : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<RoleDto?> UpdateAsync(Guid id, CreateRoleDto data)
    {
        try
        {
            var existingEntity = await _repo.GetByIdAsync(id);
            if (existingEntity == null)
                return null;

            var isDuplicate = (await _repo.FindAsync(x =>
                x.Name == data.Name && x.Id != id && !x.IsDelete)).Any();

            if (isDuplicate)
                throw new InvalidOperationException($"ข้อมูลซ้ำในช่อง: Name");

            existingEntity.Name = data.Name;
            existingEntity.Description = data.Description;
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการแก้ไขข้อมูล Role : {Message}", ex.Message);
            throw;
        }
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var existingEntity = await _repo.GetByIdAsync(id);
            if (existingEntity == null)
                return false;

            existingEntity.IsDelete = true;
            existingEntity.DeletedBy = _currentUser.GetCurrentUserEmail();
            existingEntity.DeletedAt = DateTime.Now;

            await _repo.UpdateAsync(existingEntity);
            await _repo.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล Roles : {Message}", ex.Message);
            throw;
        }
    }
}