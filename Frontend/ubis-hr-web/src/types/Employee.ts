// Type definition for Employee
import type { EmployeeBenefitPlanSummary } from './Benefit'
export interface Employee {
  id: string
  empId: string
  prefixNameTh: string | null
  prefixNameEn: string | null
  fNameTh: string
  lNameTh: string
  fNameEn: string
  lNameEn: string
  email: string
  hireDate: string
  positionId: string
  positionNameTh: string
  positionNameEn?: string
  positionLevelId: string
  positionLevel: number
  positionLevelNameTh: string
  positionLevelNameEn?: string
  employeeTypeId: string
  employeeTypeNameTh: string
  employeeTypeNameEn?: string
  status: string
  gender?: string | null
  companyId: string
  companyCode: string
  companyNameTh?: string
  companyNameEn?: string
  branchId: string
  branchNameTh: string
  branchNameEn?: string
  groupId?: string | null
  groupNameTh?: string | null
  groupNameEn?: string | null
  departmentId?: string | null
  departmentNameTh?: string | null
  departmentNameEn?: string | null
  divisionId?: string | null
  divisionNameTh?: string | null
  divisionNameEn?: string | null
  sectionId?: string | null
  sectionNameTh?: string | null
  sectionNameEn?: string | null
  reportToId?: string | null
  reportToNameTh?: string | null
  reportToNameEn?: string | null
  organizationUnitNameTh?: string | null
  organizationUnitNameEn?: string | null
  createdAt?: string | null
  createdBy?: string | null
  updatedAt?: string | null
  updatedBy?: string | null
  benefitPlans?: EmployeeBenefitPlanSummary[]
  orgChart?: EmployeeOrgChartNode | null
}

export interface EmployeeFilter {
  search?: string | null;
  status?: string | null;
  employeeTypeId?: string | null;
  companyId?: string | null;
  branchId?: string | null;
  groupId?: string | null;
  departmentId?: string | null;
  divisionId?: string | null;
  sectionId?: string | null;
  page: number;
  pageSize: number;
}

export interface EmployeeOrgChartNode {
  id: string
  empId: string
  fullNameTh: string
  fullNameEn: string
  positionNameTh: string | null
  positionNameEn: string | null
  isSelf: boolean
  children?: EmployeeOrgChartNode[]
}