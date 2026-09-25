export interface PositionLevel {
  id: string
  code: string
  nameTh: string
  nameEn: string
  level: number
  track: string
  isSubsidiary: boolean
  isActive: boolean
}

export interface PositionLevelFilter {
  search?: string
  page: number
  pageSize: number
}
