
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;

namespace UBIS.Access.Api.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _service;

    public PermissionsController(IPermissionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var datas = await _service.GetAllAsync();
        return Ok(datas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var datas = await _service.GetByIdAsync(id);
        if(datas==null)
            return NotFound();
        return Ok(datas);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePermissionDto data) 
    { 
        if (data == null) return BadRequest("กรุณาระบุข้อมูลที่ต้องการสร้าง");

        try
        {
            var result = await _service.CreateAsync(data);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreatePermissionDto data) { 
        if (data == null) return BadRequest("กรุณาระบุข้อมูลที่ต้องการแก้ไข");

        try
        {
            var result = await _service.UpdateAsync(id, data);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }
}