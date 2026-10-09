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
