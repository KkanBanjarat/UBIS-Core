export interface OrganizationUnit {
  id: string
  code: string | null
  nameTh: string
  nameEn: string
  shortName: string | null
  type: string // Group, Department, Division, Section (ไม่รวม Company)
}

export interface OrganizationLevelType {
  id: string
  nameTh: string
  nameEn: string
  sequence: number
}