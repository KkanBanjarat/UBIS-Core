import type { Component } from "vue";
import PrettyCashApprovalDetail from "../components/approval/PrettyCashApprovalDetail.vue";
export interface ApprovalDocumentConfig {
  label: string;
  detailComponent: Component;
}

// ⭐ เอกสารประเภทใหม่ในอนาคต แค่เพิ่ม Entry ตรงนี้ ไม่ต้องแก้ ApprovalsView.vue / ApprovalDetailModal.vue เลย
export const approvalDocumentRegistry: Record<string, ApprovalDocumentConfig> =
  {
    PrettyCash: {
      label: "เบิกสวัสดิการ / เงินสดย่อย",
      detailComponent: PrettyCashApprovalDetail,
    },
    // Leave: { label: 'ใบลา', detailComponent: LeaveApprovalDetail },  ← ตัวอย่างอนาคต
  };
