import type { Component } from "vue";
import PettyCashApprovalDetail from "../components/approval/PettyCashApprovalDetail.vue";
import BenefitClaimApprovalDetail from "../components/approval/BenefitClaimApprovalDetail.vue";

export interface ApprovalDocumentConfig {
  label: string;
  detailComponent: Component;
}

export const approvalDocumentRegistry: Record<string, ApprovalDocumentConfig> =
  {
    PettyCash: {
      label: "เงินสดย่อย",
      detailComponent: PettyCashApprovalDetail,
    },
    BenefitClaim: {
      label: "เบิกสวัสดิการ",
      detailComponent: BenefitClaimApprovalDetail,
    },
  };
