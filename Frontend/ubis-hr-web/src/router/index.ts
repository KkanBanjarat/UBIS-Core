import { createRouter, createWebHistory } from "vue-router";
import LoginView from "../views/LoginView.vue";
import AppLayout from "../components/AppLayout.vue";

const routes = [
  { path: "/login", name: "login", component: LoginView },
  {
    path: "/",
    component: AppLayout,
    meta: { requiresAuth: true },
    children: [
      { path: "", redirect: "/dashboard" },
      {
        path: "dashboard",
        name: "dashboard",
        component: () => import("../views/DashboardView.vue"),
      },
      {
        path: "pretty-cash",
        name: "pretty-cash",
        component: () => import("../views/prettycash/PrettyCashListView.vue"),
      },
      {
        path: "approvals",
        name: "approvals",
        component: () => import("../views/approval/ApprovalsView.vue"),
      },
      {
        path: "employees",
        name: "employees",
        component: () => import("../views/employee/EmployeeListView.vue"),
        meta: { requiresPermission: "employee.write" },
      },
      {
        path: "companies",
        name: "companies",
        component: () => import("../views/master-data/CompanyView.vue"),
        meta: { requiresPermission: "employee.write" },
      },
      {
        path: "org-unit",
        name: "organization-unit",
        component: () =>
          import("../views/master-data/OrganizationUnitView.vue"),
        meta: { requiresPermission: "employee.write" },
      },
      {
        path: "positions",
        name: "positions",
        component: () => import("../views/master-data/PositionView.vue"),
        meta: { requiresPermission: "employee.write" },
      },
      {
        path: "benefits",
        name: "benefits",
        component: () => import("../views/master-data/BenefitView.vue"),
        meta: { requiresPermission: "employee.write" },
      },
      {
        path: "position-levels",
        name: "position-levels",
        component: () => import("../views/master-data/PositionLevelView.vue"),
        meta: { requiresPermission: "employee.write" },
      },
      {
        path: "position-levels",
        name: "position-levels",
        component: () => import("../views/master-data/PositionLevelView.vue"),
        meta: { requiresPermission: "employee.write" },
      },
      {
        path: "settings/approve-routes",
        name: "approve-routes",
        component: () => import("../views/settings/ApproveRouteView.vue"),
        meta: { requiresPermission: "system.admin" },
      },
      {
        path: "settings/branch-admins",
        name: "branch-admins",
        component: () => import("../views/settings/BranchAdminView.vue"),
        meta: { requiresPermission: "system.admin" },
      },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem("token");

  if (to.meta.requiresAuth && !token) {
    next("/login");
    return;
  }

  if (token && to.path.toLowerCase() === "/login") {
    next("/dashboard");
    return;
  }

  const requiredPermission = to.meta.requiresPermission as string | undefined;
  if (requiredPermission) {
    const permissions: string[] = JSON.parse(
      localStorage.getItem("permissions") || "[]",
    );
    const hasIt = permissions.some(
      (p) =>
        p === "*" ||
        p.startsWith("*:") ||
        p === requiredPermission ||
        p.startsWith(requiredPermission + ":"),
    );
    if (!hasIt) {
      next("/dashboard");
      return;
    }
  }

  next();
});

export default router;
