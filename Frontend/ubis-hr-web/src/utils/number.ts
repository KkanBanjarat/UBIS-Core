// คำนวณ: 3 ตำแหน่ง (ต้องตรงกับ DecimalPrecision.Places ฝั่ง API)
export const DECIMAL_PLACES = 3;

// แสดงผลบนหน้าจอ: ต้องเท่ากับ DECIMAL_PLACES ไม่เช่นนั้นยอดรวมกับยอดแจกแจงที่แสดงจะไม่ตรงกัน
export const DISPLAY_DECIMALS = DECIMAL_PLACES;

export function roundNum(n: number, digits = DECIMAL_PLACES) {
  const f = 10 ** digits;
  return Math.round((n + Number.EPSILON) * f) / f;
}

export function formatNumber(
  n: number | string | null | undefined,
  digits = DISPLAY_DECIMALS,
) {
  const v = Number(n ?? 0);
  if (!Number.isFinite(v)) return "-";
  return v.toLocaleString("th-TH", {
    minimumFractionDigits: digits,
    maximumFractionDigits: digits,
  });
}
