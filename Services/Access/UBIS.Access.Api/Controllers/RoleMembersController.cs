using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.Access.Application.Interfaces;

namespace UBIS.Access.Api.Controllers;

[ApiController]
[Route("api/roles/{roleId:guid}/members")]
[Authorize(Policy = "system.admin")]   // ใช้แบบเดียวกับ RoleMenusController
public class RoleMembersController : ControllerBase
{
    private readonly IRoleMemberService _service;

    public RoleMembersController(IRoleMemberService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(Guid roleId)
        => Ok(await _service.GetByRoleIdAsync(roleId));
}