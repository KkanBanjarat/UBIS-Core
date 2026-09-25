using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class EmployeeTypeService : IEmployeeTypeService
{
    private readonly IEmployeeTypeRepos _repos;
    private readonly ILogger<EmployeeTypeService> _logger;

    public EmployeeTypeService(IEmployeeTypeRepos repos,
        ILogger<EmployeeTypeService> logger)
    {
        _repos = repos;
        _logger = logger;
    }

    public async Task<IEnumerable<EmployeeTypeDto>> GetAllAsync()
    {
        try
        {
            var empTypes = await _repos.GetAllAsync();

            return empTypes.Select(s => new EmployeeTypeDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameTh = s.NameTh,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Employee Type : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<EmployeeTypeDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _repos.GetByIdAsync(id);
            if (entity == null)
                return null;

            return new EmployeeTypeDto
            {
                Id = entity.Id,
                NameTh = entity.NameTh,
                NameEn = entity.NameEn,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Employee Type : {Message}", ex.Message);
            throw;
        }
    }
}