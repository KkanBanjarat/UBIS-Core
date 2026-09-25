using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Repositories.Interfaces;

namespace UBIS.Access.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _config;
    private readonly IUserRepos _userRepos;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRoleService _userRoleService;
    public AuthService(ILogger<AuthService> logger,

    IConfiguration config,
    ICurrentUserService currentUser,
    IUserRoleService userRoleService,
    IUserRepos userrepos)
    {
        _userRepos = userrepos;
        _logger = logger;
        _config = config;
        _currentUser = currentUser;
        _userRoleService = userRoleService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto data)
    {
        try
        {
            var user = await _userRepos.GetByEmailAsync(data.Email);

            if (user == null || !user.IsActive || user.PasswordHash == null)
                return null;

            var isValid = BCrypt.Net.BCrypt.Verify(data.Password, user.PasswordHash);
            if (!isValid) return null;

            var permissions = await _userRoleService.GetEffectivePermissionsAsync(user.Id);
            var (tokenString, expiresAt) = GenerateJwtToken(user, permissions);

            await _userRepos.UpdateLastLoginAtAsync(user.Id);

            return new LoginResponseDto
            {
                Token = tokenString,
                ExpiresAt = expiresAt,
                Email = user.Email,
                DisplayName = user.DisplayName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการ Login: {Message}", ex.Message);
            throw;
        }
    }
    private (string Token, DateTime ExpiresAt) GenerateJwtToken(TbUser user, List<EffectivePermissionDto> permissions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecret = _config["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret ยังไม่ได้ตั้งค่า");

        var key = Encoding.UTF8.GetBytes(jwtSecret);
        var expiresInMinutes = Convert.ToDouble(_config["Jwt:ExpiresInMinutes"] ?? "480");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("employeeCode", user.EmployeeCode ?? ""),
        };
        foreach (var p in permissions)
            claims.Add(new Claim("perm", $"{p.PermissionCode}:{p.Scope}"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
    }
    public async Task SyncUsersAsync(List<UserDto> users)
    {
        try
        {
            if (users == null || !users.Any())
                return;

            var entraIds = users
                .Select(x => x.EntraObjectId)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x!)
                .Distinct()
                .ToList();

            var existingUsers = await _userRepos.FindAsync(x => x.EntraObjectId != null && entraIds.Contains(x.EntraObjectId));

            var existingDict = existingUsers.Where(x => x.EntraObjectId != null).ToDictionary(x => x.EntraObjectId!);

            var insertUsers = new List<TbUser>();

            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.EntraObjectId)) continue;

                if (existingDict.TryGetValue(user.EntraObjectId, out var existingUser))
                {
                    existingUser.DisplayName = user.DisplayName;
                    existingUser.Email = user.Email;
                    existingUser.IsActive = user.IsActive;
                    existingUser.EmployeeCode = user.EmployeeCode;

                    // ✅ ใช้ repository
                    await _userRepos.UpdateAsync(existingUser);
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
                        CreatedBy = _currentUser.GetCurrentUserEmail(),
                        CreatedAt = DateTime.Now,
                        UpdatedBy = _currentUser.GetCurrentUserEmail(),
                        UpdatedAt = DateTime.Now,
                    });
                }
            }
            if (insertUsers.Any())
                await _userRepos.AddRangeAsync(insertUsers);

            await _userRepos.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการ Sync User : {Message}", ex.Message);
            throw;
        }
    }
    public async Task<LoginResponseDto> SyncCurrentUserAsync(string entraObjectId, string email, string displayName)
    {
        try
        {
            var user = await _userRepos.GetByEntraObjectIdAsync(entraObjectId);

            if (user != null)
            {
                user.LastLoginAt = DateTime.Now;
                user.Email = email;
                user.DisplayName = displayName;

                await _userRepos.UpdateAsync(user);
            }
            else
            {
                user = new TbUser
                {
                    EntraObjectId = entraObjectId,
                    DisplayName = displayName,
                    Email = email,
                    IsActive = true,
                    LastLoginAt = DateTime.Now,
                    CreatedBy = _currentUser.GetCurrentUserEmail(),
                    CreatedAt = DateTime.Now,
                    UpdatedBy = _currentUser.GetCurrentUserEmail(),
                    UpdatedAt = DateTime.Now,
                };

                await _userRepos.AddAsync(user);
            }

            await _userRepos.SaveChangesAsync();

            var permissions = await _userRoleService.GetEffectivePermissionsAsync(user.Id);
            var (tokenString, expiresAt) = GenerateJwtToken(user, permissions);

            return new LoginResponseDto
            {
                Token = tokenString,
                ExpiresAt = expiresAt,
                Email = user.Email,
                DisplayName = user.DisplayName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการ Sync My User : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<UserLookupDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepos.GetByEmailAsync(email);
        if (user == null) return null;

        return new UserLookupDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            EmployeeCode = user.EmployeeCode,
            IsActive = user.IsActive,
        };
    }

    public async Task<UserLookupDto?> LookupUserAsync(
    Guid? employeeId,
    string? employeeCode,
    string? email)
    {
        TbUser? user = null;

        if (!string.IsNullOrWhiteSpace(employeeCode))
            user = await _userRepos.GetByEmployeeCodeAsync(employeeCode);

        if (user == null && employeeId.HasValue)
            user = await _userRepos.GetByEmployeeIdAsync(employeeId.Value);

        if (user == null && !string.IsNullOrWhiteSpace(email))
            user = await _userRepos.GetByEmailAsync(email);

        if (user == null || !user.IsActive)
            return null;

        return new UserLookupDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            EmployeeId = user.EmployeeId,
            EmployeeCode = user.EmployeeCode,
            IsActive = user.IsActive,
        };
    }
}