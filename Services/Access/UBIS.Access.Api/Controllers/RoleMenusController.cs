using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;

namespace UBIS.Access.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/roles/{roleId:guid}/menus")]
public class RoleMenusController : ControllerBase
{
    private readonly IRoleMenuService _service;
    public RoleMenusController(IRoleMenuService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(Guid roleId)
    {
        var result = await _service.GetAsync(roleId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(Guid roleId, UpdateRoleMenusDto dto)
    {
        try
        {
            return await _service.UpdateAsync(roleId, dto) ? NoContent() : NotFound();
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }
}