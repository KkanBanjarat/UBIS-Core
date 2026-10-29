export type MenuNodeType = "Category" | "Module" | "Page";

export interface MenuAdmin {
  id: string;
  parentId: string | null;
  nodeType: MenuNodeType;
  code: string;
  label: string;
  path: string | null;
  componentPath: string | null;
  icon: string | null;
  permissionCode: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface SaveMenuPayload {
  parentId: string | null;
  nodeType: MenuNodeType;
  code: string;
  label: string;
  path: string | null;
  componentPath: string | null;
  icon: string | null;
  permissionCode: string | null;
  sortOrder: number;
  isActive: boolean;
}
