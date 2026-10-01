import type { Component } from "vue";
import PettyCashApprovalDetail from "../components/approval/PettyCashApprovalDetail.vue";
export interface ApprovalDocumentConfig {
  label: string;
  detailComponent: Component;
}

// ⭐ เอกสารประเภทใหม่ในอนาคต แค่เพิ่ม Entry ตรงนี้ ไม่ต้องแก้ ApprovalsView.vue / ApprovalDetailModal.vue เลย
export const approvalDocumentRegistry: Record<string, ApprovalDocumentConfig> =
  {
    PettyCash: {
      label: "เบิกสวัสดิการ / เงินสดย่อย",
      detailComponent: PettyCashApprovalDetail,
    },
    // Leave: { label: 'ใบลา', detailComponent: LeaveApprovalDetail },  ← ตัวอย่างอนาคต
  };
