import type { Directive } from "vue";
import { DECIMAL_PLACES } from "../utils/number";

// จำกัดจำนวนทศนิยมใน <input> ตอนพิมพ์/วาง (ค่าเริ่มต้น = DECIMAL_PLACES) เช่น v-decimal หรือ v-decimal="2"
// ใช้ listener แบบ capture เพื่อให้ตัดค่าก่อน v-model อ่านค่าไปใช้
type DecimalInput = HTMLInputElement & { _decimalHandler?: (e: Event) => void };

function trim(el: HTMLInputElement, digits: number) {
  const v = el.value;
  const dot = v.indexOf(".");
  if (dot >= 0 && v.length - dot - 1 > digits) {
    el.value = digits > 0 ? v.slice(0, dot + 1 + digits) : v.slice(0, dot);
  }
}

export const vDecimal: Directive<DecimalInput, number | undefined> = {
  mounted(el, binding) {
    el._decimalHandler = () => trim(el, binding.value ?? DECIMAL_PLACES);
    el.addEventListener("input", el._decimalHandler, { capture: true });
  },
  updated(el, binding) {
    if (binding.value !== binding.oldValue) {
      el.removeEventListener("input", el._decimalHandler!, { capture: true });
      el._decimalHandler = () => trim(el, binding.value ?? DECIMAL_PLACES);
      el.addEventListener("input", el._decimalHandler, { capture: true });
    }
  },
  beforeUnmount(el) {
    if (el._decimalHandler) {
      el.removeEventListener("input", el._decimalHandler, { capture: true });
    }
  },
};

declare module "vue" {
  interface GlobalDirectives {
    vDecimal: typeof vDecimal;
  }
}
