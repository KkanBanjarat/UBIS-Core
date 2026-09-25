using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BranchsController : ControllerBase
{
    private readonly IBranchService _service;

    public BranchsController(IBranchService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var datas = await _service.GetAllAsync();
        return Ok(datas);
    }

    // ✅ เพิ่ม :guid constraint เพื่อให้ match GUID เท่านั้น
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var datas = await _service.GetByIdAsync(id);
        if (datas == null)
            return NotFound();
        return Ok(datas);
    }

    // ✅ Route นี้ match ก่อน (specific route มา ก่อน generic)
    [HttpGet("by-company")]
    public async Task<IActionResult> GetByCompanyId([FromQuery] Guid companyId)
    {
        var datas = await _service.GetByCompanyIdAsync(companyId);
        if (datas == null || !datas.Any())
            return Ok(new List<object>());
        return Ok(datas);
    }
}