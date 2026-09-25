import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type { PrettyCash, PrettyCashFilter } from "../types/PrettyCash";

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

export const usePrettyCashStore = defineStore("prettyCash", {
  state: () => ({
    items: [] as PrettyCash[],
    totalCount: 0,
    isLoading: false,
    benefitOptions: [] as OptionItem[],
    allowedEmployees: [] as EmployeeOption[],
    myEmployeeId: "",
    myEmployeeName: "",
  }),

  actions: {
    async fetchList(filter: PrettyCashFilter) {
      this.isLoading = true;
      try {
        const res = await hrApi.post("/PrettyCash/pretty-cash-list", filter);
        this.items = res.data.items;
        this.totalCount = res.data.totalCount;
      } finally {
        this.isLoading = false;
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
      await hrApi.post("/PrettyCash", payload);
    },

    async update(id: string, payload: any) {
      await hrApi.put(`/PrettyCash/${id}`, payload);
    },

    async remove(id: string) {
      await hrApi.delete(`/PrettyCash/${id}`);
    },

    async submit(id: string) {
      await hrApi.post(`/PrettyCash/${id}/submit`);
    },

    async recall(id: string) {
      await hrApi.post(`/PrettyCash/${id}/recall`);
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

    async getByDocNum(docNum: string): Promise<PrettyCash> {
      const res = await hrApi.get(`/PrettyCash/by-docnum/${docNum}`);
      return res.data;
    },
  },
});
