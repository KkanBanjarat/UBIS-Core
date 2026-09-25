import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type { BranchAdminGroup } from "../types/PodAdminBranch";

export const usePodAdminBranchStore = defineStore("podAdminBranch", {
  state: () => ({
    groups: [] as BranchAdminGroup[],
    isLoading: false,
    errorMessage: "",
  }),

  getters: {
    // สาขาที่ยังไม่มีผู้ดูแลหลัก — ต้องเตือน IT เพราะ Approve Step 2 จะพัง
    branchesWithoutPrimary: (state) =>
      state.groups.filter((g) => !g.admins.some((a) => a.isPrimary)),

    totalAdmins: (state) =>
      state.groups.reduce((sum, g) => sum + g.admins.length, 0),
  },

  actions: {
    async fetchAll() {
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await hrApi.get("/PodAdminBranches/grouped-by-branch");
        this.groups = res.data;
      } catch (err) {
        this.errorMessage = "โหลดข้อมูลผู้ดูแลสาขาไม่สำเร็จ";
        console.error("Failed to load branch admins:", err);
      } finally {
        this.isLoading = false;
      }
    },

    async create(payload: {
      employeeId: string;
      branchId: string;
      isPrimary: boolean;
    }) {
      await hrApi.post("/PodAdminBranches", payload);
    },

    async setPrimary(id: string) {
      await hrApi.post(`/PodAdminBranches/${id}/set-primary`);
    },

    async remove(id: string) {
      await hrApi.delete(`/PodAdminBranches/${id}`);
    },
  },
});
