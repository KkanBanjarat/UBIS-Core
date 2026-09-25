import { defineStore } from "pinia";
import { msalInstance, ensureMsalInitialized } from "../auth/msalConfig";
import accessApi from "../services/accessApi";
import hrApi from "../services/hrApi";
import type { Employee } from "../types/Employee";
import { jwtDecode } from "jwt-decode"; // npm install jwt-decode

interface LoginResponse {
  token: string;
  expiresAt: string;
  email: string;
  displayName: string;
}

interface DecodedToken {
  employeeCode?: string;
  sub: string;
  email: string;
  perm?: string[];
}

export const useAuthStore = defineStore("auth", {
  state: () => ({
    token: localStorage.getItem("token") as string | null,
    email: localStorage.getItem("email") as string | null,
    displayName: localStorage.getItem("displayName") as string | null,
    employeeCode: localStorage.getItem("employeeCode") as string | null,
    permissions: JSON.parse(
      localStorage.getItem("permissions") || "[]",
    ) as string[],
    employee: null as Employee | null,
  }),
  getters: {
    // เช็คว่ามี Permission Code นี้ไหม (ไม่สนใจ Scope ต่อท้าย เช่น "employee.write:Branch" ก็ Match "employee.write")
    hasPermission: (state) => (code: string) => {
      return state.permissions.some(
        (p) =>
          p === "*" ||
          p.startsWith("*:") ||
          p === code ||
          p.startsWith(code + ":"),
      );
    },
  },

  actions: {
    async login(email: string, password: string) {
      const res = await accessApi.post<LoginResponse>("/Auth", {
        email,
        password,
      });
      this.setToken(res.data.token, res.data.email, res.data.displayName);
    },

    setToken(token: string, email: string, displayName: string) {
      const decoded = jwtDecode<DecodedToken>(token);
      const employeeCode = decoded.employeeCode || "";
      const permissions = decoded.perm || [];

      this.token = token;
      this.email = email;
      this.displayName = displayName;
      this.employeeCode = employeeCode;
      this.permissions = permissions;

      localStorage.setItem("token", token);
      localStorage.setItem("email", email);
      localStorage.setItem("displayName", displayName);
      localStorage.setItem("employeeCode", employeeCode);
      localStorage.setItem("permissions", JSON.stringify(permissions));
    },

    logout() {
      this.token = null;
      this.email = null;
      this.displayName = null;
      this.employeeCode = null;
      this.permissions = [];
      this.employee = null;
      localStorage.removeItem("token");
      localStorage.removeItem("email");
      localStorage.removeItem("displayName");
      localStorage.removeItem("employeeCode");
      localStorage.removeItem("permissions");
    },

    async loginWithSso() {
      await ensureMsalInitialized();
      await msalInstance.loginRedirect({ scopes: ["User.Read"] });
    },

    async handleSsoRedirect() {
      // ถ้ามี Token เดิมอยู่แล้วและยังไม่หมดอายุ ไม่ต้อง Init MSAL / Sync ใหม่
      if (this.token && !this.isTokenExpired(this.token)) {
        return true;
      }

      const result = await ensureMsalInitialized();
      if (!result) return false;

      const response = await accessApi.post(
        "/Auth/sync-me",
        {},
        {
          headers: { Authorization: `Bearer ${result.idToken}` },
        },
      );
      const { token, email, displayName } = response.data;
      this.setToken(token, email, displayName);
      return true;
    },

    isTokenExpired(token: string): boolean {
      try {
        const decoded = jwtDecode<{ exp: number }>(token);
        return decoded.exp * 1000 < Date.now();
      } catch {
        return true; // decode ไม่ได้ ถือว่าหมดอายุ ให้ Sync ใหม่
      }
    },

    // เพิ่ม method นี้
    async fetchCurrentEmployee() {
      if (!this.token) return null;
      try {
        const response = await hrApi.get("/employees/me", {
          headers: { Authorization: `Bearer ${this.token}` },
        });
        this.employee = response.data;
        return response.data;
      } catch (error) {
        console.error("Failed to fetch employee:", error);
        return null;
      }
    },
  },
});
