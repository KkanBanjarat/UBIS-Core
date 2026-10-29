import type { BreakdownRow } from "../types/BenefitClaim";

export const tripTypeOptions = [
  { id: "Domestic", label: "ในประเทศ" },
  { id: "International", label: "ต่างประเทศทั่วไป" },
  { id: "Special", label: "ต่างประเทศอัตราพิเศษ" },
];

const tripTypeLabels: Record<string, string> = Object.fromEntries(
  tripTypeOptions.map((o) => [o.id, o.label]),
);

export function tripTypeLabel(t: string | null | undefined) {
  return t ? (tripTypeLabels[t] ?? t) : "";
}

export function rangeText(r: BreakdownRow) {
  if (r.from == null) return "-";
  return r.to == null ? `${r.from} ขึ้นไป` : `${r.from} - ${r.to}`;
}
