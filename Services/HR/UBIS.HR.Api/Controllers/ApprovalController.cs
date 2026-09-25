using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ApprovalController : ControllerBase
{
    private readonly IApprovalService _service;

    public ApprovalController(IApprovalService service)
    {
        _service = service;
    }

    [HttpGet("my-pending")]
    public async Task<IActionResult> GetMyPending()
    {
        return Ok(await _service.GetMyPendingApprovalsAsync());
    }

    [HttpPost("{transApproveId}/approve")]
    public async Task<IActionResult> Approve(int transApproveId)
    {
        try
        {
            var result = await _service.ApproveAsync(transApproveId);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{transApproveId}/deny")]
    public async Task<IActionResult> Deny(int transApproveId, RejectApprovalDto data)
    {
        try
        {
            var result = await _service.DenyAsync(transApproveId, data.Reason, data.IsDisapprove);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
    [HttpGet("trail")]
    public async Task<IActionResult> GetTrail([FromQuery] string docType, [FromQuery] string docNumber)
    {
        return Ok(await _service.GetTrailAsync(docType, docNumber));
    }
}