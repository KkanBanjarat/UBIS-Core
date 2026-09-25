import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type { Benefit, BenefitPlan, BenefitPlanItem } from "../types/Benefit";

export const useBenefitStore = defineStore("benefit", {
  state: () => ({
    benefits: [] as Benefit[],
    benefitPlans: [] as BenefitPlan[],
    benefitPlanItems: [] as BenefitPlanItem[],
    isLoading: false,
  }),

  getters: {
    // Dropdown สวัสดิการสำหรับเลือกใส่ในกลุ่ม
    benefitOptions: (state) =>
      state.benefits.map((b) => ({
        id: b.id,
        label: `${b.nameTh} (${b.nameEn})`,
      })),

    // นับจำนวนสวัสดิการในแต่ละกลุ่ม (คำนวณครั้งเดียวแล้วใช้ซ้ำ แทนการ filter ทุกครั้งที่ Render)
    itemCountByPlanId: (state) => {
      const map = new Map<string, number>();
      for (const item of state.benefitPlanItems) {
        map.set(item.benefitPlanId, (map.get(item.benefitPlanId) ?? 0) + 1);
      }
      return map;
    },
  },

  actions: {
    async fetchAll() {
      this.isLoading = true;
      try {
        const [benefitsRes, plansRes, itemsRes] = await Promise.all([
          hrApi.get("/benefits"),
          hrApi.get("/benefitplans"),
          hrApi.get("/benefitplanitems"),
        ]);
        this.benefits = benefitsRes.data;
        this.benefitPlans = plansRes.data;
        this.benefitPlanItems = itemsRes.data;
      } catch (err) {
        console.error("Failed to load benefit data:", err);
        throw err;
      } finally {
        this.isLoading = false;
      }
    },

    // ===== Benefit (Master List) =====
    async createBenefit(payload: any) {
      await hrApi.post("/benefits", payload);
    },
    async updateBenefit(id: string, payload: any) {
      await hrApi.put(`/benefits/${id}`, payload);
    },
    async removeBenefit(id: string) {
      await hrApi.delete(`/benefits/${id}`);
    },

    // ===== Benefit Plan (กลุ่มสวัสดิการ) =====
    async createPlan(payload: any): Promise<BenefitPlan> {
      const res = await hrApi.post("/benefitplans", payload);
      return res.data;
    },
    async updatePlan(id: string, payload: any) {
      await hrApi.put(`/benefitplans/${id}`, payload);
    },
    async removePlan(id: string) {
      await hrApi.delete(`/benefitplans/${id}`);
    },

    // ===== Benefit Plan Item (สวัสดิการในกลุ่ม) =====
    async createItem(payload: any) {
      await hrApi.post("/benefitplanitems", payload);
    },
    async updateItem(id: string, payload: any) {
      await hrApi.put(`/benefitplanitems/${id}`, payload);
    },
    async removeItem(id: string) {
      await hrApi.delete(`/benefitplanitems/${id}`);
    },
  },
});
