export interface BenefitClaimLine {
  id: string;
  benefitId: string;
  benefitNameTh: string | null;
  detail: string;
  limitAmount: number;
  amount: number;
  qty: number;
  accountCode: string | null;
}

export interface BenefitClaim {
  id: string;
  docNum: string;
  docStatus: string;
  docDate: string;
  employeeId: string;
  employeeNameTh: string | null;
  affiliation?: string | null;
  affiliationCode?: string | null;
  positionTh: string | null;
  positionEn: string | null;
  positionLevel: string | null;
  positionLevelNameTh: string | null;
  positionLevelNameEn: string | null;
  company: string | null;
  companyCode?: string | null;
  branch: string | null;
  remark: string | null;
  totalAmount: number;
  lineCount?: number;
  createdBy: string;
  createdAt: string;
  updatedBy: string;
  updatedAt: string;
  lines: BenefitClaimLine[];
}

export interface BenefitClaimFilter {
  search: string;
  docStatus: string | null;
  employeeId: string | null;
  page: number;
  pageSize: number;
}

export interface BenefitClaimSummaryFilter {
  dateFrom: string | null;
  dateTo: string | null;
  companyId: string | null;
  docStatus: string | null;
}

export interface SummaryItem {
  key: string;
  label: string;
  count: number;
  amount: number;
}

export interface BenefitClaimSummary {
  byCompany: CompanySummary[];
}
export interface BenefitClaimDetailRow {
  company: string | null;
  docNum: string;
  docDate: string;
  docStatus: string;
  employeeCode: string;
  employeeName: string;
  benefitName: string;
  detail: string | null;
  qty: number;
  amount: number;
}

export interface CompanyBenefit {
  benefit: string;
  docCount: number;
  lineCount: number;
  amount: number;
}

export interface CompanyBranch {
  branch: string;
  docCount: number;
  lineCount: number;
  amount: number;
  benefits: CompanyBenefit[];
}

export interface CompanySummary {
  company: string;
  docCount: number;
  lineCount: number;
  amount: number;
  branches: CompanyBranch[];
}

export interface EmployeeSummary {
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  company: string;
  docCount: number;
  amount: number;
}

export interface BenefitClaimEmployeeFilter extends BenefitClaimSummaryFilter {
  search: string | null;
  page: number;
  pageSize: number;
}
