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
        // สไตล์เดียวกับชีตรายบุคคล: พื้นขาว เส้นดำบาง แถวรวมพื้นเทาอ่อน
        const int lastCol = 2;
        ExcelStyles.Init(ws);

        var title = ws.Range(1, 1, 1, lastCol).Merge();
        title.Value = "รายงานสรุปการเบิกสวัสดิการ (ตามบริษัท / สาขา)";
        title.Style.Font.Bold = true;
        title.Style.Font.FontSize = 14;
        title.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Row(1).Height = 26;

        var sub = ws.Range(2, 1, 2, lastCol).Merge();
        sub.Value = FilterText(filter);
        sub.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        const int headerRow = 4;
        ws.Cell(headerRow, 1).Value = "รายการสวัสดิการ";
        ws.Cell(headerRow, 2).Value = "จำนวนเงิน (บาท)";
        var header = ws.Range(headerRow, 1, headerRow, lastCol);
        header.Style.Font.Bold = true;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Row(headerRow).Height = 22;

        var r = headerRow + 1;
        decimal grandAmount = 0;
        var totalRows = new List<int>();
        var spacerRows = new List<int>();

        foreach (var c in summary.ByCompany)
        {
            // ชื่อบริษัท
            var companyRow = ws.Range(r++, 1, r - 1, lastCol).Merge();
            companyRow.Value = c.Company;
            companyRow.Style.Font.Bold = true;

            foreach (var b in c.Branches)
            {
                // ชื่อสาขา
                var branchRow = ws.Range(r++, 1, r - 1, lastCol).Merge();
                branchRow.Value = $"สาขา: {b.Branch}";
                branchRow.Style.Font.Bold = true;

                foreach (var x in b.Benefits)
                {
                    ws.Cell(r, 1).Value = "    " + x.Benefit;
                    ws.Cell(r, 2).Value = x.Amount;
                    r++;
                }

                WriteTotal(r++, $"รวมสาขา {b.Branch}", b.Amount);
            }

            WriteTotal(r++, $"รวม {c.Company}", c.Amount);
            grandAmount += c.Amount;

            // เว้น 1 บรรทัดแบ่งบริษัท
            spacerRows.Add(r++);
        }

        WriteTotal(r, "รวมทั้งหมด", grandAmount);

        // ---------- Format ----------
        var table = ws.Range(headerRow, 1, r, lastCol);
        table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        table.Style.Border.OutsideBorderColor = XLColor.Black;
        table.Style.Border.InsideBorderColor = XLColor.Black;

        ExcelStyles.Money(ws.Range(headerRow + 1, 2, r, 2));

        foreach (var row in totalRows)
        {
            var total = ws.Range(row, 1, row, lastCol);
            total.Style.Font.Bold = true;
            total.Style.Fill.BackgroundColor = ExcelStyles.Total;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        }

        foreach (var row in spacerRows)
        {
            var spacer = ws.Range(row, 1, row, lastCol);
            spacer.Style.Border.InsideBorder = XLBorderStyleValues.None;
            spacer.Style.Border.LeftBorder = XLBorderStyleValues.None;
            spacer.Style.Border.RightBorder = XLBorderStyleValues.None;
        }

        ws.Column(1).Width = 45;
        ws.Column(2).Width = 18;
        for (var row = headerRow + 1; row <= r; row++)
            if (ws.Row(row).Height < 20) ws.Row(row).Height = 20;

        ExcelStyles.FreezeHeader(ws, headerRow);

        void WriteTotal(int row, string label, decimal amount)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 2).Value = amount;
            totalRows.Add(row);
        }
    }

    // ================= Sheet 2 =================
    // รูปแบบตามตัวอย่างของ HR: พื้นขาว เส้นตารางดำบาง ไม่มีสี
    // คอลัมน์ผู้เบิก/บริษัท Merge ตามกลุ่ม เพื่อให้อ่านรายการของแต่ละคนได้ง่าย
    private static void BuildEmployeeSheet(
        IXLWorksheet ws,
        List<BenefitClaimExportRowDto> rows,
        BenefitClaimSummaryFilterDto filter)
    {
        const int lastCol = 8;
        const int colCompany = 1, colCode = 2, colName = 3, colDocNum = 4,
                  colDate = 5, colBenefit = 6, colDetail = 7, colAmount = 8;

        ExcelStyles.Init(ws);

        // ---------- Title ----------
        var title = ws.Range(1, 1, 1, lastCol).Merge();
        title.Value = "ข้อมูลค่าใช้จ่ายสวัสดิการ (รายบุคคล)";
        title.Style.Font.Bold = true;
        title.Style.Font.FontSize = 14;
        title.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Row(1).Height = 26;

        var sub = ws.Range(2, 1, 2, lastCol).Merge();
        sub.Value = FilterText(filter);
        sub.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // ---------- Header ----------
        const int headerRow = 4;
        string[] headers =
        {
            "บริษัท", "รหัสพนักงาน", "ชื่อพนักงาน", "เลขที่เอกสาร",
            "วันที่", "สวัสดิการ", "รายละเอียด", "จำนวนเงิน"
        };
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(headerRow, i + 1).Value = headers[i];

        var header = ws.Range(headerRow, 1, headerRow, lastCol);
        header.Style.Font.Bold = true;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Row(headerRow).Height = 22;

        // ---------- Body ----------
        var r = headerRow + 1;
        decimal grandTotal = 0;
        var totalRows = new List<int>();
        var spacerRows = new List<int>();

        foreach (var company in rows
            .GroupBy(x => string.IsNullOrWhiteSpace(x.Company) ? "ไม่ระบุบริษัท" : x.Company)
            .OrderBy(g => g.Key))
        {
            var companyFirst = r;

            foreach (var employee in company
                .GroupBy(x => new { x.EmployeeCode, x.EmployeeName })
                .OrderBy(g => g.Key.EmployeeCode)
                .ThenBy(g => g.Key.EmployeeName))
            {
                var employeeFirst = r;

                foreach (var item in employee.OrderBy(x => x.DocDate).ThenBy(x => x.DocNum))
                {
                    ws.Cell(r, colDetail).Value = string.IsNullOrWhiteSpace(item.Detail) ? "-" : item.Detail;
                    ws.Cell(r, colAmount).Value = item.Amount;
                    ws.Cell(r, colBenefit).Value = item.BenefitName;
                    ws.Cell(r, colDocNum).Value = item.DocNum;
                    ws.Cell(r, colDate).Value = item.DocDate;
                    r++;
                }

                ws.Cell(employeeFirst, colCode).Value = employee.Key.EmployeeCode;
                ws.Cell(employeeFirst, colName).Value = employee.Key.EmployeeName;
                MergeGroup(employeeFirst, r - 1, colCode);
                MergeGroup(employeeFirst, r - 1, colName);
            }

            ws.Cell(companyFirst, colCompany).Value = company.Key;
            MergeGroup(companyFirst, r - 1, colCompany);

            // รวมบริษัท
            var companyTotal = company.Sum(x => x.Amount);
            WriteTotal(r++, $"รวม {company.Key}", companyTotal);
            grandTotal += companyTotal;

            // เว้น 1 บรรทัดแบ่งบริษัท
            spacerRows.Add(r++);
        }

        WriteTotal(r, "รวมทั้งหมด", grandTotal);

        // ---------- Format ----------
        var table = ws.Range(headerRow, 1, r, lastCol);
        table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        table.Style.Border.OutsideBorderColor = XLColor.Black;
        table.Style.Border.InsideBorderColor = XLColor.Black;

        var data = ws.Range(headerRow + 1, 1, r, lastCol);
        ws.Range(headerRow + 1, colDetail, r, colDetail).Style.Alignment.WrapText = true;
        ExcelStyles.Money(ws.Range(headerRow + 1, colAmount, r, colAmount));
        ExcelStyles.Date(ws.Range(headerRow + 1, colDate, r, colDate));
        ws.Range(headerRow + 1, colCompany, r, colCompany).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Range(headerRow + 1, colCode, r, colCode).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Range(headerRow + 1, colDocNum, r, colDocNum).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Range(headerRow + 1, colBenefit, r, colBenefit).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        data.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        // แถวรวม: ชิดขวา ตัวหนา พื้นเทาอ่อน (ทำหลังจัดคอลัมน์ ไม่งั้นโดนจัดกึ่งกลางทับ)
        foreach (var row in totalRows)
        {
            var total = ws.Range(row, 1, row, lastCol);
            total.Style.Font.Bold = true;
            total.Style.Fill.BackgroundColor = ExcelStyles.Total;
            ws.Cell(row, colCompany).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        }

        // บรรทัดเว้นระหว่างบริษัท: ไม่มีเส้นตาราง
        foreach (var row in spacerRows)
        {
            var spacer = ws.Range(row, 1, row, lastCol);
            spacer.Style.Border.InsideBorder = XLBorderStyleValues.None;
            spacer.Style.Border.LeftBorder = XLBorderStyleValues.None;
            spacer.Style.Border.RightBorder = XLBorderStyleValues.None;
        }

        ws.Column(colCompany).Width = 18;
        ws.Column(colCode).Width = 13;
        ws.Column(colName).Width = 26;
        ws.Column(colBenefit).Width = 22;
        ws.Column(colDocNum).Width = 18;
        ws.Column(colDate).Width = 12;
        ws.Column(colDetail).Width = 55;
        ws.Column(colAmount).Width = 14;

        // ความสูงตามรายละเอียดที่ตัดบรรทัด (คอลัมน์ Merge ไม่นำมาคิด)
        for (var row = headerRow + 1; row <= r; row++)
        {
            ws.Row(row).AdjustToContents(colDetail, colDetail);
            if (ws.Row(row).Height < 20) ws.Row(row).Height = 20;
        }

        ws.SheetView.FreezeRows(headerRow);

        // ---------- Print ----------
        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
        ws.PageSetup.FitToPages(1, 0);
        ws.PageSetup.SetRowsToRepeatAtTop(headerRow, headerRow);
        ws.PageSetup.PrintAreas.Add(1, 1, r, lastCol);

        // ---------- Local helpers ----------
        void MergeGroup(int first, int last, int column)
        {
            if (last <= first) return;
            ws.Range(first, column, last, column).Merge();
        }

        void WriteTotal(int row, string label, decimal amount)
        {
            // Merge ตั้งแต่บริษัทถึงรายละเอียด แล้วแสดงยอดที่คอลัมน์จำนวนเงิน
            ws.Range(row, colCompany, row, colAmount - 1).Merge().Value = label;
            ws.Cell(row, colAmount).Value = amount;
            totalRows.Add(row);
        }
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