export interface RoleMenuItem {
  menuId: string;
  parentId: string | null;
  nodeType: "Category" | "Module" | "Page";
  code: string;
  label: string;
  path: string | null;
  icon: string | null;
  sortOrder: number;
  accessLevel: number; // 0 ไม่มีสิทธิ์, 1 Read, 2 Write, 3 All
}

export interface RoleMenuAccess {
  menuId: string;
  accessLevel: number;
}
