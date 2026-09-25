import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type {
  PositionLevel,
  PositionLevelFilter,
} from "../types/PositionLevel";

export const usePositionLevelStore = defineStore("positionLevel", {
  state: () => ({
    positionLevels: [] as PositionLevel[],
    totalCount: 0,
    isLoading: false,
    errorMessage: "",
  }),

  actions: {
    async fetchList(filter: PositionLevelFilter) {
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await hrApi.post("/positionlevels/position-level-list", {
          search: filter.search || null,
          page: filter.page,
          pageSize: filter.pageSize,
        });
        this.positionLevels = res.data.items;
        this.totalCount = res.data.totalCount;
      } catch (err) {
        this.errorMessage = "โหลดข้อมูลระดับตำแหน่งไม่สำเร็จ";
        console.error("Failed to load position levels:", err);
      } finally {
        this.isLoading = false;
      }
    },

    async create(payload: any) {
      await hrApi.post("/positionlevels", payload);
    },

    async update(id: string, payload: any) {
      await hrApi.put(`/positionlevels/${id}`, payload);
    },

    async remove(id: string) {
      await hrApi.delete(`/positionlevels/${id}`);
    },
  },
});
