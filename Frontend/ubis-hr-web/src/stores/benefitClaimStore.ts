import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type {
  BenefitClaim,
  BenefitClaimFilter,
  BenefitClaimSummary,
  BenefitClaimSummaryFilter,
  BenefitClaimDetailRow,
  BenefitClaimEmployeeFilter,
  EmployeeSummary,
} from "../types/BenefitClaim";

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
let summaryReqId = 0;
let detailReqId = 0;
let employeeReqId = 0;

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
    summary: null as BenefitClaimSummary | null,
    isSummaryLoading: false,
    summaryError: "",
    detailRows: [] as BenefitClaimDetailRow[],
    isDetailLoading: false,
    detailError: "",
    employeeItems: [] as EmployeeSummary[],
    employeeTotal: 0,
    isEmployeeLoading: false,
    employeeError: "",
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
        console.log(err);
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

    async fetchSummary(filter: BenefitClaimSummaryFilter) {
      const reqId = ++summaryReqId;
      this.isSummaryLoading = true;
      this.summaryError = "";
      try {
        const res = await hrApi.post(
          "/BenefitClaim/benefit-claim-summary-report",
          filter,
        );
        if (reqId !== summaryReqId) return;
        this.summary = res.data;
      } catch (err: any) {
        console.log(err);
        if (reqId !== summaryReqId) return;
        this.summary = null;
        this.summaryError =
          err?.response?.status === 403
            ? "เฉพาะ HR Admin เท่านั้นที่ดูรายงานสรุปได้"
            : "โหลดรายงานสรุปไม่สำเร็จ กรุณาลองใหม่";
        console.error("Failed to load benefit claim summary:", err);
      } finally {
        if (reqId === summaryReqId) this.isSummaryLoading = false;
      }
    },
    async fetchEmployees(filter: BenefitClaimEmployeeFilter) {
      const reqId = ++employeeReqId;
      this.isEmployeeLoading = true;
      this.employeeError = "";
      try {
        const res = await hrApi.post(
          "/BenefitClaim/benefit-claim-summary-employees",
          filter,
        );
        if (reqId !== employeeReqId) return;
        this.employeeItems = res.data.items;
        this.employeeTotal = res.data.totalCount;
      } catch (err) {
        if (reqId !== employeeReqId) return;
        this.employeeItems = [];
        this.employeeTotal = 0;
        this.employeeError = "โหลดรายชื่อพนักงานไม่สำเร็จ กรุณาลองใหม่";
        console.error("Failed to load employee summary:", err);
      } finally {
        if (reqId === employeeReqId) this.isEmployeeLoading = false;
      }
    },

    async fetchEmployeeRows(
      filter: BenefitClaimSummaryFilter,
      employeeId: string,
    ): Promise<BenefitClaimDetailRow[]> {
      const res = await hrApi.post(
        "/BenefitClaim/benefit-claim-summary-employee-rows",
        { ...filter, employeeId },
      );
      return res.data;
    },
    async exportSummary(filter: BenefitClaimSummaryFilter) {
      const res = await hrApi.post(
        "/BenefitClaim/benefit-claim-summary-export",
        filter,
        { responseType: "blob" },
      );
      const url = URL.createObjectURL(res.data);
      const a = document.createElement("a");
      a.href = url;
      a.download = `BenefitClaimSummary_${new Date().toISOString().slice(0, 10)}.xlsx`;
      a.click();
      URL.revokeObjectURL(url);
    },
    async fetchDetail(filter: BenefitClaimSummaryFilter) {
      const reqId = ++detailReqId;
      this.isDetailLoading = true;
      this.detailError = "";
      try {
        const res = await hrApi.post(
          "/BenefitClaim/benefit-claim-summary-detail",
          filter,
        );
        if (reqId !== detailReqId) return;
        this.detailRows = res.data;
      } catch (err: any) {
        if (reqId !== detailReqId) return;
        this.detailRows = [];
        this.detailError =
          err?.response?.status === 403
            ? "เฉพาะ HR Admin เท่านั้นที่ดูรายงานสรุปได้"
            : "โหลดรายละเอียดไม่สำเร็จ กรุณาลองใหม่";
        console.error("Failed to load benefit claim detail:", err);
      } finally {
        if (reqId === detailReqId) this.isDetailLoading = false;
      }
    },
  },
});
