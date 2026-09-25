using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class BenefitPlanItemService : IBenefitPlanItemService
{
    private readonly IBenefitPlanItemRepos _repos;
    private readonly ILogger<BenefitPlanItemService> _logger;
    private readonly ICurrentUserService _currentUser;

    public BenefitPlanItemService(IBenefitPlanItemRepos repos,
        ILogger<BenefitPlanItemService> logger,
        ICurrentUserService currentUser)
    {
        _repos = repos;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<BenefitPlanItemDto>> GetAllAsync()
    {
        try
        {
            var benefits = await _repos.GetAllWithDetailsAsync();

            return benefits.Select(s => new BenefitPlanItemDto
            {
                Id = s.Id,
                BenefitId = s.BenefitId,
                BenefitNameEn = s.Benefit.NameEn,
                BenefitNameTh = s.Benefit.NameTh,
                BenefitPlanId = s.BenefitPlanId,
                BenefitPlanNameTh = s.BenefitPlan.NameTh,
                BenefitPlanNameEn = s.BenefitPlan.NameEn,
                LimitAmount = s.LimitAmount,
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Benefits Plan Item: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitPlanItemDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var s = await _repos.GetByIdAsync(id);
            if (s == null)
                return null;

            return new BenefitPlanItemDto
            {
                Id = s.Id,
                BenefitId = s.BenefitId,
                BenefitNameEn = s.Benefit.NameEn,
                BenefitNameTh = s.Benefit.NameTh,
                BenefitPlanId = s.BenefitPlanId,
                BenefitPlanNameTh = s.BenefitPlan.NameTh,
                BenefitPlanNameEn = s.BenefitPlan.NameEn,
                LimitAmount = s.LimitAmount,
                IsActive = s.IsActive,
                Description = s.Description,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Benefit Plan Item: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitPlanItemDto> CreateAsync(CreateBenefitPlanItemDto data)
    {
        try
        {
            var duplicateFields = new List<string>();
            if (await _repos.AnyAsync(x => x.BenefitPlanId == data.BenefitPlanId && x.BenefitId == data.BenefitId))
                duplicateFields.Add("Benefit นี้มีอยู่ใน Plan นี้แล้ว");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            var newEntity = new TbBenefitPlanItem
            {
                BenefitId = data.BenefitId,
                BenefitPlanId = data.BenefitPlanId,
                LimitAmount = data.LimitAmount,
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Benefit Plan Item : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitPlanItemDto?> UpdateAsync(Guid id, CreateBenefitPlanItemDto data)
    {
        try
        {
            // ✅ เปลี่ยน: _context.TbBenefitPlanItems.FindAsync()
            // → _repos.GetByIdAsync()
            var existingEntity = await _repos.GetByIdAsync(id);
            if (existingEntity == null)
                return null;

            var duplicateFields = new List<string>();

            // ✅ เปลี่ยน: _context.TbBenefitPlanItems.AnyAsync()
            // → _repos.AnyAsync()
            // Repository ทำ soft delete check อยู่ในตัว + IsDelete == false ใน query
            if (await _repos.AnyAsync(x => x.BenefitPlanId == data.BenefitPlanId && x.BenefitId == data.BenefitId && x.Id != id))
                duplicateFields.Add("Benefit นี้มีอยู่ใน Plan นี้แล้ว");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            existingEntity.Description = data.Description;
            existingEntity.BenefitId = data.BenefitId;
            existingEntity.BenefitPlanId = data.BenefitPlanId;
            existingEntity.LimitAmount = data.LimitAmount;
            existingEntity.IsActive = data.IsActive;
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            // ✅ เปลี่ยน: _context.SaveChangesAsync()
            // → _repos.UpdateAsync() + SaveChangesAsync()
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Benefit Plan Item: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            // ✅ เปลี่ยน: _context.TbBenefitPlanItems.FindAsync()
            // → _repos.GetByIdAsync()
            var existingEntity = await _repos.GetByIdAsync(id);
            if (existingEntity == null)
            {
                return false;
            }

            existingEntity.IsDelete = true;
            existingEntity.DeletedBy = _currentUser.GetCurrentUserEmail();
            existingEntity.DeletedAt = DateTime.Now;

            // ✅ เปลี่ยน: _context.SaveChangesAsync()
            // → _repos.UpdateAsync() + SaveChangesAsync()
            await _repos.UpdateAsync(existingEntity);
            await _repos.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล Benefit Plan Item : {Message}", ex.Message);
            throw;
        }
    }
}