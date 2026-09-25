using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class PositionLevelService : IPositionLevelService
{
    private readonly IPositionLevelRepos _repos;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PositionLevelService> _logger;

    public PositionLevelService(IPositionLevelRepos repos,
        ICurrentUserService currentUser,
        ILogger<PositionLevelService> logger)
    {
        _repos = repos;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<IEnumerable<PositionLevelDto>> GetAllAsync()
    {
        try
        {
            var positionLevels = await _repos.GetListAsync(predicate: w=>w.IsDelete == false);

            return positionLevels.Select(s => new PositionLevelDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameTh = s.NameTh,
                IsActive = s.IsActive,
                IsSubsidiary = s.IsSubsidiary,
                Code = s.Code,
                Level = s.Level,
                Track = s.Track,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Position Level : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PositionLevelDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _repos.GetByIdAsync(id);
            if (entity == null || entity.IsDelete)
                return null;

            return new PositionLevelDto
            {
                Id = entity.Id,
                NameEn = entity.NameEn,
                NameTh = entity.NameTh,
                IsActive = entity.IsActive,
                IsSubsidiary = entity.IsSubsidiary,
                Code = entity.Code,
                Level = entity.Level,
                Track = entity.Track,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Position Level : {Message}", ex.Message);
            throw;
        }
    }
    
    public async Task<PagedResultDto<PositionLevelDto>> GetPagedAsync(PositionLevelFilterDto filter)
    {
        try
        {
            var (items, totalCount) = await _repos.GetFilteredPagedAsync(filter);

            var dtos = items.Select(s => new PositionLevelDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameTh = s.NameTh,
                IsActive = s.IsActive,
                IsSubsidiary = s.IsSubsidiary,
                Code = s.Code,
                Level = s.Level,
                Track = s.Track,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();

            return new PagedResultDto<PositionLevelDto> { Items = dtos, TotalCount = totalCount };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Position Level (Paged): {Message}", ex.Message);
            throw;
        }
    }
    public async Task<PositionLevelDto> CreateAsync(CreatePositionLevelDto data)
    {
        try
        {
            var duplicateFields = new List<string>();

            if (await _repos.AnyAsync(x => x.NameTh == data.NameTh && !x.IsDelete))
                duplicateFields.Add("NameTh");

            if (await _repos.AnyAsync(x => x.NameEn == data.NameEn && !x.IsDelete))
                duplicateFields.Add("NameEn");

            if (await _repos.AnyAsync(x => x.Code == data.Code && !x.IsDelete))
                duplicateFields.Add("Code");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            var newEntity = new TbPositionLevel
            {
                NameEn = data.NameEn,
                NameTh = data.NameTh,
                IsActive = data.IsActive,
                IsSubsidiary = data.IsSubsidiary,
                Code = data.Code,
                Level = data.Level,
                Track = data.Track,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false
            };

            await _repos.AddAsync(newEntity);
            await _repos.SaveChangesAsync();

            return new PositionLevelDto
            {
                Id = newEntity.Id,
                NameEn = newEntity.NameEn,
                NameTh = newEntity.NameTh,
                IsActive = newEntity.IsActive,
                Code = newEntity.Code,
                Track = newEntity.Track,
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Position Level : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PositionLevelDto?> UpdateAsync(Guid id, CreatePositionLevelDto data)
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

            if (await _repos.AnyAsync(x => x.Code == data.Code && x.Id != id && !x.IsDelete))
                duplicateFields.Add("Code");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            existingEntity.NameEn = data.NameEn;
            existingEntity.NameTh = data.NameTh;
            existingEntity.IsActive = data.IsActive;
            existingEntity.IsSubsidiary = data.IsSubsidiary;
            existingEntity.Code = data.Code;
            existingEntity.Track = data.Track;
            existingEntity.Level = data.Level;
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.SaveChangesAsync();

            return new PositionLevelDto
            {
                Id = existingEntity.Id,
                NameEn = existingEntity.NameEn,
                NameTh = existingEntity.NameTh,
                IsActive = existingEntity.IsActive,
                IsSubsidiary = existingEntity.IsSubsidiary,
                Code = existingEntity.Code,
                Level = existingEntity.Level,
                Track = existingEntity.Track,
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Position Level : {Message}", ex.Message);
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล Position Level: {Message}", ex.Message);
            throw;
        }
    }
}