using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    // [Authorize(Policy = "employee.read")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] bool includeOrgChart = true)
    {
        var datas = await _service.GetByIdAsync(id, includeOrgChart);

        if (datas == null)
            return NotFound();

        return Ok(datas);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentEmployee()
    {
        var employeeCode = User.FindFirst("employeeCode")?.Value;

        if (string.IsNullOrEmpty(employeeCode))
            return Unauthorized(new { message = "ไม่พบรหัสพนักงาน" });

        var employee = await _service.GetByEmployeeCodeAsync(employeeCode);

        if (employee == null)
            return NotFound(new { message = "ไม่พบข้อมูลพนักงาน" });

        return Ok(employee);
    }

    // [Authorize(Policy = "employee.read")]
    [HttpPost("employee-list")]
    public async Task<ActionResult<PagedResultDto<EmployeeDto>>> GetAll(EmployeeFilterDto filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<ActionResult<PagedResultDto<EmployeeSearchDto>>> SearchEmployees([FromBody] SearchEmployeeRequestDto request)
    {
        var result = await _service.SearchEmployeesAsync(request);
        return Ok(result);
    }
    [HttpGet("allowed-for-document")]
    public async Task<IActionResult> GetAllowedForDocument()
    {
        return Ok(await _service.GetAllowedEmployeesForDocumentAsync());
    }

    [Authorize(Policy = "employee.write")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeDto data)
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
    [Authorize(Policy = "employee.write")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, CreateEmployeeDto data)
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
    [Authorize(Policy = "employee.write")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }

}