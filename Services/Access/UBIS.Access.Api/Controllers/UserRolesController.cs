using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;

namespace UBIS.Access.Api.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserRolesController : ControllerBase
{
    private readonly IUserRoleService _service;

    public UserRolesController(IUserRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() {
        var datas = await _service.GetAllAsync();
        return Ok(datas);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId) {
        var datas = await _service.GetByUserIdAsync(userId);
        return Ok(datas);
    }
    [HttpPost]
    public async Task<IActionResult> Assign(CreateUserRoleDto data) {
        if (data == null) return BadRequest("กรุณาระบุข้อมูลที่ต้องการสร้าง");

        try
        {
            var result = await _service.AssignAsync(data);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Revoke(Guid id) {
        var result = await _service.RevokeAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }
}