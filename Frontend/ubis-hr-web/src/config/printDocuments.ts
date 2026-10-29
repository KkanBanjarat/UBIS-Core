import { useBenefitClaimStore } from "../stores/benefitClaimStore";
import { usePettyCashStore } from "../stores/pettyCashStore";

export const COMPANY_NAME = "ชื่อบริษัท";

export interface PrintDocumentConfig {
  title: string;
  showBenefit: boolean;
  load: (id: string) => Promise<any>;
}

// เอกสารประเภทใหม่ แค่เพิ่ม entry ตรงนี้
export const printDocumentRegistry: Record<string, PrintDocumentConfig> = {
  BenefitClaim: {
    title: "ใบเบิกสวัสดิการ",
    showBenefit: true,
    load: (id) => useBenefitClaimStore().getById(id),
  },
  PettyCash: {
    title: "ใบเบิกเงินสดย่อย",
    showBenefit: false,
    load: (id) => usePettyCashStore().getById(id),
  },
};
