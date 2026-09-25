using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Api.Controllers;

[Authorize(Policy = "system.admin")]
[ApiController]
[Route("api/[controller]")]
public class PodAdminBranchesController : ControllerBase
{
    private readonly IPodAdminBranchService _service;

    public PodAdminBranchesController(IPodAdminBranchService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var datas = await _service.GetAllAsync();
        return Ok(datas);
    }

    /// <summary>
    /// สำหรับหน้าจัดการผู้ดูแลสาขา — จัดกลุ่มตามสาขา รวมสาขาที่ยังไม่มีผู้ดูแล
    /// </summary>
    [HttpGet("grouped-by-branch")]
    public async Task<IActionResult> GetGroupedByBranch()
    {
        return Ok(await _service.GetGroupedByBranchAsync());
    }

    /// <summary>
    /// ผู้ใช้ทั่วไปเรียกได้ เพราะระบบอื่นใช้เช็คว่าตัวเองดูแลสาขาไหนบ้าง
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByUserId(Guid id)
    {
        var datas = await _service.GetByUserIdAsync(id);

        if (datas == null)
            return NotFound();

        return Ok(datas);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePodAdminBranchDto data)
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

    /// <summary>
    /// ตั้งเป็นผู้ดูแลหลักของสาขา (1 สาขามีได้คนเดียว ระบบจะปลดคนเดิมให้อัตโนมัติ)
    /// </summary>
    [HttpPost("{id}/set-primary")]
    public async Task<IActionResult> SetPrimary(Guid id)
    {
        try
        {
            var result = await _service.SetPrimaryAsync(id);
            if (!result) return NotFound();
            return NoContent();
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