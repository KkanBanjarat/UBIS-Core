
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeeTypesController : ControllerBase
{
    private readonly IEmployeeTypeService _service;

    public EmployeeTypesController(IEmployeeTypeService service)
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
}