export interface UserItem {
  id: string;
  entraObjectId?: string | null;
  email: string;
  displayName: string;
  employeeId?: string | null;
  employeeCode?: string | null;
  isActive: boolean;
  isEntra: boolean;
  lastLoginAt?: string | null;
}

export interface UserFilter {
  search: string;
  status: string | null; // "active" | "inactive"
  source: string | null; // "entra" | "local"
  page: number;
  pageSize: number;
}

// ตรงกับ CreateUserDto ฝั่ง .NET (สร้างผู้ใช้ที่ไม่มี Microsoft)
export interface CreateUserPayload {
  email: string;
  displayName: string;
  employeeCode: string | null;
  employeeId: string | null;
  password: string;
  isActive: boolean;
}

// ตรงกับ UpdateUserDto ฝั่ง .NET
export interface UpdateUserPayload {
  email: string;
  displayName: string;
  employeeCode: string | null;
  employeeId: string | null;
  isActive: boolean;
}

// ตรงกับ EntraSyncResultDto ฝั่ง .NET
export interface SyncResult {
  total: number;
  created: number;
  updated: number;
  linked: number;
  unchanged: number;
  skipped: number;
}
