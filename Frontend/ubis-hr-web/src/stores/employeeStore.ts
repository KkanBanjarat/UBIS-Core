import { defineStore } from "pinia";
import hrApi from "../services/hrApi";
import type { Employee, EmployeeFilter } from "../types/Employee";

export interface OptionItem {
  id: string;
  label: string;
}

export const useEmployeeStore = defineStore("employee", {
  state: () => ({
    // List
    employees: [] as Employee[],
    totalCount: 0,
    isLoading: false,
    errorMessage: "",

    // Master Data Options (โหลดครั้งเดียวพอ ใช้ซ้ำได้ทั้ง Session)
    positionOptions: [] as OptionItem[],
    positionLevelOptions: [] as OptionItem[],
    employeeTypeOptions: [] as OptionItem[],
    companyOptions: [] as OptionItem[],
    groupOptions: [] as OptionItem[],
    departmentOptions: [] as OptionItem[],
    divisionOptions: [] as OptionItem[],
    sectionOptions: [] as OptionItem[],
    benefitPlanOptions: [] as OptionItem[],
    isOptionsLoaded: false,
  }),

  actions: {
    async fetchList(filter: EmployeeFilter) {
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await hrApi.post("/Employees/employee-list", filter);
        this.employees = res.data.items;
        this.totalCount = res.data.totalCount;
      } catch (err) {
        this.errorMessage = "โหลดข้อมูลพนักงานไม่สำเร็จ";
        console.error("Failed to fetch employees:", err);
      } finally {
        this.isLoading = false;
      }
    },
    async fetchOptions(force = false) {
      if (this.isOptionsLoaded && !force) return;

      try {
        const [posRes, lvlRes, typeRes, compRes, orgUnitRes, benefitPlanRes] =
          await Promise.all([
            hrApi.get("/positions"),
            hrApi.get("/positionlevels"),
            hrApi.get("/employeetypes"),
            hrApi.get("/companies"),
            hrApi.get("/organizationunits"),
            hrApi.get("/benefitplans"),
          ]);

        this.positionOptions = posRes.data.map((x: any) => ({
          id: x.id,
          label: `${x.nameEn} (${x.nameTh})`,
        }));

        this.positionLevelOptions = lvlRes.data.map((x: any) => ({
          id: x.id,
          label: `L${x.level} : ${x.nameEn} (${x.nameTh})`,
        }));

        this.employeeTypeOptions = typeRes.data.map((x: any) => ({
          id: x.id,
          label: x.nameTh,
        }));

        this.companyOptions = compRes.data.map((x: any) => ({
          id: x.id,
          label: x.nameTh,
        }));

        const mapOrgUnit = (x: any): OptionItem => ({
          id: x.id,
          label: `${x.nameEn} (${x.nameTh})`,
        });
        const orgUnits: any[] = orgUnitRes.data;
        this.groupOptions = orgUnits
          .filter((x) => x.type === "Group")
          .map(mapOrgUnit);
        this.departmentOptions = orgUnits
          .filter((x) => x.type === "Department")
          .map(mapOrgUnit);
        this.divisionOptions = orgUnits
          .filter((x) => x.type === "Division")
          .map(mapOrgUnit);
        this.sectionOptions = orgUnits
          .filter((x) => x.type === "Section")
          .map(mapOrgUnit);

        this.benefitPlanOptions = benefitPlanRes.data
          .filter((x: any) => x.isActive)
          .map((x: any) => ({ id: x.id, label: `${x.nameTh} (${x.nameEn})` }));

        this.isOptionsLoaded = true;
      } catch (err) {
        console.error("Failed to load options:", err);
        this.errorMessage = "โหลดตัวเลือกไม่สำเร็จ";
      }
    },

    /**
     * สาขาตามบริษัท — ไม่เก็บใน State เพราะหน้าเดียวมีได้ 2 ชุดพร้อมกัน
     * (ชุดของ Filter กับชุดของ Form แก้ไข) คืนค่ากลับให้ Component เก็บเอง
     */
    async getBranchesByCompany(companyId: string): Promise<OptionItem[]> {
      const res = await hrApi.get(`/branchs/by-company?companyId=${companyId}`);
      return res.data.map((x: any) => ({
        id: x.id,
        label: `${x.nameEn} (${x.nameTh})`,
      }));
    },

    async getById(id: string, includeOrgChart = true) {
      const res = await hrApi.get(
        `/Employees/${id}?includeOrgChart=${includeOrgChart}`,
      );
      return res.data;
    },

    async create(payload: any) {
      await hrApi.post("/Employees", payload);
    },

    async update(id: string, payload: any) {
      await hrApi.put(`/Employees/${id}`, payload);
    },

    async remove(id: string) {
      await hrApi.delete(`/Employees/${id}`);
    },
  },
});
