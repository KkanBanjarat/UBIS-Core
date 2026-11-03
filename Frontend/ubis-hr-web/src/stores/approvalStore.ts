import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type {
  Approval,
  ApprovalFilter,
  ApprovalTrail,
} from "../types/Approval";

let pendingReqId = 0;

// ตัดค่าว่างทิ้ง เพื่อไม่ส่ง ?search=&docType= ไปที่ API
function cleanParams(filter: ApprovalFilter) {
  const params: Record<string, string> = {};
  (Object.keys(filter) as (keyof ApprovalFilter)[]).forEach((k) => {
    const v = filter[k]?.trim();
    if (v) params[k] = v;
  });
  return params;
}

export const useApprovalStore = defineStore("approval", {
  state: () => ({
    items: [] as Approval[],
    filter: {} as ApprovalFilter, // filter ล่าสุดที่ใช้โหลด
    isLoading: true, // กันเห็น "ไม่มีรายการ" วาบก่อนโหลด
    errorMessage: "",
  }),

  actions: {
    /**
     * ส่ง filter มา = ตั้งค่าใหม่ แล้วโหลด
     * ไม่ส่ง = โหลดซ้ำด้วย filter เดิม (ใช้หลังอนุมัติ/ไม่อนุมัติ)
     */
    async fetchMyPending(filter?: ApprovalFilter) {
      if (filter) this.filter = { ...filter };

      const reqId = ++pendingReqId;
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await hrApi.get("/Approval/my-pending", {
          params: cleanParams(this.filter),
        });
        if (reqId !== pendingReqId) return;
        this.items = res.data;
      } catch (err) {
        if (reqId !== pendingReqId) return;
        this.errorMessage = "โหลดรายการรออนุมัติไม่สำเร็จ กรุณาลองใหม่";
        console.error("Failed to load pending approvals:", err);
      } finally {
        if (reqId === pendingReqId) this.isLoading = false;
      }
    },

    async approve(transApproveId: number) {
      await hrApi.post(`/Approval/${transApproveId}/approve`);
      await this.fetchMyPending();
    },

    async deny(transApproveId: number, reason: string, isDisapprove: boolean) {
      await hrApi.post(`/Approval/${transApproveId}/deny`, {
        reason,
        isDisapprove,
      });
      await this.fetchMyPending();
    },

    async getTrail(docType: string, docNumber: string): Promise<ApprovalTrail> {
      const res = await hrApi.get("/Approval/trail", {
        params: { docType, docNumber },
      });

      return res.data;
    },
  },
});
