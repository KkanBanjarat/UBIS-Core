import { defineStore } from "pinia";
import accessApi from "../services/accessApi";
import type { RoleMember, RoleScope } from "../types/RoleMember";

export const useRoleMemberStore = defineStore("roleMember", {
  actions: {
    async fetch(roleId: string): Promise<RoleMember[]> {
      const res = await accessApi.get<RoleMember[]>(`/roles/${roleId}/members`);
      return res.data;
    },
    async add(userId: string, roleId: string, scope: RoleScope) {
      await accessApi.post("/UserRoles", { userId, roleId, scope });
    },
    async remove(userRoleId: string) {
      await accessApi.delete(`/UserRoles/${userRoleId}`);
    },
  },
});
