using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class OrganizationUnitService : IOrganizationUnitService
{
    private readonly IOrganizationUnitRepos _repos;
    private readonly ILogger<OrganizationUnitService> _logger;
    private readonly ICurrentUserService _currentUser;

    public OrganizationUnitService(IOrganizationUnitRepos repos,
        ICurrentUserService currentUser,
        ILogger<OrganizationUnitService> logger)
    {
        _repos = repos;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<OrganizationUnitDto>> GetAllAsync(string? type = null)
    {
        try
        {
            var units = await _repos.FindAsync(x => !x.IsDelete
                && (string.IsNullOrEmpty(type) || x.Type == type));

            return units.Select(s => new OrganizationUnitDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameTh = s.NameTh,
                Code = s.Code,
                Type = s.Type,
                ShortName = s.ShortName,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล OrganizationUnit: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<OrganizationUnitDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _repos.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return null;

            return new OrganizationUnitDto
            {
                Id = entity.Id,
                NameEn = entity.NameEn,
                NameTh = entity.NameTh,
                Code = entity.Code,
                Type = entity.Type,
                ShortName = entity.ShortName,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล OrganizationUnit: {Message}", ex.Message);
            throw;
        }
    }
    public async Task<List<OrganizationUnitPathItemDto>> GetPathAsync(Guid organizationUnitId)
    {
        var units = await _repos.FirstOrDefaultAsync(u => u.Id == organizationUnitId && !u.IsDelete);

        if (units == null)
            return new List<OrganizationUnitPathItemDto>();

        return new List<OrganizationUnitPathItemDto>
        {
            new OrganizationUnitPathItemDto
            {
                Id = units.Id,
                NameTh = units.NameTh,
                NameEn = units.NameEn,
                LevelTypeNameTh = units.Type,
                LevelTypeNameEn = units.Type
            }
        };
    }

    public async Task<(IEnumerable<OrganizationUnitDto> Items, int TotalCount)> GetFilteredPagedAsync(OrganizationUnitFilterDto filter)
    {
        try
        {
            var (items, totalCount) = await _repos.GetFilteredPagedAsync(filter);

            var dtos = items.Select(s => new OrganizationUnitDto
            {
                Id = s.Id,
                Code = s.Code,
                NameTh = s.NameTh,
                NameEn = s.NameEn,
                ShortName = s.ShortName,
                Type = s.Type,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();

            return (dtos, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล OrganizationUnit (Paged): {Message}", ex.Message);
            throw;
        }
    }
    public async Task<OrganizationUnitDto?> CreateAsync(CreateOrganizationUnitDto data)
    {
        try
        {
            var duplicateFields = new List<string>();

            if (await _repos.AnyAsync(x => x.NameTh == data.NameTh && !x.IsDelete))
                duplicateFields.Add("NameTh");

            if (await _repos.AnyAsync(x => x.NameEn == data.NameEn && !x.IsDelete))
                duplicateFields.Add("NameEn");

            if (!string.IsNullOrEmpty(data.Code) && await _repos.AnyAsync(x => x.Code == data.Code && !x.IsDelete))
                duplicateFields.Add("Code");

            if (!string.IsNullOrEmpty(data.ShortName) && await _repos.AnyAsync(x => x.ShortName == data.ShortName && !x.IsDelete))
                duplicateFields.Add("ShortName");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            var newEntity = new TbOrganizationUnit
            {
                NameEn = data.NameEn,
                NameTh = data.NameTh,
                Code = data.Code,
                Type = data.Type,
                ShortName = data.ShortName,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false,
            };

            await _repos.AddAsync(newEntity);
            await _repos.SaveChangesAsync();

            return await GetByIdAsync(newEntity.Id);
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

    public async Task<OrganizationUnitDto?> UpdateAsync(Guid id, CreateOrganizationUnitDto data)
    {
        try
        {
            var existingEntity = await _repos.GetByIdAsync(id); // Tracked, ไม่ Filter IsDelete
            if (existingEntity == null)
                return null;

            var duplicateFields = new List<string>();

            if (await _repos.AnyAsync(x => x.NameTh == data.NameTh && !x.IsDelete && x.Id != id))
                duplicateFields.Add("NameTh");

            if (await _repos.AnyAsync(x => x.NameEn == data.NameEn && !x.IsDelete && x.Id != id))
                duplicateFields.Add("NameEn");

            if (!string.IsNullOrEmpty(data.Code) && await _repos.AnyAsync(x => x.Code == data.Code && !x.IsDelete && x.Id != id))
                duplicateFields.Add("Code");

            if (!string.IsNullOrEmpty(data.ShortName) && await _repos.AnyAsync(x => x.ShortName == data.ShortName && !x.IsDelete && x.Id != id))
                duplicateFields.Add("ShortName");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            existingEntity.NameEn = data.NameEn;
            existingEntity.NameTh = data.NameTh;
            existingEntity.Code = data.Code;
            existingEntity.Type = data.Type;
            existingEntity.ShortName = data.ShortName;
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.SaveChangesAsync();

            return await GetByIdAsync(id);
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล OrganizationUnits: {Message}", ex.Message);
            throw;
        }
    }
}