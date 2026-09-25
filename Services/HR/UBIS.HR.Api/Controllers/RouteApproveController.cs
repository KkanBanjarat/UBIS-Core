using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Api.Controllers;

[Authorize(Policy = "system.admin")]
[ApiController]
[Route("api/[controller]")]
public class RouteApproveController : ControllerBase
{
    private readonly IRouteApproveService _service;

    public RouteApproveController(IRouteApproveService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("doc-types")]
    public async Task<IActionResult> GetDocTypes()
    {
        return Ok(await _service.GetDocTypesAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRouteApproveDto data)
    {
        try
        {
            return Ok(await _service.CreateAsync(data));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreateRouteApproveDto data)
    {
        try
        {
            var result = await _service.UpdateAsync(id, data);
            if (result == null) return NotFound();
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
        if (!result) return NotFound();
        return NoContent();
    }
}