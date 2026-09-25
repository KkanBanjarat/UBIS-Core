import {
  LayoutDashboard,
  Users,
  Briefcase,
  Receipt,
  ClipboardCheck,
  Settings,
} from "lucide-vue-next";
import type { Component } from "vue";

export interface MenuItem {
  path: string;
  label: string;
  icon: Component;
  permission?: string;
  children?: { path: string; label: string }[];
}

export interface MenuGroup {
  label: string | null;
  items: MenuItem[];
}

export const menuGroups: MenuGroup[] = [
  {
    label: null,
    items: [
      { path: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
      {
        path: "/pretty-cash",
        label: "เบิกสวัสดิการ/เงินสดย่อย",
        icon: Receipt,
      },
      { path: "/approvals", label: "รายการรออนุมัติ", icon: ClipboardCheck },
    ],
  },
  {
    label: "จัดการ HR",
    items: [
      {
        path: "/employees",
        label: "Employee Management",
        icon: Users,
        permission: "employee.write",
      },
      {
        path: "/master-data",
        label: "Master Data",
        icon: Briefcase,
        permission: "employee.write",
        children: [
          { path: "/companies", label: "บริษัท/สาขา" },
          { path: "/org-unit", label: "โครงสร้างองค์กร" },
          { path: "/positions", label: "ตำแหน่งงาน" },
          { path: "/position-levels", label: "ระดับตำแหน่ง" },
          { path: "/benefits", label: "สวัสดิการ" },
        ],
      },
    ],
  },
  {
    label: "ตั้งค่าระบบ",
    items: [
      {
        path: "/settings",
        label: "System Settings",
        icon: Settings,
        permission: "system.admin",
        children: [
          { path: "/settings/approve-routes", label: "สายอนุมัติเอกสาร" },
          { path: "/settings/branch-admins", label: "ผู้ดูแลสาขา" },
        ],
      },
    ],
  },
];
