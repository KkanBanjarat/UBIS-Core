import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type { RouteApprove } from "../types/RouteApprove";

export const useRouteApproveStore = defineStore("routeApprove", {
  state: () => ({
    routes: [] as RouteApprove[],
    isLoading: false,
    errorMessage: "",
  }),

  getters: {
    // จัดกลุ่มตามประเภทเอกสาร เรียงตามลำดับขั้นตอน
    routesByDocType: (state) => {
      const map = new Map<string, RouteApprove[]>();
      for (const r of state.routes) {
        if (!map.has(r.docType)) map.set(r.docType, []);
        map.get(r.docType)!.push(r);
      }
      for (const list of map.values()) {
        list.sort((a, b) => a.stepNo - b.stepNo);
      }
      return map;
    },

    docTypes: (state) =>
      Array.from(new Set(state.routes.map((r) => r.docType))).sort(),
  },

  actions: {
    async fetchAll() {
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await hrApi.get("/RouteApprove");
        this.routes = res.data;
      } catch (err) {
        this.errorMessage = "โหลดข้อมูลสายอนุมัติไม่สำเร็จ";
        console.error("Failed to load approve routes:", err);
      } finally {
        this.isLoading = false;
      }
    },

    async create(payload: any) {
      await hrApi.post("/RouteApprove", payload);
    },

    async update(id: string, payload: any) {
      await hrApi.put(`/RouteApprove/${id}`, payload);
    },

    async remove(id: string) {
      await hrApi.delete(`/RouteApprove/${id}`);
    },
  },
});
