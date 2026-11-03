import { defineStore } from "pinia";
import accessApi from "../services/accessApi";
import type { RoleMenuItem, RoleMenuAccess } from "../types/RoleMenu";

export const useRoleMenuStore = defineStore("roleMenu", {
  actions: {
    async fetch(roleId: string): Promise<RoleMenuItem[]> {
      const res = await accessApi.get<RoleMenuItem[]>(`/roles/${roleId}/menus`);
      return res.data;
    },
    async save(roleId: string, items: RoleMenuAccess[]) {
      await accessApi.put(`/roles/${roleId}/menus`, { items });
    },
  },
});
