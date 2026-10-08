export type RoleScope = "Self" | "Subordinate" | "Branch" | "All";

export interface RoleMember {
  userRoleId: string; // ใช้ตอนลบ
  userId: string;
  displayName: string;
  email: string;
  employeeCode: string | null;
  isActive: boolean;
  scope: RoleScope;
}
