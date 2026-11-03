export interface RoleItem {
  id: string;
  name: string;
  description: string | null;
}

export interface PermissionItem {
  id: string;
  code: string;
  description: string | null;
}

// ตรงกับ RolePermissionDto ฝั่ง .NET
export interface RolePermissionGroup {
  roleId: string;
  roleName: string | null;
  permissions: {
    id: string;
    code: string;
    description: string | null;
    rolePermissionId: string | null;
  }[];
}

export interface RoleFormPayload {
  name: string;
  description: string | null;
}

// ตรงกับ CreatePermissionDto ฝั่ง .NET
export interface PermissionFormPayload {
  code: string;
  description: string | null;
}
