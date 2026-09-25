using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttachmentController : ControllerBase
{
    private readonly IAttachmentService _service;

    public AttachmentController(IAttachmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetByDocument([FromQuery] string docType, [FromQuery] string docNumber)
    {
        return Ok(await _service.GetByDocumentAsync(docType, docNumber));
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] string docType, [FromForm] string docNumber, IFormFile file)
    {
        try
        {
            return Ok(await _service.UploadAsync(docType, docNumber, file));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var result = await _service.DownloadAsync(id);
        if (result == null) return NotFound();

        var (content, contentType, fileName) = result.Value;

        var encodedFileName = Uri.EscapeDataString(fileName);
        Response.Headers.Append(
            "Content-Disposition",
            $"attachment; filename=\"{encodedFileName}\"; filename*=UTF-8''{encodedFileName}"
        );

        return File(content, contentType);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}