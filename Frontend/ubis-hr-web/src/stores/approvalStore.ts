import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type { Approval, ApprovalTrail } from "../types/Approval";

let pendingReqId = 0;

export const useApprovalStore = defineStore("approval", {
  state: () => ({
    items: [] as Approval[],
    isLoading: true, // เดิม false — กันเห็น "ไม่มีรายการ" วาบก่อนโหลด
    errorMessage: "", // เพิ่มใหม่
  }),

  actions: {
    async fetchMyPending() {
      const reqId = ++pendingReqId;
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await hrApi.get("/Approval/my-pending");
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
