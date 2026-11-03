using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubis.Hr.Api.Services.Email;
using UBIS.HR.Api.Services;
using UBIS.HR.Application.Interfaces;

namespace Ubis.Hr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailTestController : ControllerBase
{
    private readonly IEmailService _email;
    public EmailTestController(IEmailService email) => _email = email;

    [HttpPost]
    public async Task<IActionResult> Send([FromQuery] string to)
    {
        var info = new EmailDocInfo(
            "ใบเบิกสวัสดิการ", "BC-TEST-0001", "ทดสอบ ระบบ", 1234.50m,
            "หัวหน้าแผนก", "ผู้อนุมัติทดสอบ", "ข้อความทดสอบ",
            "http://localhost:5173");
        var (subject, html) = EmailTemplates.ApprovalRequest(info);
        await _email.SendAsync(to, subject, html);
        return Ok("ส่งแล้ว ตรวจสอบกล่องจดหมาย (และ Junk)");
    }
}