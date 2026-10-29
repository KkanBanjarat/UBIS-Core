import { computed } from "vue";
import { useMenuStore } from "../stores/menuStore";

export function usePermission(code: string) {
  const menuStore = useMenuStore();
  const level = computed(() => menuStore.levelOf(code));

  return {
    level,
    canRead: computed(() => level.value >= 1),
    canWrite: computed(() => level.value >= 2), // สร้าง/แก้ไข/ส่งอนุมัติ/ดึงกลับ
    canAll: computed(() => level.value >= 3), // ลบ และงานผู้ดูแล
  };
}
