import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type { PettyCash, PettyCashFilter } from "../types/PettyCash";

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
export const usePettyCashStore = defineStore("pettyCash", {
  state: () => ({
    items: [] as PettyCash[],
    totalCount: 0,
    isLoading: true, // เดิม false — กันเห็น "ไม่พบข้อมูล" วาบก่อนโหลด
    errorMessage: "", // เพิ่มใหม่
    benefitOptions: [] as OptionItem[],
    allowedEmployees: [] as EmployeeOption[],
    myEmployeeId: "",
    myEmployeeName: "",
  }),
  actions: {
    async fetchList(filter: PettyCashFilter) {
      const reqId = ++listReqId;
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await hrApi.post("/PettyCash/petty-cash-list", filter);
        if (reqId !== listReqId) return; // มี Request ใหม่กว่าแล้ว ทิ้ง Response เก่า
        this.items = res.data.items;
        this.totalCount = res.data.totalCount;
      } catch (err) {
        if (reqId !== listReqId) return;
        this.errorMessage = "โหลดรายการใบเบิกไม่สำเร็จ กรุณาลองใหม่";
        console.error("Failed to load petty cash list:", err);
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
      await hrApi.post("/PettyCash", payload);
    },

    async update(id: string, payload: any) {
      await hrApi.put(`/PettyCash/${id}`, payload);
    },

    async remove(id: string) {
      await hrApi.delete(`/PettyCash/${id}`);
    },

    async submit(id: string) {
      await hrApi.post(`/PettyCash/${id}/submit`);
    },

    async recall(id: string) {
      await hrApi.post(`/PettyCash/${id}/recall`);
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

    async getByDocNum(docNum: string): Promise<PettyCash> {
      const res = await hrApi.get(`/PettyCash/by-docnum/${docNum}`);
      return res.data;
    },
    async getById(id: string): Promise<PettyCash> {
      const res = await hrApi.get(`/PettyCash/${id}`);
      return res.data;
    },
  },
});
