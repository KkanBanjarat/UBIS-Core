import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type { Position, PositionFilter } from "../types/Position";

export const usePositionStore = defineStore("position", {
  state: () => ({
    positions: [] as Position[],
    totalCount: 0,
    isLoading: false,
    errorMessage: "",
  }),

  actions: {
    async fetchList(filter: PositionFilter) {
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await hrApi.post("/positions/position-list", {
          search: filter.search || null,
          page: filter.page,
          pageSize: filter.pageSize,
        });
        this.positions = res.data.items;
        this.totalCount = res.data.totalCount;
      } catch (err) {
        this.errorMessage = "โหลดข้อมูลตำแหน่งงานไม่สำเร็จ";
        console.error("Failed to load positions:", err);
      } finally {
        this.isLoading = false;
      }
    },

    async create(payload: any) {
      await hrApi.post("/positions", payload);
    },

    async update(id: string, payload: any) {
      await hrApi.put(`/positions/${id}`, payload);
    },

    async remove(id: string) {
      await hrApi.delete(`/positions/${id}`);
    },
  },
});
