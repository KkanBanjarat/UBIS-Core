using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PettyCashController : ControllerBase
{
    private readonly IPettyCashService _service;

    public PettyCashController(IPettyCashService service)
    {
        _service = service;
    }

    [HttpPost("petty-cash-list")]
    public async Task<IActionResult> GetPaged(PettyCashFilterDto filter)
    {
        return Ok(await _service.GetAllAsync(filter));
    }

    [HttpGet("by-docnum/{docNum}")]
    public async Task<IActionResult> GetByDocNum(string docNum)
    {
        var result = await _service.GetByDocNumAsync(docNum);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePettyCashDto data)
    {
        return Ok(await _service.CreateAsync(data));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreatePettyCashDto data)
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
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(Guid id)
    {
        try
        {
            var result = await _service.SubmitAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id}/recall")]
    public async Task<IActionResult> Recall(Guid id)
    {
        try
        {
            var result = await _service.RecallAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}