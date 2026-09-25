using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly IPositionService _service;

    public PositionsController(IPositionService service)
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
    public async Task<IActionResult> GetById(Guid Id)
    {
        var datas = await _service.GetByIdAsync(Id);

        if(datas == null)
            return NotFound();

        return Ok(datas);
    }
    [HttpPost]
    public async Task<IActionResult> Create(CreatePositionDto data)
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
    [HttpPost("position-list")]
    public async Task<ActionResult<PagedResultDto<PositionDto>>> GetAll(PositionFilterDto filter)
    {
        var (items, totalCount) = await _service.GetFilteredPagedAsync(filter);
        return Ok(new PagedResultDto<PositionDto> { Items = items.ToList(), TotalCount = totalCount });
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, CreatePositionDto data)
    {
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
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }
    
}