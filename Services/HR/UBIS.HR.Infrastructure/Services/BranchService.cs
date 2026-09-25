using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly IBranchRepos _branchRepos;
    private readonly ILogger<BranchService> _logger;

    public BranchService(IBranchRepos branchRepos,
        ILogger<BranchService> logger)
    {
        _branchRepos = branchRepos;
        _logger = logger;
    }

    public async Task<IEnumerable<BranchDto>> GetAllAsync()
    {
        try
        {
            var branches = await _branchRepos.GetAllAsync();

            return branches.Select(s => new BranchDto
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                CompanyNameEn = s.Company.NameEn,
                CompanyNameTh = s.Company.NameTh,
                CompanyCode = s.Company.Code,
                CompanyGroupName = s.Company.GroupName,
                NameEn = s.NameEn,
                NameTh = s.NameTh,
                Code = s.Code,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Branches : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BranchDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _branchRepos.GetByIdAsync(id);
            if (entity == null)
                return null;

            return new BranchDto
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                CompanyNameEn = entity.Company.NameEn,
                CompanyNameTh = entity.Company.NameTh,
                CompanyCode = entity.Company.Code,
                CompanyGroupName = entity.Company.GroupName,
                NameEn = entity.NameEn,
                NameTh = entity.NameTh,
                Code = entity.Code,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Branches : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<BranchDto>> GetByCompanyIdAsync(Guid companyId)
    {
        try
        {
            var branches = await _branchRepos.GetByCompanyIdAsync(companyId);

            return branches.Select(s => new BranchDto
            {
                Id = s.Id,
                Code = s.Code,
                NameTh = s.NameTh,
                NameEn = s.NameEn,
                CompanyId = s.CompanyId,
                CompanyNameEn = s.Company.NameEn,
                CompanyNameTh = s.Company.NameTh,
                CompanyCode = s.Company.Code,
                CompanyGroupName = s.Company.GroupName,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Branches by Company : {Message}", ex.Message);
            throw;
        }
    }
}