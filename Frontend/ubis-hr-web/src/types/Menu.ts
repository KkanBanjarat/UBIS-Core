export interface MenuNode {
  id: string;
  parentId: string | null;
  nodeType: "Category" | "Module" | "Page";
  code: string;
  label: string;
  path: string | null;
  componentPath: string | null;
  icon: string | null;
  permissionCode: string | null;
  sortOrder: number;
  accessLevel: number; // เฉพาะ Page: 1 Read, 2 Write, 3 All
  children: MenuNode[];
}
