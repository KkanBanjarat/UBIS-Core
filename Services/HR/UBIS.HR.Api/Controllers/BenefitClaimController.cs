using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Api.Authorization;

namespace UBIS.HR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BenefitClaimController : ControllerBase
{
    private readonly IBenefitClaimService _service;

    public BenefitClaimController(IBenefitClaimService service)
    {
        _service = service;
    }
    // [MenuAuthorize("benefit-claim", MenuLevel.Read)]
    [HttpPost("benefit-claim-list")]
    public async Task<IActionResult> GetPaged(BenefitClaimFilterDto filter)
        => Ok(await _service.GetAllAsync(filter));

    // [MenuAuthorize("benefit-claim", MenuLevel.Read)]
    [HttpGet("by-docnum/{docNum}")]
    public async Task<IActionResult> GetByDocNum(string docNum)
    {
        var result = await _service.GetByDocNumAsync(docNum);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }
    [MenuAuthorize("benefit-claim", MenuLevel.Write)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateBenefitClaimDto data)
    {
        try { return Ok(await _service.CreateAsync(data)); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreateBenefitClaimDto data)
    {
        try
        {
            var result = await _service.UpdateAsync(id, data);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(Guid id)
    {
        try
        {
            var result = await _service.SubmitAsync(id);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPost("{id}/recall")]
    public async Task<IActionResult> Recall(Guid id)
    {
        try
        {
            var result = await _service.RecallAsync(id);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }
}