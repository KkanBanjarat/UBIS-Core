using ClosedXML.Excel;
using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Infrastructure.Excel;

/// <summary>
/// สร้างไฟล์ Excel รายงานสรุปการเบิกสวัสดิการ (2 ชีต)
/// ชีต 1: บริษัท → สาขา → รายการสวัสดิการ
/// ชีต 2: บริษัท → รายบุคคล → เอกสาร/รายละเอียด
/// </summary>
public static class BenefitClaimSummaryExcelBuilder
{
    public static byte[] Build(
        BenefitClaimSummaryDto summary,
        List<BenefitClaimExportRowDto> rows,
        BenefitClaimSummaryFilterDto filter)
    {
        using var wb = new XLWorkbook();
        BuildCompanySheet(wb.AddWorksheet("สรุปตามบริษัท"), summary, filter);
        BuildEmployeeSheet(wb.AddWorksheet("รายละเอียดรายบุคคล"), rows, filter);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ================= Sheet 1 =================
    private static void BuildCompanySheet(
        IXLWorksheet ws, BenefitClaimSummaryDto summary, BenefitClaimSummaryFilterDto filter)
    {
        const int lastCol = 2;
        ExcelStyles.Init(ws);

        var r = 1;
        ExcelStyles.Title(ws, r++, "รายงานสรุปการเบิกสวัสดิการ (ตามบริษัท / สาขา)", lastCol);
        ws.Cell(r++, 1).Value = FilterText(filter);
        r++;

        var headerRow = r;
        ws.Cell(r, 1).Value = "รายการสวัสดิการ";
        // ws.Cell(r, 2).Value = "จำนวนเอกสาร";
        // ws.Cell(r, 3).Value = "จำนวนรายการ";
        ws.Cell(r, 2).Value = "จำนวนเงิน (บาท)";
        ExcelStyles.HeaderRow(ws, r++, lastCol);

        decimal grandAmount = 0;
        var grandDocs = 0;
        var grandLines = 0;

        foreach (var c in summary.ByCompany)
        {
            ExcelStyles.BandRow(ws, r++, $"{c.Company}", lastCol);

            foreach (var b in c.Branches)
            {
                // ชื่อสาขา
                var branchRow = ws.Range(r, 1, r, lastCol).Merge();
                branchRow.Value = $"สาขา: {b.Branch}";
                branchRow.Style.Font.Bold = true;
                ExcelStyles.Body(branchRow);
                branchRow.Style.Font.Bold = true;
                r++;

                var first = r;
                foreach (var x in b.Benefits)
                {
                    ws.Cell(r, 1).Value = "    " + x.Benefit;
                    // ws.Cell(r, 2).Value = x.DocCount;
                    // ws.Cell(r, 3).Value = x.LineCount;
                    ws.Cell(r, 2).Value = x.Amount;
                    r++;
                }
                if (r > first)
                {
                    ExcelStyles.Body(ws.Range(first, 1, r - 1, lastCol));
                    ExcelStyles.Int(ws.Range(first, 2, r - 1, 3));
                    ExcelStyles.Money(ws.Range(first, 4, r - 1, 4));
                }

                // รวมสาขา
                ws.Cell(r, 1).Value = $"รวมสาขา {b.Branch}";
                // ws.Cell(r, 2).Value = b.DocCount;
                // ws.Cell(r, 3).Value = b.LineCount;
                ws.Cell(r, 2).Value = b.Amount;
                ExcelStyles.TotalRow(ws, r, 1, lastCol, XLAlignmentHorizontalValues.Right);
                ExcelStyles.Int(ws.Range(r, 2, r, 3));
                ExcelStyles.Money(ws.Cell(r, 2).AsRange());
                r++;
            }

            // รวมบริษัท
            ws.Cell(r, 1).Value = $"รวม {c.Company}";
            // ws.Cell(r, 2).Value = c.DocCount;
            // ws.Cell(r, 3).Value = c.LineCount;
            ws.Cell(r, 2).Value = c.Amount;
            ExcelStyles.TotalRow(ws, r, 1, lastCol, XLAlignmentHorizontalValues.Right);
            ExcelStyles.Int(ws.Range(r, 2, r, 3));
            ExcelStyles.Money(ws.Cell(r, 2).AsRange());
            r += 2;

            grandDocs += c.DocCount;
            grandLines += c.LineCount;
            grandAmount += c.Amount;
        }

        // รวมทั้งหมด
        ws.Cell(r, 1).Value = "รวมทั้งหมด";
        // ws.Cell(r, 2).Value = grandDocs;
        // ws.Cell(r, 3).Value = grandLines;
        ws.Cell(r, 2).Value = grandAmount;
        ExcelStyles.TotalRow(ws, r, 1, lastCol, XLAlignmentHorizontalValues.Right);
        ExcelStyles.Int(ws.Range(r, 2, r, 3));
        ExcelStyles.Money(ws.Cell(r, 2).AsRange());

        ExcelStyles.FreezeHeader(ws, headerRow);
        ExcelStyles.AutoFit(ws, 12, 45);
    }

    // ================= Sheet 2 =================
    private static void BuildEmployeeSheet(
        IXLWorksheet ws, List<BenefitClaimExportRowDto> rows, BenefitClaimSummaryFilterDto filter)
    {
        const int lastCol = 9;
        ExcelStyles.Init(ws);

        var r = 1;
        ExcelStyles.Title(ws, r++, "รายละเอียดการเบิกสวัสดิการรายบุคคล", lastCol);
        ws.Cell(r++, 1).Value = FilterText(filter);
        r++;

        var headerRow = r;
        string[] heads =
        {
            "เลขที่เอกสาร", "วันที่", "สถานะ", "รหัสพนักงาน", "ชื่อ-สกุล",
            "สวัสดิการ", "รายละเอียด", "จำนวน", "จำนวนเงิน (บาท)"
        };
        for (var i = 0; i < heads.Length; i++) ws.Cell(r, i + 1).Value = heads[i];
        ExcelStyles.HeaderRow(ws, r++, lastCol);

        decimal grand = 0;

        foreach (var company in rows
                     .GroupBy(x => string.IsNullOrWhiteSpace(x.Company) ? "ไม่ระบุบริษัท" : x.Company)
                     .OrderBy(g => g.Key))
        {
            ExcelStyles.BandRow(ws, r++, $"บริษัท: {company.Key}", lastCol);
            decimal companyTotal = 0;

            foreach (var emp in company
                         .GroupBy(x => new { x.EmployeeCode, x.EmployeeName })
                         .OrderBy(g => g.Key.EmployeeCode))
            {
                var first = r;
                foreach (var x in emp.OrderBy(x => x.DocDate).ThenBy(x => x.DocNum))
                {
                    ws.Cell(r, 1).Value = x.DocNum;
                    ws.Cell(r, 2).Value = x.DocDate;
                    ws.Cell(r, 3).Value = StatusText(x.DocStatus);
                    ws.Cell(r, 4).Value = x.EmployeeCode;
                    ws.Cell(r, 5).Value = x.EmployeeName;
                    ws.Cell(r, 6).Value = x.BenefitName;
                    ws.Cell(r, 7).Value = x.Detail;
                    ws.Cell(r, 8).Value = x.Qty;
                    ws.Cell(r, 9).Value = x.Amount;
                    r++;
                }

                var body = ws.Range(first, 1, r - 1, lastCol);
                ExcelStyles.Body(body);
                ExcelStyles.Date(ws.Range(first, 2, r - 1, 2));
                ExcelStyles.Int(ws.Range(first, 8, r - 1, 8));
                ExcelStyles.Money(ws.Range(first, 9, r - 1, 9));
                ws.Range(first, 7, r - 1, 7).Style.Alignment.WrapText = true;

                var empTotal = emp.Sum(x => x.Amount);
                ws.Cell(r, 1).Value = $"รวม {emp.Key.EmployeeCode} {emp.Key.EmployeeName}";
                ws.Cell(r, 9).Value = empTotal;
                ExcelStyles.TotalRow(ws, r, 1, lastCol);
                ExcelStyles.Money(ws.Cell(r, 9).AsRange());
                r++;

                companyTotal += empTotal;
            }

            ws.Cell(r, 1).Value = $"รวมบริษัท {company.Key}";
            ws.Cell(r, 9).Value = companyTotal;
            ExcelStyles.TotalRow(ws, r, 1, lastCol);
            ExcelStyles.Money(ws.Cell(r, 9).AsRange());
            r += 2;

            grand += companyTotal;
        }

        ws.Cell(r, 1).Value = "รวมทั้งหมด";
        ws.Cell(r, 9).Value = grand;
        ExcelStyles.TotalRow(ws, r, 1, lastCol);
        ExcelStyles.Money(ws.Cell(r, 9).AsRange());

        ExcelStyles.FreezeHeader(ws, headerRow);
        ExcelStyles.AutoFit(ws, 10, 45);
    }

    // ================= helpers =================
    private static string StatusText(string? s) => s switch
    {
        "Draft" => "ร่าง",
        "WaitApprove" => "รออนุมัติ",
        "Approved" => "อนุมัติ",
        "Rejected" => "ตีกลับ",
        "Disapproved" => "ไม่อนุมัติ",
        _ => s ?? "-"
    };

    private static string FilterText(BenefitClaimSummaryFilterDto f)
    {
        var from = f.DateFrom?.ToString("dd/MM/yyyy") ?? "-";
        var to = f.DateTo?.ToString("dd/MM/yyyy") ?? "-";
        var status = string.IsNullOrEmpty(f.DocStatus) ? "ทุกสถานะ (ไม่รวมร่าง)" : StatusText(f.DocStatus);
        return $"ช่วงวันที่: {from} ถึง {to}   |   สถานะ: {status}";
    }
}