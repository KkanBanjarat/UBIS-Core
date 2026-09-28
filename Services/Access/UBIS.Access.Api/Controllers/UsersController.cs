using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;

namespace UBIS.Access.Api.Controllers;

[Authorize(Policy = "system.admin")]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;
    private readonly IEntraGraphService _graph;

    public UsersController(IUserService users, IEntraGraphService graph)
    {
        _users = users;
        _graph = graph;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _users.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _users.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("user-list")]
    public async Task<ActionResult<PagedResultDto<UserListDto>>> GetAll(UserFilterDto filter)
    {
        var result = await _users.GetAllAsync(filter);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        try { return Ok(await _users.CreateAsync(dto)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserDto dto)
    {
        try
        {
            var result = await _users.UpdateAsync(id, dto);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> ResetPassword(Guid id, ResetPasswordDto dto)
    {
        try { return await _users.ResetPasswordAsync(id, dto.NewPassword) ? NoContent() : NotFound(); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { return await _users.DeleteAsync(id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPost("sync-entra")]
    public async Task<IActionResult> SyncEntra([FromBody] List<EntraUserInputDto> users)
    {
        if (users == null || users.Count == 0)
            return BadRequest("ไม่มีรายชื่อให้ Sync");
        if (users.Count > 20000)
            return BadRequest("รายชื่อมากเกินไป");
        if (users.Any(u => !Guid.TryParse(u.EntraObjectId, out _)))
            return BadRequest("EntraObjectId ไม่ถูกต้อง");

        var list = users.Select(u => new UserDto
        {
            EntraObjectId = u.EntraObjectId,
            Email = u.Email,
            DisplayName = u.DisplayName,
            EmployeeCode = u.EmployeeCode,
            IsActive = u.IsActive,
        }).ToList();

        return Ok(await _users.SyncUsersAsync(list));
    }
}