import { defineStore } from "pinia";
import accessApi from "../services/accessApi";
import type { MenuAdmin, SaveMenuPayload } from "../types/MenuAdmin";
import { useMenuStore } from "./menuStore";

export const useMenuAdminStore = defineStore("menuAdmin", {
  state: () => ({
    items: [] as MenuAdmin[],
    isLoading: false,
    error: "" as string,
  }),

  actions: {
    async fetchAll() {
      this.isLoading = true;
      this.error = "";
      try {
        const res = await accessApi.get<MenuAdmin[]>("/menus");
        this.items = res.data;
      } catch (e: any) {
        this.error = e?.response?.data?.message ?? "โหลดเมนูไม่สำเร็จ";
        throw e;
      } finally {
        this.isLoading = false;
      }
    },

    async create(payload: SaveMenuPayload) {
      const res = await accessApi.post<MenuAdmin>("/menus", payload);
      this.items.push(res.data);
      this.refreshSidebar();
      return res.data;
    },

    async update(id: string, payload: SaveMenuPayload) {
      const res = await accessApi.put<MenuAdmin>(`/menus/${id}`, payload);
      const i = this.items.findIndex((x) => x.id === id);
      if (i >= 0) this.items[i] = res.data;
      this.refreshSidebar();
      return res.data;
    },

    async remove(id: string) {
      await accessApi.delete(`/menus/${id}`);
      this.items = this.items.filter((x) => x.id !== id);
      this.refreshSidebar();
    },

    // โหลดเมนู sidebar ของตัวเองใหม่ในเบื้องหลัง ให้เห็นผลทันที
    refreshSidebar() {
      useMenuStore()
        .load()
        .catch(() => {});
    },
  },
});
