using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.Access.Application.Interfaces;

namespace UBIS.Access.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/me")]
public class MeController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MeController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet("menus")]
    public async Task<IActionResult> GetMenus()
    {
        // JwtBearer แปลง claim "sub" เป็น NameIdentifier ให้อัตโนมัติ จึงเช็คทั้งสองชื่อ
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(sub, out var userId))
            return Unauthorized();

        var menus = await _menuService.GetMyMenusAsync(userId);
        return Ok(menus);
    }
}