import { defineStore } from "pinia";
import accessApi from "../services/accessApi";
import { fetchEntraUsers } from "../auth/graphUsers";
import type {
  UserItem,
  UserFilter,
  CreateUserPayload,
  UpdateUserPayload,
  SyncResult,
} from "../types/User";

interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

export const useUserStore = defineStore("user", {
  state: () => ({
    users: [] as UserItem[],
    totalCount: 0,
    isLoading: false,
    isSyncing: false,
    errorMessage: "",
  }),
  actions: {
    // แบ่งหน้าและกรองที่ฝั่ง Server เหมือนหน้ารายชื่อพนักงาน
    async fetchList(filter: UserFilter) {
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await accessApi.post<PagedResult<UserItem>>(
          "/Users/user-list",
          {
            search: filter.search || null,
            status: filter.status,
            source: filter.source,
            page: filter.page,
            pageSize: filter.pageSize,
          },
        );
        this.users = res.data.items;
        this.totalCount = res.data.totalCount;
      } catch (err) {
        console.error("Failed to load users:", err);
        this.errorMessage = "โหลดรายชื่อผู้ใช้งานไม่สำเร็จ";
      } finally {
        this.isLoading = false;
      }
    },

    async create(payload: CreateUserPayload) {
      return (await accessApi.post<UserItem>("/Users", payload)).data;
    },

    async update(id: string, payload: UpdateUserPayload) {
      await accessApi.put(`/Users/${id}`, payload);
    },

    async remove(id: string) {
      await accessApi.delete(`/Users/${id}`);
    },

    async resetPassword(id: string, newPassword: string) {
      await accessApi.put(`/Users/${id}/password`, { newPassword });
    },

    async syncEntra(): Promise<SyncResult> {
      this.isSyncing = true;
      try {
        // ดึงรายชื่อจาก Microsoft ด้วย token ของแอดมินที่ Login อยู่ แล้วส่งให้ Backend บันทึก
        const entraUsers = await fetchEntraUsers();
        // timeout ปกติของ accessApi คือ 15 วินาที ไม่พอถ้ามีผู้ใช้เยอะ
        return (
          await accessApi.post<SyncResult>("/Users/sync-entra", entraUsers, {
            timeout: 120000,
          })
        ).data;
      } finally {
        this.isSyncing = false;
      }
    },
  },
});
