export interface Position {
  id: string
  nameTh: string
  nameEn: string
  isSubsidiary: boolean
  isActive: boolean
}

export interface PositionFilter {
  search?: string
  page: number
  pageSize: number
}