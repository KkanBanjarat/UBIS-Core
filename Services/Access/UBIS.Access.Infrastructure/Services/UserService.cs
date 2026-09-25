using Microsoft.EntityFrameworkCore;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Data;

namespace UBIS.Access.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AccessDbContext _db;
    public UserService(AccessDbContext db) => _db = db;

    public async Task<List<UserListDto>> GetAllAsync()
    {
        return await _db.TbUsers
            .Where(u => !u.IsDelete)
            .OrderBy(u => u.DisplayName)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                Email = u.Email,
                DisplayName = u.DisplayName,
                EmployeeId = u.EmployeeId,
                EmployeeCode = u.EmployeeCode,
                IsActive = u.IsActive,
                IsEntra = u.EntraObjectId != null,
                LastLoginAt = u.LastLoginAt,
                Roles = u.TbUserRoles
                    .Where(ur => !ur.IsDelete && !ur.Role.IsDelete)
                    .Select(ur => ur.Role.Name + ":" + ur.Scope)
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<UserListDto?> CreateAsync(CreateLocalUserDto dto, string by)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(dto.DisplayName))
            throw new InvalidOperationException("กรุณากรอกอีเมลและชื่อแสดงผล");

        if (await _db.TbUsers.AnyAsync(u => !u.IsDelete && u.Email.ToLower() == email))
            throw new InvalidOperationException("อีเมลนี้มีอยู่ในระบบแล้ว");

        var now = DateTime.UtcNow;
        var user = new TbUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = dto.DisplayName.Trim(),
            EmployeeCode = dto.EmployeeCode?.Trim(),
            IsActive = true,
            IsDelete = false,
            CreatedBy = by,
            CreatedAt = now,
            UpdatedBy = by,
            UpdatedAt = now
        };
        _db.TbUsers.Add(user);
        await _db.SaveChangesAsync();

        return (await GetAllAsync()).FirstOrDefault(x => x.Id == user.Id);
    }

    public async Task<UserListDto?> UpdateAsync(Guid id, UpdateUserDto dto, string by)
    {
        var user = await _db.TbUsers.FirstOrDefaultAsync(u => u.Id == id && !u.IsDelete);
        if (user is null) return null;

        // Email / ชื่อ แก้ได้เฉพาะ User local (User Entra ให้ Entra เป็นเจ้าของข้อมูล)
        if (user.EntraObjectId is null)
        {
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var email = dto.Email.Trim().ToLowerInvariant();
                if (await _db.TbUsers.AnyAsync(u => !u.IsDelete && u.Id != id && u.Email.ToLower() == email))
                    throw new InvalidOperationException("อีเมลนี้มีอยู่ในระบบแล้ว");
                user.Email = email;
            }
            if (!string.IsNullOrWhiteSpace(dto.DisplayName))
                user.DisplayName = dto.DisplayName.Trim();
        }

        user.EmployeeCode = dto.EmployeeCode?.Trim();
        user.IsActive = dto.IsActive;
        user.UpdatedBy = by;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (await GetAllAsync()).FirstOrDefault(x => x.Id == id);
    }

    public async Task<bool> DeleteAsync(Guid id, string by)
    {
        var user = await _db.TbUsers.FirstOrDefaultAsync(u => u.Id == id && !u.IsDelete);
        if (user is null) return false;

        var now = DateTime.UtcNow;
        user.IsDelete = true;
        user.IsActive = false;
        user.DeletedBy = by; user.DeletedAt = now;
        user.UpdatedBy = by; user.UpdatedAt = now;
        await _db.SaveChangesAsync();
        return true;
    }
}