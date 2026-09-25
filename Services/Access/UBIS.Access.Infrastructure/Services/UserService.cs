using Microsoft.Extensions.Logging;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepos _repos;
    private readonly ILogger<UserService> _logger;
    private readonly ICurrentUserService _currentUser;

    public UserService(IUserRepos repos,
    ILogger<UserService> logger,
    ICurrentUserService currentUser)
    {
        _repos = repos;
        _logger = logger;
        _currentUser = currentUser;
    }
    public async Task<List<UserListDto>> GetAllAsync()
    {
        try
        {
            var user = await _repos.GetAllAsync();

            return user.Select(s => new UserListDto
            {
                Id = s.Id,
                EntraObjectId = s.EntraObjectId,
                DisplayName = s.DisplayName,
                IsActive = s.IsActive,
                Email = s.Email,
                EmployeeCode = s.EmployeeCode,
                EmployeeId = s.EmployeeId,
                IsEntra = !string.IsNullOrEmpty(s.EntraObjectId),
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
    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var user = await _repos.FirstOrDefaultAsync(w => w.Id == id);
            if (user == null)
                throw new KeyNotFoundException($"ไม่พบข้อมูล");

            return new UserDto
            {
                Id = user.Id,
                EntraObjectId = user.EntraObjectId,
                DisplayName = user.DisplayName,
                IsActive = user.IsActive,
                Email = user.Email,
                EmployeeCode = user.EmployeeCode,
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy,
                UpdatedAt = user.UpdatedAt,
                UpdatedBy = user.UpdatedBy,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Position: {Message}", ex.Message);
            throw;
        }
    }
    public async Task<UserDto?> CreateAsync(CreateUserDto data)
    {
        try
        {
            var duplicateFields = new List<string>();

            if (await _repos.AnyAsync(x => x.EmployeeCode == data.EmployeeCode && !x.IsDelete))
                duplicateFields.Add("EmployeeCode");

            if (await _repos.AnyAsync(x => x.EmployeeId == data.EmployeeId && !x.IsDelete))
                duplicateFields.Add("EmployeeId");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            var newEntity = new TbUser
            {
                DisplayName = data.DisplayName,
                EmployeeCode = data.EmployeeCode,
                EmployeeId = data.EmployeeId,
                EntraObjectId = data.EntraObjectId,
                Email = data.Email,
                IsActive = data.IsActive,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false
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
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Position : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<UserDto?> UpdateAsync(Guid id, CreateUserDto data)
    {
        try
        {
            var existingEntity = await _repos.GetByIdAsync(id);
            if (existingEntity == null)
                return null;

            var duplicateFields = new List<string>();

            if (await _repos.AnyAsync(x => x.EmployeeCode == data.EmployeeCode && x.Id != id && !x.IsDelete))
                duplicateFields.Add("EmployeeCode");

            if (await _repos.AnyAsync(x => x.EntraObjectId == data.EntraObjectId && x.Id != id && !x.IsDelete))
                duplicateFields.Add("EntraObjectId");

            if (duplicateFields.Count > 0)
            {
                var message = $"ข้อมูลซ้ำในช่อง: {string.Join(", ", duplicateFields)}";
                throw new InvalidOperationException(message);
            }

            existingEntity.EmployeeCode = data.EmployeeCode;
            existingEntity.Email = data.Email;
            existingEntity.IsActive = data.IsActive;
            existingEntity.EntraObjectId = data.EntraObjectId;
            existingEntity.EmployeeId = data.EmployeeId;
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
                return false;


            existingEntity.IsDelete = true;
            existingEntity.DeletedBy = _currentUser.GetCurrentUserEmail();
            existingEntity.DeletedAt = DateTime.Now;

            await _repos.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล Users: {Message}", ex.Message);
            throw;
        }
    }

    public async Task SyncUsersAsync(List<UserDto> users)
    {
        try
        {
            if (users == null || users.Count == 0)
                return;

            var entraIds = users
                .Select(x => x.EntraObjectId)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x)
                .Distinct()
                .ToList();

            if (entraIds.Count == 0)
                return;

            var existingUsers = await _repos.FindAsync(x => x.EntraObjectId != null && entraIds.Contains(x.EntraObjectId));

            var existingDict = existingUsers.Where(x => x.EntraObjectId != null).ToDictionary(x => x.EntraObjectId!);

            var insertUsers = new List<TbUser>();
            var currentUser = _currentUser.GetCurrentUserEmail();
            var now = DateTime.Now;

            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.EntraObjectId))
                    continue;

                if (existingDict.TryGetValue(
                    user.EntraObjectId,
                    out var existingUser))
                {
                    existingUser.DisplayName = user.DisplayName;
                    existingUser.Email = user.Email;
                    existingUser.IsActive = user.IsActive;
                    existingUser.EmployeeCode = user.EmployeeCode;
                    existingUser.UpdatedAt = now;
                    existingUser.UpdatedBy = currentUser;
                }
                else
                {
                    insertUsers.Add(new TbUser
                    {
                        EntraObjectId = user.EntraObjectId,
                        DisplayName = user.DisplayName,
                        Email = user.Email,
                        IsActive = user.IsActive,
                        EmployeeCode = user.EmployeeCode,
                        CreatedAt = now,
                        CreatedBy = currentUser,
                        UpdatedAt = now,
                        UpdatedBy = currentUser,
                        IsDelete = false
                    });
                }
            }

            if (insertUsers.Count > 0)
                await _repos.AddRangeAsync(insertUsers);

            await _repos.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "เกิดข้อผิดพลาดในการ Sync User : {Message}",
                ex.Message);

            throw;
        }
    }
}