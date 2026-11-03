namespace UBIS.HR.Application.Common;

// จำนวนตำแหน่งทศนิยมมาตรฐานของระบบ (ต้องตรงกับ DECIMAL_PLACES ฝั่งเว็บ และ numeric(18,3) ใน DB)
public static class DecimalPrecision
{
    public const int Places = 3;
    public const string Format = "N3";

    // ปัดยอดเงินทุกจุดก่อนรวม เพื่อให้ยอดรวมเท่ากับผลรวมยอดแจกแจงที่แสดงบนเอกสาร
    public static decimal Round(decimal value) =>
        Math.Round(value, Places, MidpointRounding.AwayFromZero);
}
