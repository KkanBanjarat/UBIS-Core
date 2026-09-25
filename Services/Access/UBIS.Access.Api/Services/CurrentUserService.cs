using System.Security.Claims;
using UBIS.Access.Application.Interfaces;

namespace UBIS.Access.Api.Services;

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
}