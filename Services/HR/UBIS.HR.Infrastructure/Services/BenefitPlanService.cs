using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class BenefitPlanService : IBenefitPlanService
{
    private readonly IBenefitPlanRepos _repos;
    private readonly IBenefitPlanItemRepos _benefitPlanItemRepos;
    private readonly ILogger<BenefitPlanService> _logger;
    private readonly ICurrentUserService _currentUser;

    public BenefitPlanService(IBenefitPlanRepos repos,
        IBenefitPlanItemRepos benefitPlanItemRepos,
        ILogger<BenefitPlanService> logger,
        ICurrentUserService currentUser)
    {
        _repos = repos;
        _benefitPlanItemRepos = benefitPlanItemRepos;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<BenefitPlanDto>> GetAllAsync()
    {
        try
        {
            var benefits = await _repos.GetAllAsync();

            return benefits.Select(s => new BenefitPlanDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameTh = s.NameTh,
                IsActive = s.IsActive,
                Description = s.Description,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Benefits Plan: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitPlanDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _repos.GetByIdAsync(id);
            if (entity == null)
                return null;

            return new BenefitPlanDto
            {
                Id = entity.Id,
                NameTh = entity.NameTh,
                NameEn = entity.NameEn,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Benefit Plans : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitPlanDto> CreateAsync(CreateBenefitPlanDto data)
    {
        try
        {
            var duplicateFields = new List<string>();

            if (await _repos.AnyAsync(x => x.NameTh == data.NameTh))
                duplicateFields.Add("NameTh");

            if (await _repos.AnyAsync(x => x.NameEn == data.NameEn))
                duplicateFields.Add("NameEn");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            var newEntity = new TbBenefitPlan
            {
                NameEn = data.NameEn,
                NameTh = data.NameTh,
                Description = data.Description,
                IsActive = data.IsActive,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false
            };

            await _repos.AddAsync(newEntity);
            await _repos.SaveChangesAsync();

            return (await GetByIdAsync(newEntity.Id))!;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Benefit Plans : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitPlanDto?> UpdateAsync(Guid id, CreateBenefitPlanDto data)
    {
        try
        {
            var existingEntity = await _repos.GetByIdAsync(id);
            if (existingEntity == null)
                return null;

            var duplicateFields = new List<string>();

            if (await _repos.AnyAsync(x => x.NameTh == data.NameTh && x.Id != id))
                duplicateFields.Add("NameTh");

            if (await _repos.AnyAsync(x => x.NameEn == data.NameEn && x.Id != id))
                duplicateFields.Add("NameEn");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            existingEntity.NameEn = data.NameEn;
            existingEntity.NameTh = data.NameTh;
            existingEntity.Description = data.Description;
            existingEntity.IsActive = data.IsActive;
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.UpdateAsync(existingEntity);
            await _repos.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Benefit Plans: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            Console.WriteLine($"DeleteAsync id = {id}");
            var existingEntity = await _repos.GetByIdAsync(id);
            if (existingEntity == null)
                return false;

            var plans = await _benefitPlanItemRepos.GetListAsync(w => w.BenefitPlanId == id && w.IsDelete == false);
            if (plans.Any())
                throw new InvalidOperationException("ไม่สามารถลบกลุ่มสวัสดิการนี้ได้ เนื่องจากยังมีรายการสวัสดิการอยู่ภายในกลุ่ม กรุณาตรวจสอบ");


            existingEntity.IsDelete = true;
            existingEntity.DeletedBy = _currentUser.GetCurrentUserEmail();
            existingEntity.DeletedAt = DateTime.Now;

            await _repos.UpdateAsync(existingEntity);
            await _repos.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล Benefit Plan : {Message}", ex.Message);
            throw;
        }
    }
}