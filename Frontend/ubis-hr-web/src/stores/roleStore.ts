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
    permissionsLoaded: false,
    isLoadingPermissions: false,
    permissionsError: "",
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
    // โหลดเฉพาะรายการ Role (เส้นเบา) ใช้ตอนเปิดหน้า
    async fetchRoles() {
      this.isLoading = true;
      this.errorMessage = "";
      try {
        const res = await accessApi.get<RoleItem[]>("/Roles");
        this.roles = res.data;
      } catch (err) {
        console.error("Failed to load roles:", err);
        this.errorMessage = "โหลดรายการ Role ไม่สำเร็จ";
      } finally {
        this.isLoading = false;
      }
    },

    // โหลด Permission + การผูกสิทธิ์ ครั้งเดียวตอนเปิดแท็บ Permission
    async fetchPermissionData(force = false) {
      if (this.permissionsLoaded && !force) return;
      if (this.isLoadingPermissions) return;
      this.isLoadingPermissions = true;
      this.permissionsError = "";
      try {
        const [permissions, rolePermissions] = await Promise.all([
          accessApi.get<PermissionItem[]>("/Permissions"),
          accessApi.get<RolePermissionGroup[]>("/RolePermissions"),
        ]);
        this.permissions = permissions.data;
        this.rolePermissions = rolePermissions.data;
        this.permissionsLoaded = true;
      } catch (err) {
        console.error("Failed to load permissions:", err);
        this.permissionsError = "โหลดข้อมูล Permission ไม่สำเร็จ";
      } finally {
        this.isLoadingPermissions = false;
      }
    },
    async fetchRolePermissions() {
      const res =
        await accessApi.get<RolePermissionGroup[]>("/RolePermissions");
      this.rolePermissions = res.data;
    },

    // ---------------- Role ----------------
    async createRole(payload: RoleFormPayload) {
      const res = await accessApi.post<RoleItem>("/Roles", payload);
      this.roles.push(res.data);
    },
    async updateRole(id: string, payload: RoleFormPayload) {
      const res = await accessApi.put<RoleItem>(`/Roles/${id}`, payload);
      const i = this.roles.findIndex((r) => r.id === id);
      if (i >= 0) this.roles[i] = res.data;
    },
    async deleteRole(id: string) {
      await accessApi.delete(`/Roles/${id}`);
      this.roles = this.roles.filter((r) => r.id !== id);
      this.rolePermissions = this.rolePermissions.filter(
        (g) => g.roleId !== id,
      );
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
    async grant(roleId: string, permissionId: string) {
      const res = await accessApi.post<RolePermissionGroup>(
        "/RolePermissions",
        {
          roleId,
          permissionId,
        },
      );
      // response คือกลุ่มของ role นั้นที่มีสิทธิ์ใหม่ 1 รายการ
      const group = this.rolePermissions.find((g) => g.roleId === roleId);
      if (group) group.permissions.push(...res.data.permissions);
      else this.rolePermissions.push(res.data);
    },
    async revoke(rolePermissionId: string) {
      await accessApi.delete(`/RolePermissions/${rolePermissionId}`);
      for (const g of this.rolePermissions) {
        const i = g.permissions.findIndex(
          (p) => p.rolePermissionId === rolePermissionId,
        );
        if (i >= 0) {
          g.permissions.splice(i, 1);
          break;
        }
      }
    },
  },
});
