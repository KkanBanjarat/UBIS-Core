//EmailTemplates.cs
using System.Globalization;
using System.Net;
using System.Text;

namespace Ubis.Hr.Api.Services.Email;

public record EmailDocInfo(
    string DocTypeLabel,      // เช่น "ใบเบิกสวัสดิการ" / "ใบเบิกเงินสดย่อย"
    string DocNumber,
    string RequesterName,
    decimal? Amount,
    string? StepName,         // ขั้นที่รออนุมัติ / ขั้นที่ดำเนินการ
    string? ActorName,        // ผู้อนุมัติ (ใช้ในเมล์อนุมัติ/ไม่อนุมัติ)
    string? Remark,           // เหตุผล (กรณีไม่อนุมัติ)
    string Url                // ลิงก์เปิดเอกสาร
);

public static class EmailTemplates
{
    private const string Blue = "#3b5bdb";
    private const string Green = "#2f9e44";
    private const string Red = "#e03131";

    public static (string Subject, string Html) ApprovalRequest(EmailDocInfo d) => (
        $"[รออนุมัติ] {d.DocTypeLabel} {d.DocNumber} - {d.RequesterName}",
        Layout(Blue, "รออนุมัติ",
            "มีเอกสารรอการอนุมัติจากคุณ",
            $"{Enc(d.RequesterName)} ได้ส่ง{Enc(d.DocTypeLabel)}เพื่อขออนุมัติ กรุณาตรวจสอบและดำเนินการ",
            d, showActor: false, showRemark: false));

    public static (string Subject, string Html) Approved(EmailDocInfo d) => (
        $"[อนุมัติแล้ว] {d.DocTypeLabel} {d.DocNumber}",
        Layout(Green, "อนุมัติแล้ว",
            "เอกสารของคุณได้รับการอนุมัติ",
            $"{Enc(d.DocTypeLabel)}ของคุณผ่านการอนุมัติครบทุกขั้นตอนแล้ว",
            d, showActor: true, showRemark: false));

    public static (string Subject, string Html) Rejected(EmailDocInfo d) => (
        $"[ไม่อนุมัติ] {d.DocTypeLabel} {d.DocNumber}",
        Layout(Red, "ไม่อนุมัติ",
            "เอกสารของคุณไม่ได้รับการอนุมัติ",
            $"{Enc(d.DocTypeLabel)}ของคุณถูกปฏิเสธ กรุณาตรวจสอบเหตุผลด้านล่าง",
            d, showActor: true, showRemark: true));

    // ---------------------------------------------------------------

    private static string Layout(string color, string badge, string title, string intro,
        EmailDocInfo d, bool showActor, bool showRemark)
    {
        var rows = new StringBuilder();
        Row(rows, "ประเภทเอกสาร", d.DocTypeLabel);
        Row(rows, "เลขที่เอกสาร", d.DocNumber);
        Row(rows, "ผู้ขอ", d.RequesterName);
        if (d.Amount.HasValue)
            Row(rows, "จำนวนเงิน", d.Amount.Value.ToString("N2", CultureInfo.InvariantCulture) + " บาท");
        if (!string.IsNullOrWhiteSpace(d.StepName)) Row(rows, "ขั้นตอน", d.StepName!);
        if (showActor && !string.IsNullOrWhiteSpace(d.ActorName)) Row(rows, "ผู้ดำเนินการ", d.ActorName!);
        if (showRemark && !string.IsNullOrWhiteSpace(d.Remark)) Row(rows, "เหตุผล", d.Remark!);

        return $@"<!DOCTYPE html>
<html lang=""th""><head><meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<title>{Enc(title)}</title></head>
<body style=""margin:0;padding:0;background:#f1f3f9;font-family:'Sarabun','Tahoma',Arial,sans-serif;"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f1f3f9;padding:24px 12px;"">
 <tr><td align=""center"">
  <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;width:100%;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(30,41,59,.08);"">
   <tr><td style=""background:{color};padding:20px 28px;color:#ffffff;font-size:18px;font-weight:bold;"">
     UBIS HR
   </td></tr>
   <tr><td style=""padding:28px 28px 8px 28px;"">
     <span style=""display:inline-block;background:{color}1a;color:{color};font-size:13px;font-weight:bold;padding:4px 12px;border-radius:999px;"">{Enc(badge)}</span>
     <h1 style=""margin:14px 0 6px 0;font-size:22px;color:#1e293b;"">{Enc(title)}</h1>
     <p style=""margin:0;font-size:15px;line-height:1.6;color:#475569;"">{intro}</p>
   </td></tr>
   <tr><td style=""padding:18px 28px;"">
     <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border:1px solid #e2e8f0;border-radius:8px;border-collapse:separate;"">
      {rows}
     </table>
   </td></tr>
   <tr><td align=""center"" style=""padding:8px 28px 32px 28px;"">
     <a href=""{Enc(d.Url)}"" style=""display:inline-block;background:{color};color:#ffffff;text-decoration:none;font-size:15px;font-weight:bold;padding:12px 32px;border-radius:8px;"">เปิดเอกสาร</a>
   </td></tr>
   <tr><td style=""background:#f8fafc;padding:16px 28px;font-size:12px;color:#94a3b8;text-align:center;border-top:1px solid #e2e8f0;"">
     อีเมลนี้ส่งอัตโนมัติจากระบบ UBIS HR กรุณาอย่าตอบกลับ
   </td></tr>
  </table>
 </td></tr>
</table>
</body></html>";
    }

    private static void Row(StringBuilder sb, string label, string value) =>
        sb.Append($@"<tr>
<td style=""padding:10px 16px;width:130px;font-size:14px;color:#64748b;border-bottom:1px solid #f1f5f9;"">{Enc(label)}</td>
<td style=""padding:10px 16px;font-size:14px;color:#1e293b;font-weight:bold;border-bottom:1px solid #f1f5f9;"">{Enc(value)}</td></tr>");

    private static string Enc(string s) => WebUtility.HtmlEncode(s);
}