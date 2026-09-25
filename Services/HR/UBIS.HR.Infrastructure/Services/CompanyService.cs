using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepos _repos;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(ICompanyRepos repos,
        ILogger<CompanyService> logger)
    {
        _repos = repos;
        _logger = logger;
    }

    public async Task<IEnumerable<CompanyDto>> GetAllAsync()
    {
        try
        {
            var companies = await _repos.GetAllAsync();

            return companies.Select(s => new CompanyDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameTh = s.NameTh,
                Code = s.Code,
                GroupName = s.GroupName,
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล companies : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<CompanyDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _repos.GetByIdAsync(id);
            if (entity == null)
                return null;

            return new CompanyDto
            {
                Id = entity.Id,
                NameTh = entity.NameTh,
                NameEn = entity.NameEn,
                Code = entity.Code,
                GroupName = entity.GroupName,
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล companies : {Message}", ex.Message);
            throw;
        }
    }
}