using Microsoft.Extensions.Logging;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepos _repos;
    private readonly ILogger<UserService> _logger;
    private readonly ICurrentUserService _currentUser;

    public UserService(IUserRepos repos, ILogger<UserService> logger, ICurrentUserService currentUser)
    {
        _repos = repos;
        _logger = logger;
        _currentUser = currentUser;
    }

    private string Actor
    {
        get
        {
            var email = _currentUser.GetCurrentUserEmail();
            return string.IsNullOrWhiteSpace(email) ? "System" : email;
        }
    }

    private bool IsSelf(TbUser u) => string.Equals(u.Email, Actor, StringComparison.OrdinalIgnoreCase);
    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    private static bool IsBusinessError(Exception ex) => ex is InvalidOperationException or ArgumentException;

    private static UserDto ToDto(TbUser u) => new()
    {
        Id = u.Id,
        EntraObjectId = u.EntraObjectId,
        DisplayName = u.DisplayName,
        IsActive = u.IsActive,
        Email = u.Email,
        EmployeeCode = u.EmployeeCode,
        CreatedAt = u.CreatedAt,
        CreatedBy = u.CreatedBy,
        UpdatedAt = u.UpdatedAt,
        UpdatedBy = u.UpdatedBy,
    };

    public async Task<List<UserListDto>> GetAllAsync()
    {
        try
        {
            var users = await _repos.GetAllAsync();
            return users
                .Where(s => !s.IsDelete)
                .OrderBy(s => s.DisplayName)
                .Select(s => new UserListDto
                {
                    Id = s.Id,
                    EntraObjectId = s.EntraObjectId,
                    DisplayName = s.DisplayName,
                    IsActive = s.IsActive,
                    Email = s.Email,
                    EmployeeCode = s.EmployeeCode,
                    EmployeeId = s.EmployeeId,
                    IsEntra = !string.IsNullOrEmpty(s.EntraObjectId),
                    LastLoginAt = s.LastLoginAt,
                    CreatedAt = s.CreatedAt,
                    CreatedBy = s.CreatedBy,
                    UpdatedAt = s.UpdatedAt,
                    UpdatedBy = s.UpdatedBy,
                }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล User: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var user = await _repos.GetByIdAsync(id);
            return user == null || user.IsDelete ? null : ToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล User: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<UserDto?> CreateAsync(CreateUserDto data)
    {
        try
        {
            var email = (data.Email ?? "").Trim().ToLowerInvariant();
            var name = (data.DisplayName ?? "").Trim();
            var code = Clean(data.EmployeeCode);

            if (!email.Contains('@')) throw new ArgumentException("รูปแบบอีเมลไม่ถูกต้อง");
            if (name == "") throw new ArgumentException("กรุณากรอกชื่อแสดงผล");
            if (string.IsNullOrWhiteSpace(data.Password) || data.Password.Length < 8)
                throw new ArgumentException("รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร");

            var dup = new List<string>();
            // Email เช็ครวมที่ถูกลบแล้วด้วย เผื่อมี Unique Index
            if (await _repos.AnyAsync(x => x.Email.ToLower() == email)) dup.Add("Email");
            if (code != null && await _repos.AnyAsync(x => x.EmployeeCode == code && !x.IsDelete))
                dup.Add("EmployeeCode");
            if (data.EmployeeId.HasValue &&
                await _repos.AnyAsync(x => x.EmployeeId == data.EmployeeId && !x.IsDelete))
                dup.Add("EmployeeId");
            if (dup.Count > 0)
                throw new InvalidOperationException($"ข้อมูลซ้ำในช่อง: {string.Join(", ", dup)}");

            var now = DateTime.Now;
            var entity = new TbUser
            {
                Email = email,
                DisplayName = name,
                EmployeeCode = code,
                EmployeeId = data.EmployeeId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(data.Password),
                IsActive = data.IsActive,
                IsDelete = false,
                CreatedAt = now,
                CreatedBy = Actor,
                UpdatedAt = now,
                UpdatedBy = Actor,
            };

            await _repos.AddAsync(entity);
            await _repos.SaveChangesAsync();
            return ToDto(entity);
        }
        catch (Exception ex) when (!IsBusinessError(ex))
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล User: {Message}", ex.Message);
            throw;
        }
    }
    public async Task<PagedResultDto<UserListDto>> GetAllAsync(UserFilterDto filter)
    {
        try
        {
            var (users, totalCount) = await _repos.GetFilteredPagedAsync(filter);

            var items = users.Select(s => new UserListDto
            {
                Id = s.Id,
                EntraObjectId = s.EntraObjectId,
                DisplayName = s.DisplayName,
                IsActive = s.IsActive,
                Email = s.Email,
                EmployeeCode = s.EmployeeCode,
                EmployeeId = s.EmployeeId,
                IsEntra = !string.IsNullOrEmpty(s.EntraObjectId),
                LastLoginAt = s.LastLoginAt,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();

            return new PagedResultDto<UserListDto> { Items = items, TotalCount = totalCount };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล User: {Message}", ex.Message);
            throw;
        }
    }
    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto data)
    {
        try
        {
            var entity = await _repos.GetByIdAsync(id);
            if (entity == null || entity.IsDelete) return null;

            if (!data.IsActive && IsSelf(entity))
                throw new InvalidOperationException("ไม่สามารถปิดการใช้งานบัญชีของตัวเองได้");

            var code = Clean(data.EmployeeCode);
            var dup = new List<string>();
            if (code != null && await _repos.AnyAsync(x => x.Id != id && x.EmployeeCode == code && !x.IsDelete))
                dup.Add("EmployeeCode");
            if (data.EmployeeId.HasValue &&
                await _repos.AnyAsync(x => x.Id != id && x.EmployeeId == data.EmployeeId && !x.IsDelete))
                dup.Add("EmployeeId");

            // อีเมล/ชื่อ แก้ได้เฉพาะ User local (User Entra ให้ Entra เป็นเจ้าของข้อมูล)
            if (entity.EntraObjectId == null)
            {
                if (!string.IsNullOrWhiteSpace(data.Email))
                {
                    var email = data.Email.Trim().ToLowerInvariant();
                    if (!email.Contains('@')) throw new ArgumentException("รูปแบบอีเมลไม่ถูกต้อง");
                    if (await _repos.AnyAsync(x => x.Id != id && x.Email.ToLower() == email))
                        dup.Add("Email");
                    else
                        entity.Email = email;
                }
                if (!string.IsNullOrWhiteSpace(data.DisplayName))
                    entity.DisplayName = data.DisplayName.Trim();
            }

            if (dup.Count > 0)
                throw new InvalidOperationException($"ข้อมูลซ้ำในช่อง: {string.Join(", ", dup)}");

            entity.EmployeeCode = code;
            entity.EmployeeId = data.EmployeeId;
            entity.IsActive = data.IsActive;
            entity.UpdatedAt = DateTime.Now;
            entity.UpdatedBy = Actor;

            await _repos.SaveChangesAsync();
            return ToDto(entity);
        }
        catch (Exception ex) when (!IsBusinessError(ex))
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการแก้ไขข้อมูล User: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _repos.GetByIdAsync(id);
            if (entity == null || entity.IsDelete) return false;

            if (IsSelf(entity))
                throw new InvalidOperationException("ไม่สามารถลบบัญชีของตัวเองได้");

            var now = DateTime.Now;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeletedBy = Actor;
            entity.DeletedAt = now;
            entity.UpdatedBy = Actor;
            entity.UpdatedAt = now;

            await _repos.SaveChangesAsync();
            return true;
        }
        catch (Exception ex) when (!IsBusinessError(ex))
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล User: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> ResetPasswordAsync(Guid id, string newPassword)
    {
        try
        {
            var entity = await _repos.GetByIdAsync(id);
            if (entity == null || entity.IsDelete) return false;

            // if (entity.EntraObjectId != null)
            //     throw new InvalidOperationException("ผู้ใช้จาก Entra ใช้ SSO ไม่มีรหัสผ่านในระบบนี้");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                throw new ArgumentException("รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร");

            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            entity.UpdatedAt = DateTime.Now;
            entity.UpdatedBy = Actor;

            await _repos.SaveChangesAsync();
            return true;
        }
        catch (Exception ex) when (!IsBusinessError(ex))
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการตั้งรหัสผ่าน: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<EntraSyncResultDto> SyncUsersAsync(List<UserDto> users)
    {
        try
        {
            var result = new EntraSyncResultDto { Total = users?.Count ?? 0 };
            if (users == null || users.Count == 0) return result;

            var now = DateTime.Now;
            var existing = (await _repos.GetAllAsync()).ToList();   // ต้องรวมที่ถูกลบด้วย (ดูหมายเหตุท้ายคำตอบ)

            var byEntraId = existing.Where(u => u.EntraObjectId != null)
                .GroupBy(u => u.EntraObjectId!).ToDictionary(g => g.Key, g => g.First());
            var byEmail = existing.GroupBy(u => u.Email.ToLower())
                .ToDictionary(g => g.Key, g => g.First());
            var codes = existing.Where(u => u.EmployeeCode != null)
                .Select(u => u.EmployeeCode!).ToHashSet();

            var newUsers = new List<TbUser>();

            foreach (var eu in users)
            {
                var entraId = eu.EntraObjectId;
                var email = (eu.Email ?? "").Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(entraId) || email == "") { result.Skipped++; continue; }

                var name = string.IsNullOrWhiteSpace(eu.DisplayName) ? email : eu.DisplayName.Trim();
                var code = Clean(eu.EmployeeCode);

                TbUser? user = null;
                var isLink = false;

                if (byEntraId.TryGetValue(entraId, out var u1))
                {
                    user = u1;
                }
                else if (byEmail.TryGetValue(email, out var u2))
                {
                    if (u2.EntraObjectId == null) { user = u2; isLink = true; }   // User local → ผูกเข้า Entra
                    else { result.Skipped++; continue; }                          // อีเมลนี้เป็นของ Entra อีกบัญชี
                }

                if (user == null)
                {
                    var created = new TbUser
                    {
                        EntraObjectId = entraId,
                        Email = email,
                        DisplayName = name,
                        EmployeeCode = code != null && codes.Add(code) ? code : null,
                        IsActive = eu.IsActive,
                        IsDelete = false,
                        CreatedAt = now,
                        CreatedBy = Actor,
                        UpdatedAt = now,
                        UpdatedBy = Actor,
                    };
                    newUsers.Add(created);
                    byEntraId[entraId] = created;
                    byEmail[email] = created;
                    result.Created++;
                    continue;
                }

                if (user.IsDelete) { result.Skipped++; continue; }   // Admin ลบไว้ ไม่ปลุกกลับมา

                var changed = false;
                if (user.EntraObjectId != entraId) { user.EntraObjectId = entraId; changed = true; }
                if (user.DisplayName != name) { user.DisplayName = name; changed = true; }
                if (user.Email != email && (!byEmail.TryGetValue(email, out var other) || other.Id == user.Id))
                {
                    user.Email = email;
                    changed = true;
                }
                if (user.EmployeeCode == null && code != null && codes.Add(code))   // เติมเฉพาะที่ยังว่าง
                {
                    user.EmployeeCode = code;
                    changed = true;
                }
                if (!eu.IsActive && user.IsActive) { user.IsActive = false; changed = true; }   // ปิดตาม Entra แต่ไม่เปิดคืน

                if (!changed) { result.Unchanged++; continue; }

                user.UpdatedAt = now;
                user.UpdatedBy = Actor;
                if (isLink) result.Linked++; else result.Updated++;
            }

            if (newUsers.Count > 0) await _repos.AddRangeAsync(newUsers);
            await _repos.SaveChangesAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการ Sync User: {Message}", ex.Message);
            throw;
        }
    }
}