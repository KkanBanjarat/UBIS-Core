export interface PettyCashLine {
  id: string;
  benefitId: string | null;
  benefitNameTh: string | null;
  detail: string;
  limitAmount: number;
  amount: number;
  qty: number;
  accountCode: string | null;
}

export interface PettyCash {
  id: string;
  docNum: string;
  docStatus: string;
  docDate: string;
  employeeId: string;
  employeeNameTh: string | null;
  remark: string | null;
  totalAmount: number;
  lineCount?: number;
  hasBenefitLine?: boolean;
  hasCashLine?: boolean;
  createdBy: string;
  createdAt: string;
  updatedBy: string;
  updatedAt: string;
  lines: PettyCashLine[];
}

export interface PettyCashFilter {
  search: string;
  docStatus: string | null;
  employeeId: string | null;
  page: number;
  pageSize: number;
}
