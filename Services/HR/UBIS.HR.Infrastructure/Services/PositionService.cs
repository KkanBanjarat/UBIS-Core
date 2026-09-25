using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class PositionService : IPositionService
{
    private readonly IPositionRepos _repos;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PositionService> _logger;

    public PositionService(IPositionRepos repos,
        ICurrentUserService currentUser,
        ILogger<PositionService> logger)
    {
        _repos = repos;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<IEnumerable<PositionDto>> GetAllAsync()
    {
        try
        {
            var positions = await _repos.GetAllAsync();

            return positions.Select(s => new PositionDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameTh = s.NameTh,
                IsActive = s.IsActive,
                IsSubsidiary = s.IsSubsidiary,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Position: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PositionDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _repos.GetByIdAsync(id);
            if (entity == null || entity.IsDelete)
                return null;

            return new PositionDto
            {
                Id = entity.Id,
                NameTh = entity.NameTh,
                NameEn = entity.NameEn,
                IsActive = entity.IsActive,
                IsSubsidiary = entity.IsSubsidiary,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Position : {Message}", ex.Message);
            throw;
        }
    }
    public async Task<(IEnumerable<PositionDto> Items, int TotalCount)> GetFilteredPagedAsync(PositionFilterDto filter)
    {
        try
        {
            var (items, totalCount) = await _repos.GetFilteredPagedAsync(filter);

            var dtos = items.Select(s => new PositionDto
            {
                Id = s.Id,
                NameTh = s.NameTh,
                NameEn = s.NameEn,
                IsActive = s.IsActive,
                IsSubsidiary = s.IsSubsidiary,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();

            return (dtos, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Position (Paged): {Message}", ex.Message);
            throw;
        }
    }
    
    public async Task<PositionDto> CreateAsync(CreatePositionDto data)
    {
        try
        {
            var duplicateFields = new List<string>();

            if (await _repos.AnyAsync(x => x.NameTh == data.NameTh && !x.IsDelete))
                duplicateFields.Add("NameTh");

            if (await _repos.AnyAsync(x => x.NameEn == data.NameEn && !x.IsDelete))
                duplicateFields.Add("NameEn");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            var newEntity = new TbPosition
            {
                NameEn = data.NameEn,
                NameTh = data.NameTh,
                IsActive = data.IsActive,
                IsSubsidiary = data.IsSubsidiary,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false
            };

            await _repos.AddAsync(newEntity);
            await _repos.SaveChangesAsync();

            return new PositionDto
            {
                Id = newEntity.Id,
                NameEn = newEntity.NameEn,
                NameTh = newEntity.NameTh,
                IsActive = newEntity.IsActive,
                IsSubsidiary = newEntity.IsSubsidiary,
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Position : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PositionDto?> UpdateAsync(Guid id, CreatePositionDto data)
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

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            existingEntity.NameEn = data.NameEn;
            existingEntity.NameTh = data.NameTh;
            existingEntity.IsActive = data.IsActive;
            existingEntity.IsSubsidiary = data.IsSubsidiary;
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.SaveChangesAsync();

            return new PositionDto
            {
                Id = existingEntity.Id,
                NameEn = existingEntity.NameEn,
                NameTh = existingEntity.NameTh,
                IsActive = existingEntity.IsActive,
                IsSubsidiary = existingEntity.IsSubsidiary,
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Position: {Message}", ex.Message);
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล Position: {Message}", ex.Message);
            throw;
        }
    }
}