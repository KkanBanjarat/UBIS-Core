
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;

namespace UBIS.Access.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;
    private readonly IConfiguration _config;

    public AuthController(IAuthService service, IConfiguration config)
    {
        _service = service;
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDto data)
    {
        var usr = await _service.LoginAsync(data);
        if (usr == null)
            return Unauthorized();
        return Ok(usr);
    }
    [HttpPost("sync")]
    public async Task<IActionResult> SyncUsersAsync(List<UserDto> data)
    {
        var apiKey = Request.Headers["X-Api-Key"].ToString();
        if (apiKey != _config["ApiKey"])
            return Unauthorized();

        await _service.SyncUsersAsync(data);
        return Ok();
    }

    [Authorize(AuthenticationSchemes = "EntraID")]
    [HttpPost("sync-me")]
    public async Task<IActionResult> SyncCurrentUserAsync()
    {
        var entraObjectId = User.FindFirst("oid")?.Value
            ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        var email = User.FindFirst("preferred_username")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.Upn)?.Value;
        var displayName = User.FindFirst("name")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(entraObjectId))
            return Unauthorized();

        var result = await _service.SyncCurrentUserAsync(entraObjectId, email!, displayName!);
        return Ok(result);
    }
    [HttpGet("lookup")]
    public async Task<IActionResult> LookupUser([FromQuery] Guid? employeeId, [FromQuery] string? employeeCode, [FromQuery] string? email)
    {
        if (!employeeId.HasValue && string.IsNullOrWhiteSpace(employeeCode) && string.IsNullOrWhiteSpace(email))
            return BadRequest("กรุณาระบุ employeeId, employeeCode หรือ email อย่างน้อยหนึ่งอย่าง");

        var apiKey = Request.Headers["X-Api-Key"].ToString();

        if (string.IsNullOrWhiteSpace(apiKey) ||
            apiKey != _config["SecurityHeaders:MySecretValue"])
            return Unauthorized();


        var user = await _service.LookupUserAsync(
            employeeId,
            employeeCode,
            email);

        if (user == null)
            return NotFound(new { message = "ไม่พบบัญชีผู้ใช้งาน" });

        return Ok(user);
    }
}