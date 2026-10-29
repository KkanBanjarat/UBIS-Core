using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocLimitController : ControllerBase
{
    private readonly IDocLimitService _service;

    public DocLimitController(IDocLimitService service) => _service = service;

    [HttpGet("{docType}")]
    public async Task<IActionResult> Get(string docType, [FromQuery] DateTime? docDate)
        => Ok(await _service.GetEffectiveAsync(docType, docDate ?? DateTime.Today));
}