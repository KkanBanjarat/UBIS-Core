export interface Benefit {
  id: string
  nameTh: string
  nameEn: string
  isActive: boolean
}

export interface BenefitPlan {
  id: string
  nameTh: string
  nameEn: string
  description: string | null
  isActive: boolean
}

export interface BenefitPlanItem {
  id: string
  benefitPlanId: string
  benefitPlanNameTh: string
  benefitPlanNameEn: string
  benefitId: string
  benefitNameTh: string
  benefitNameEn: string
  limitAmount: number
  description: string | null
  isActive: boolean
}

export interface EmployeeBenefitPlanItemSummary {
  benefitId: string
  benefitNameTh: string
  benefitNameEn: string
  limitAmount: number
  description: string | null
}

export interface EmployeeBenefitPlanSummary {
  id: string
  nameTh: string
  nameEn: string
  items?: EmployeeBenefitPlanItemSummary[]
}