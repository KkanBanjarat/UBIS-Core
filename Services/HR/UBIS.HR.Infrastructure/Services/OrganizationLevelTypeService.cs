using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class OrganizationLevelTypeService : IOrganizationLevelTypeService
{
    private readonly IOrganizationLevelTypeRepos _repos;
    private readonly ILogger<OrganizationLevelTypeService> _logger;
    private readonly ICurrentUserService _currentUser;

    public OrganizationLevelTypeService(IOrganizationLevelTypeRepos repos,
        ICurrentUserService currentUser,
        ILogger<OrganizationLevelTypeService> logger)
    {
        _repos = repos;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<OrganizationLevelTypeDto>> GetAllAsync()
    {
        try
        {
            var orgLevelTypes = await _repos.GetAllAsync();

            return orgLevelTypes.Select(s => new OrganizationLevelTypeDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameTh = s.NameTh,
                Sequence = s.Sequence,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล OrganizationLevelType: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<OrganizationLevelTypeDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _repos.GetByIdAsync(id);
            if (entity == null || entity.IsDelete)
                return null;

            return new OrganizationLevelTypeDto
            {
                Id = entity.Id,
                NameTh = entity.NameTh,
                NameEn = entity.NameEn,
                Sequence = entity.Sequence,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล OrganizationLevelType: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<OrganizationLevelTypeDto> CreateAsync(CreateOrganizationLevelTypeDto data)
    {
        try
        {
            var duplicateFields = new List<string>();

            if (await _repos.AnyAsync(x => x.NameTh == data.NameTh && !x.IsDelete))
                duplicateFields.Add("NameTh");

            if (await _repos.AnyAsync(x => x.NameEn == data.NameEn && !x.IsDelete))
                duplicateFields.Add("NameEn");

            if (await _repos.AnyAsync(x => x.Sequence == data.Sequence && !x.IsDelete))
                duplicateFields.Add("Sequence");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            var newEntity = new TbOrganizationLevelType
            {
                NameEn = data.NameEn,
                NameTh = data.NameTh,
                Sequence = data.Sequence,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false,
            };

            await _repos.AddAsync(newEntity);
            await _repos.SaveChangesAsync();

            return new OrganizationLevelTypeDto
            {
                Id = newEntity.Id,
                NameEn = newEntity.NameEn,
                NameTh = newEntity.NameTh,
                Sequence = newEntity.Sequence,
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล OrganizationLevelType: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<OrganizationLevelTypeDto?> UpdateAsync(Guid id, CreateOrganizationLevelTypeDto data)
    {
        try
        {
            var existingEntity = await _repos.GetByIdAsync(id);
            if (existingEntity == null)
                return null;

            var duplicateFields = new List<string>();

            if (await _repos.AnyAsync(x => x.NameTh == data.NameTh && x.Id != id && !x.IsDelete))
                duplicateFields.Add("NameTh");

            if (await _repos.AnyAsync(x => x.NameEn == data.NameEn && x.Id != id && !x.IsDelete))
                duplicateFields.Add("NameEn");

            if (await _repos.AnyAsync(x => x.Sequence == data.Sequence && x.Id != id && !x.IsDelete))
                duplicateFields.Add("Sequence");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            existingEntity.NameEn = data.NameEn;
            existingEntity.NameTh = data.NameTh;
            existingEntity.Sequence = data.Sequence;
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.SaveChangesAsync();

            return new OrganizationLevelTypeDto
            {
                Id = existingEntity.Id,
                NameEn = existingEntity.NameEn,
                NameTh = existingEntity.NameTh,
                Sequence = existingEntity.Sequence,
                CreatedAt = existingEntity.CreatedAt,
                CreatedBy = existingEntity.CreatedBy,
                UpdatedAt = existingEntity.UpdatedAt,
                UpdatedBy = existingEntity.UpdatedBy
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล OrganizationLevelType: {Message}", ex.Message);
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล OrganizationLevelType: {Message}", ex.Message);
            throw;
        }
    }
}