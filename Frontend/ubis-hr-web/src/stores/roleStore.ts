import { defineStore } from "pinia";
import accessApi from "../services/accessApi";
import type {
  RoleItem,
  PermissionItem,
  RolePermissionGroup,
  RoleFormPayload,
  PermissionFormPayload,
} from "../types/Role";

export const useRoleStore = defineStore("role", {
  state: () => ({
    roles: [] as RoleItem[],
    permissions: [] as PermissionItem[],
    rolePermissions: [] as RolePermissionGroup[],
    isLoading: false,
    errorMessage: "",
  }),
  getters: {
    // key: `${roleId}:${permissionId}` -> rolePermissionId หรือ null ถ้ายังไม่ Grant
    grantMap(state): Record<string, string> {
      const map: Record<string, string> = {};
      for (const g of state.rolePermissions) {
        for (const p of g.permissions) {
          if (p.rolePermissionId)
            map[`${g.roleId}:${p.id}`] = p.rolePermissionId;
        }
      }
      return map;
    },
  },
  actions: {
    async fetchAll(silent = false) {
      if (!silent) this.isLoading = true;
      this.errorMessage = "";
      try {
        const [roles, permissions, rolePermissions] = await Promise.all([
          accessApi.get<RoleItem[]>("/Roles"),
          accessApi.get<PermissionItem[]>("/Permissions"),
          accessApi.get<RolePermissionGroup[]>("/RolePermissions"),
        ]);
        this.roles = roles.data;
        this.permissions = permissions.data;
        this.rolePermissions = rolePermissions.data;
      } catch (err) {
        console.error("Failed to load roles:", err);
        this.errorMessage = "โหลดข้อมูล Role/Permission ไม่สำเร็จ";
      } finally {
        this.isLoading = false;
      }
    },

    async fetchRolePermissions() {
      const res =
        await accessApi.get<RolePermissionGroup[]>("/RolePermissions");
      this.rolePermissions = res.data;
    },

    // ---------------- Role ----------------
    async createRole(payload: RoleFormPayload) {
      await accessApi.post("/Roles", payload);
      await this.fetchAll(true);
    },
    async updateRole(id: string, payload: RoleFormPayload) {
      await accessApi.put(`/Roles/${id}`, payload);
      await this.fetchAll(true);
    },
    async deleteRole(id: string) {
      await accessApi.delete(`/Roles/${id}`);
      await this.fetchAll(true);
    },

    // ---------------- Permission ----------------
    async createPermission(payload: PermissionFormPayload) {
      await accessApi.post("/Permissions", payload);
      await this.fetchAll(true);
    },
    async deletePermission(id: string) {
      await accessApi.delete(`/Permissions/${id}`);
      await this.fetchAll(true);
    },

    // ---------------- Grant / Revoke ----------------
    // Grant: ยังไม่เคยติ๊ก -> ติ๊ก
    async grant(roleId: string, permissionId: string) {
      await accessApi.post("/RolePermissions", { roleId, permissionId });
      await this.fetchRolePermissions();
    },
    // Revoke: ติ๊กอยู่ -> เอาออก
    async revoke(rolePermissionId: string) {
      await accessApi.delete(`/RolePermissions/${rolePermissionId}`);
      await this.fetchRolePermissions();
    },
  },
});
