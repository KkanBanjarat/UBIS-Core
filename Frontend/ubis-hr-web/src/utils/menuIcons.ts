import {
  LayoutDashboard,
  Gift,
  Receipt,
  ClipboardCheck,
  Users,
  Briefcase,
  Settings,
  Circle,
} from "lucide-vue-next";
import type { Component } from "vue";

// ชื่อใน tb_menu.Icon -> คอมโพเนนต์
// เพิ่มไอคอนใหม่: import ด้านบน แล้วเพิ่มบรรทัดในแมปนี้
const icons: Record<string, Component> = {
  LayoutDashboard,
  Gift,
  Receipt,
  ClipboardCheck,
  Users,
  Briefcase,
  Settings,
};

export function resolveIcon(name?: string | null): Component {
  return (name && icons[name]) || Circle;
}
