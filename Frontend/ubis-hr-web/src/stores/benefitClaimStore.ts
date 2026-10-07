import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type { BenefitClaim, BenefitClaimFilter } from "../types/BenefitClaim";

interface OptionItem {
  id: string;
  label: string;
}

interface BenefitLimitInfo {
  limitAmount: number;
  description: string | null;
}

interface EmployeeOption {
  id: string;
  empId: string;
  fullNameTh: string;
  positionNameTh?: string | null;
}

let listReqId = 0;

export const useBenefitClaimStore = defineStore("benefitClaim", {
  state: () => ({
    items: [] as BenefitClaim[],
    totalCount: 0,
    isLoading: true,
    errorMessage: "",
    benefitOptions: [] as OptionItem[],
    allowedEmployees: [] as EmployeeOption[],
    myEmployeeId: "",
    myEmployeeName: "",
  }),
  actions: {
    async fetchList(filter: BenefitClaimFilter) {
      const reqId = ++listReqId;
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await hrApi.post(
          "/BenefitClaim/benefit-claim-list",
          filter,
        );
        if (reqId !== listReqId) return;
        this.items = res.data.items;
        this.totalCount = res.data.totalCount;
      } catch (err) {
        if (reqId !== listReqId) return;
        this.errorMessage = "โหลดรายการใบเบิกสวัสดิการไม่สำเร็จ กรุณาลองใหม่";
        console.error("Failed to load benefit claim list:", err);
      } finally {
        if (reqId === listReqId) this.isLoading = false;
      }
    },

    async fetchInitData() {
      const [benefitRes, allowedRes] = await Promise.all([
        hrApi.get("/benefits"),
        hrApi.get("/Employees/allowed-for-document"),
      ]);

      this.benefitOptions = benefitRes.data
        .filter((x: any) => x.isActive)
        .map((x: any) => ({ id: x.id, label: `${x.nameTh} (${x.nameEn})` }));

      this.allowedEmployees = allowedRes.data.map((x: any) => ({
        id: x.id,
        empId: x.empId,
        fullNameTh: x.fullNameTh,
        positionNameTh: x.positionNameTh,
      }));
    },

    setMyEmployee(id: string, name: string) {
      this.myEmployeeId = id;
      this.myEmployeeName = name;
    },

    async create(payload: any) {
      await hrApi.post("/BenefitClaim", payload);
    },

    async update(id: string, payload: any) {
      await hrApi.put(`/BenefitClaim/${id}`, payload);
    },

    async remove(id: string) {
      await hrApi.delete(`/BenefitClaim/${id}`);
    },

    async submit(id: string) {
      await hrApi.post(`/BenefitClaim/${id}/submit`);
    },

    async recall(id: string) {
      await hrApi.post(`/BenefitClaim/${id}/recall`);
    },

    async getBenefitLimitsFor(
      employeeId: string,
    ): Promise<Record<string, BenefitLimitInfo>> {
      const res = await hrApi.get(`/Employees/${employeeId}`);
      const limits: Record<string, BenefitLimitInfo> = {};
      for (const plan of res.data.benefitPlans ?? []) {
        for (const item of plan.items ?? []) {
          const current = limits[item.benefitId]?.limitAmount ?? 0;
          if (item.limitAmount >= current) {
            limits[item.benefitId] = {
              limitAmount: item.limitAmount,
              description: item.description ?? null,
            };
          }
        }
      }
      return limits;
    },

    async getByDocNum(docNum: string): Promise<BenefitClaim> {
      const res = await hrApi.get(`/BenefitClaim/by-docnum/${docNum}`);
      return res.data;
    },

    async getById(id: string): Promise<BenefitClaim> {
      const res = await hrApi.get(`/BenefitClaim/${id}`);
      return res.data;
    },
  },
});
