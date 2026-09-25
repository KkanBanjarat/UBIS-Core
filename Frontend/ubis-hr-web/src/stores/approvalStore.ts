import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type { Approval, ApprovalTrail } from "../types/Approval";

export const useApprovalStore = defineStore("approval", {
  state: () => ({
    items: [] as Approval[],
    isLoading: false,
  }),

  actions: {
    async fetchMyPending() {
      this.isLoading = true;
      try {
        const res = await hrApi.get("/Approval/my-pending");
        this.items = res.data;
      } finally {
        this.isLoading = false;
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
