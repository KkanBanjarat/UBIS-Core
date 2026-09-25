import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type { Company } from "../types/Company";
import type { Branch } from "../types/Branch";

export const useCompanyStore = defineStore("company", {
  state: () => ({
    companies: [] as Company[],
    branches: [] as Branch[],
    isLoading: false,
    errorMessage: "",
    isLoaded: false,
  }),

  getters: {
    // จัดกลุ่มสาขาตามบริษัทไว้ล่วงหน้า แทนการ filter ทั้ง Array ทุกครั้งที่ Render แต่ละการ์ด
    branchesByCompanyId: (state) => {
      const map = new Map<string, Branch[]>();
      for (const b of state.branches) {
        if (!map.has(b.companyId)) map.set(b.companyId, []);
        map.get(b.companyId)!.push(b);
      }
      return map;
    },

    companyOptions: (state) =>
      state.companies.map((c) => ({ id: c.id, label: c.nameTh })),
  },

  actions: {
    /**
     * Master Data บริษัท/สาขา เปลี่ยนแปลงน้อยมาก จึง Cache ไว้ใช้ทั้ง Session
     */
    async fetchAll(force = false) {
      if (this.isLoaded && !force) return;

      this.isLoading = true;
      this.errorMessage = "";
      try {
        const [companyRes, branchRes] = await Promise.all([
          hrApi.get("/companies"),
          hrApi.get("/branchs"),
        ]);

        this.companies = companyRes.data.map((x: any) => ({
          id: x.id,
          code: x.code,
          nameTh: x.nameTh,
          nameEn: x.nameEn,
          groupName: x.groupName,
          isActive: x.isActive,
          updatedAt: x.updatedAt,
        }));

        this.branches = branchRes.data.map((x: any) => ({
          id: x.id,
          companyId: x.companyId,
          code: x.code,
          nameTh: x.nameTh,
          nameEn: x.nameEn,
          isActive: x.isActive,
        }));

        this.isLoaded = true;
      } catch (err) {
        this.errorMessage = "โหลดข้อมูลบริษัทไม่สำเร็จ";
        console.error("Failed to load company data:", err);
      } finally {
        this.isLoading = false;
      }
    },

    /**
     * สาขาตามบริษัท — ใช้ข้อมูลที่โหลดไว้แล้วถ้ามี ไม่ต้องยิง API ซ้ำ
     * (หน้า Employee เดิมยิง /branchs/by-company ทุกครั้งที่เปลี่ยนบริษัท)
     */
    async getBranchOptionsByCompany(companyId: string) {
      await this.fetchAll();
      const list = this.branchesByCompanyId.get(companyId) ?? [];
      return list.map((b) => ({
        id: b.id,
        label: `${b.nameEn} (${b.nameTh})`,
      }));
    },
  },
});
