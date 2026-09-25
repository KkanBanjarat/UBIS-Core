using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserEmail()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var email = user?.FindFirst(ClaimTypes.Email)?.Value
        ?? user?.FindFirst("preferred_username")?.Value;

        return email ?? "System"; // เผื่อกรณีไม่มี context จริง (เช่น เรียกนอก HTTP request)
    }

    public Guid GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("ไม่พบ User Id ใน token");

        return userId;
    }

    public string GetCurrentUserEmployeeCode()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var code = user?.FindFirst("employeeCode")?.Value;

        if (string.IsNullOrEmpty(code))
            throw new UnauthorizedAccessException("ไม่พบ Employee Code ใน token");

        return code;
    }

    public string? GetScope(string permissionCode)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var claims = user?.Claims.Where(c => c.Type == "perm").Select(c => c.Value).ToList() ?? new List<string>();

        // SuperAdmin ("*") ถือว่ามีทุก Permission ด้วย Scope สูงสุดเสมอ
        var wildcard = claims.FirstOrDefault(c => c == "*" || c.StartsWith("*:"));
        if (wildcard != null)
            return wildcard.Contains(':') ? wildcard.Split(':')[1] : "All";

        var claim = claims.FirstOrDefault(c => c.StartsWith(permissionCode + ":"));
        return claim?.Split(':')[1];
    }
}