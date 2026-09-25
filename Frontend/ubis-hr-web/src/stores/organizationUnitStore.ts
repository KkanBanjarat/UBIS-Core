import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type {
  OrganizationUnit,
  OrganizationLevelType,
} from "../types/OrganizationUnit";

export interface OrganizationUnitFilter {
  search: string;
  type: string | null;
  page: number;
  pageSize: number;
}

export const useOrganizationUnitStore = defineStore("organizationUnit", {
  state: () => ({
    // List (Server-side Paging)
    organizationUnits: [] as OrganizationUnit[],
    totalCount: 0,
    isLoading: false,
    errorMessage: "",

    // Master Data — เปลี่ยนแปลงน้อยมาก Cache ไว้ทั้ง Session
    levelTypes: [] as OrganizationLevelType[],
    isLevelTypesLoaded: false,
  }),

  getters: {
    /**
     * ตัวเลือกประเภทหน่วยงาน (ตัด "Company" ออก เพราะไม่ใช่หน่วยงานย่อย)
     * ใช้ร่วมกันทั้ง Filter และ Form
     */
    typeOptions: (state) =>
      state.levelTypes
        .filter((t) => t.nameEn.toLowerCase() !== "company")
        .sort((a, b) => a.sequence - b.sequence)
        .map((t) => ({ id: t.nameEn, label: `${t.nameTh} (${t.nameEn})` })),

    // แปลง Type Code → ชื่อไทย (สร้าง Map ครั้งเดียว แทนการ find() ทุกแถวที่ Render)
    typeLabelMap: (state) => {
      const map = new Map<string, string>();
      for (const t of state.levelTypes) {
        map.set(t.nameEn, t.nameTh);
      }
      return map;
    },
  },

  actions: {
    async fetchList(filter: OrganizationUnitFilter) {
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await hrApi.post(
          "/organizationunits/organization-unit-list",
          {
            search: filter.search || null,
            type: filter.type || null,
            page: filter.page,
            pageSize: filter.pageSize,
          },
        );
        this.organizationUnits = res.data.items;
        this.totalCount = res.data.totalCount;
      } catch (err) {
        this.errorMessage = "โหลดข้อมูลหน่วยงานไม่สำเร็จ";
        console.error("Failed to load organization units:", err);
      } finally {
        this.isLoading = false;
      }
    },

    async fetchLevelTypes(force = false) {
      if (this.isLevelTypesLoaded && !force) return;

      try {
        const res = await hrApi.get("/organizationleveltypes");
        this.levelTypes = res.data;
        this.isLevelTypesLoaded = true;
      } catch (err) {
        console.error("Failed to load level types:", err);
      }
    },

    async create(payload: any) {
      await hrApi.post("/organizationunits", payload);
    },

    async update(id: string, payload: any) {
      await hrApi.put(`/organizationunits/${id}`, payload);
    },

    async remove(id: string) {
      await hrApi.delete(`/organizationunits/${id}`);
    },
  },
});
