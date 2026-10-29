using ClosedXML.Excel;

namespace UBIS.HR.Infrastructure.Excel;

/// <summary>
/// สี / ฟอนต์ / helper ตกแต่ง Excel กลางของทั้งระบบ
/// แก้ที่นี่ที่เดียว มีผลกับทุกรายงาน
/// </summary>
public static class ExcelStyles
{
    // ===== ฟอนต์ =====
    public const string FontName = "Tahoma";
    public const double FontSize = 10;
    public const double TitleSize = 14;

    // ===== สี =====
    public static readonly XLColor Primary = XLColor.FromHtml("#1F4E79"); // หัวตาราง
    public static readonly XLColor Band = XLColor.FromHtml("#DDEBF7");    // แถบบริษัท/สาขา
    public static readonly XLColor Total = XLColor.FromHtml("#F2F2F2");   // แถวรวม
    public static readonly XLColor Border = XLColor.FromHtml("#BFBFBF");
    public static readonly XLColor HeaderText = XLColor.White;

    // ===== Format =====
    public const string MoneyFormat = "#,##0.00";
    public const string IntFormat = "#,##0";
    public const string DateFormat = "dd/MM/yyyy";

    // ===== ระดับชีต =====

    /// <summary>ตั้งฟอนต์ทั้งชีต และซ่อน gridlines</summary>
    public static void Init(IXLWorksheet ws)
    {
        ws.Style.Font.FontName = FontName;
        ws.Style.Font.FontSize = FontSize;
        ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.ShowGridLines = false;
    }

    /// <summary>ปรับความกว้างคอลัมน์อัตโนมัติ โดยจำกัดช่วง min-max</summary>
    public static void AutoFit(IXLWorksheet ws, double minWidth = 8, double maxWidth = 50)
    {
        ws.Columns().AdjustToContents();
        foreach (var col in ws.ColumnsUsed())
        {
            if (col.Width < minWidth) col.Width = minWidth;
            else if (col.Width > maxWidth) col.Width = maxWidth;
        }
    }

    /// <summary>ตรึงแถวตั้งแต่บนสุดถึง row (หัวตาราง)</summary>
    public static void FreezeHeader(IXLWorksheet ws, int row)
        => ws.SheetView.FreezeRows(row);

    // ===== ระดับแถว =====

    /// <summary>หัวรายงาน (merge คอลัมน์ 1..lastCol)</summary>
    public static void Title(IXLWorksheet ws, int row, string text, int lastCol)
    {
        var r = ws.Range(row, 1, row, lastCol).Merge();
        r.Value = text;
        r.Style.Font.Bold = true;
        r.Style.Font.FontSize = TitleSize;
        r.Style.Font.FontColor = Primary;
        r.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        ws.Row(row).Height = 24;
    }

    /// <summary>หัวตาราง</summary>
    public static void HeaderRow(IXLWorksheet ws, int row, int lastCol)
    {
        var r = ws.Range(row, 1, row, lastCol);
        r.Style.Fill.BackgroundColor = Primary;
        r.Style.Font.FontColor = HeaderText;
        r.Style.Font.Bold = true;
        r.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        r.Style.Alignment.WrapText = true;
        SetBorder(r);
        ws.Row(row).Height = 22;
    }

    /// <summary>แถบกลุ่ม เช่น ชื่อบริษัท / สาขา (merge ทั้งแถว)</summary>
    public static void BandRow(IXLWorksheet ws, int row, string text, int lastCol)
    {
        var r = ws.Range(row, 1, row, lastCol).Merge();
        r.Value = text;
        r.Style.Fill.BackgroundColor = Band;
        r.Style.Font.Bold = true;
        r.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        SetBorder(r);
    }

    /// <summary>แถวรวม (ตัวหนา พื้นเทา เส้นบนหนา)</summary>
    public static void TotalRow(IXLWorksheet ws, int row, int firstCol, int lastCol, XLAlignmentHorizontalValues alignment = XLAlignmentHorizontalValues.Left)
    {
        var r = ws.Range(row, firstCol, row, lastCol);
        r.Style.Fill.BackgroundColor = Total;
        r.Style.Font.Bold = true;

        // จัดตำแหน่งตามค่าที่ส่งมา (Default = Left)
        r.Style.Alignment.Horizontal = alignment;
        r.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        SetBorder(r);
        r.Style.Border.TopBorder = XLBorderStyleValues.Medium;
    }

    /// <summary>ขอบบาง + ฟอนต์ปกติ สำหรับช่วงข้อมูล</summary>
    public static void Body(IXLRange range)
    {
        range.Style.Font.Bold = false;
        SetBorder(range);
    }

    // ===== ระดับเซลล์ / ช่วง =====

    public static void Money(IXLRange range)
    {
        range.Style.NumberFormat.Format = MoneyFormat;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
    }

    public static void Int(IXLRange range)
    {
        range.Style.NumberFormat.Format = IntFormat;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
    }

    public static void Date(IXLRange range)
    {
        range.Style.DateFormat.Format = DateFormat;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    // ===== ภายใน =====

    private static void SetBorder(IXLRange r)
    {
        r.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        r.Style.Border.OutsideBorderColor = Border;
        r.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        r.Style.Border.InsideBorderColor = Border;
    }
}