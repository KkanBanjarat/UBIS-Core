export interface PrettyCashLine {
  id: string
  benefitId: string | null
  benefitNameTh: string | null
  detail: string
  limitAmount: number
  amount: number
  qty: number
  accountCode: string | null
}

export interface PrettyCash {
  id: string
  docNum: string
  docStatus: string
  docDate: string
  employeeId: string
  employeeNameTh: string | null
  remark: string | null
  totalAmount: number
  lines: PrettyCashLine[]
  createdBy: string
  createdAt: string
  updatedBy: string
  updatedAt: string
}

export interface PrettyCashFilter {
  search: string
  docStatus: string | null
  employeeId: string | null
  page: number
  pageSize: number
}