const DIGITS = [
  "ศูนย์",
  "หนึ่ง",
  "สอง",
  "สาม",
  "สี่",
  "ห้า",
  "หก",
  "เจ็ด",
  "แปด",
  "เก้า",
];
const UNITS = ["", "สิบ", "ร้อย", "พัน", "หมื่น", "แสน"];

// อ่านเลขไม่เกินหลักแสน (0 - 999,999)
function readBelowMillion(n: number, afterMillion = false): string {
  const s = String(n);
  const len = s.length;
  let out = "";
  for (let i = 0; i < len; i++) {
    const d = Number(s[i]);
    const pos = len - i - 1;
    if (d === 0) continue;
    if (pos === 0 && d === 1 && (len > 1 || afterMillion)) out += "เอ็ด";
    else if (pos === 1 && d === 2) out += "ยี่สิบ";
    else if (pos === 1 && d === 1) out += "สิบ";
    else out += DIGITS[d] + UNITS[pos];
  }
  return out;
}

function readInt(n: number): string {
  if (n === 0) return DIGITS[0];
  const million = Math.floor(n / 1_000_000);
  const rest = n % 1_000_000;
  let out = "";
  if (million > 0) out += readInt(million) + "ล้าน";
  if (rest > 0) out += readBelowMillion(rest, million > 0);
  return out;
}

/** 842.45 -> แปดร้อยสี่สิบสองบาทสี่สิบห้าสตางค์ */
export function thaiBahtText(amount: number | null | undefined): string {
  if (amount == null || Number.isNaN(amount)) return "";
  const total = Math.round(Math.abs(amount) * 100);
  const baht = Math.floor(total / 100);
  const satang = total % 100;
  const neg = amount < 0 ? "ลบ" : "";

  if (baht === 0 && satang > 0) return `${neg}${readInt(satang)}สตางค์`;
  const bahtText = `${readInt(baht)}บาท`;
  return `${neg}${bahtText}${satang === 0 ? "ถ้วน" : readInt(satang) + "สตางค์"}`;
}
