import {
  createRouter,
  createWebHistory,
  type RouteComponent,
  type RouteRecordRaw,
} from "vue-router";
import LoginView from "../views/LoginView.vue";
import AppLayout from "../components/AppLayout.vue";
import { useAuthStore } from "../stores/authStore";
import { useMenuStore } from "../stores/menuStore";

// โหลด view ทุกไฟล์ล่วงหน้าแบบ lazy: key เช่น "../views/DashboardView.vue"
const viewModules = import.meta.glob("../views/**/*.vue");

// route คงที่ (ไม่ขึ้นกับเมนู)
const routes: RouteRecordRaw[] = [
  { path: "/login", name: "login", component: LoginView },
  {
    path: "/print/:docType/:id",
    name: "print",
    component: () => import("../views/print/DocumentPrintView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/",
    name: "app-layout",
    component: AppLayout,
    meta: { requiresAuth: true },
    children: [{ path: "", name: "app-home", redirect: "/dashboard" }],
  },
  // ⬇️ เพิ่มตรงนี้ (ก่อนปิดวงเล็บเหลี่ยม ]; ของอาร์เรย์)
  {
    path: "/:pathMatch(.*)*",
    name: "not-found",
    component: { render: () => null },
    meta: { requiresAuth: true },
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// route ที่สร้างจากเมนู (เก็บชื่อไว้เพื่อลบตอนสลับผู้ใช้)
let dynamicRouteNames: string[] = [];

function registerMenuRoutes() {
  dynamicRouteNames.forEach((name) => {
    if (router.hasRoute(name)) router.removeRoute(name);
  });
  dynamicRouteNames = [];

  for (const page of useMenuStore().pages) {
    if (!page.path || !page.componentPath) continue;

    const loader = viewModules[`../${page.componentPath}`];
    if (!loader) {
      console.warn(
        `[menu] ไม่พบไฟล์ view ของเมนู "${page.code}": ${page.componentPath}`,
      );
      continue;
    }

    const record: RouteRecordRaw = {
      path: page.path,
      name: page.code,
      component: loader as () => Promise<RouteComponent>,
      meta: {
        menuCode: page.code,
        permissionCode: page.permissionCode,
        accessLevel: page.accessLevel,
      },
    };
    router.addRoute("app-layout", record);
    dynamicRouteNames.push(page.code);
  }
}
// ดึงเมนูใหม่เบื้องหลัง (ตอนที่เปิดหน้าจากแคชไปแล้ว)
function refreshMenusInBackground() {
  const menuStore = useMenuStore();
  menuStore
    .refresh()
    .then((changed) => {
      if (!changed) return;
      registerMenuRoutes();

      // ถ้าหน้าที่เปิดอยู่ไม่อยู่ในสิทธิ์แล้ว ให้ไปหน้าแรกที่เข้าได้
      const current = router.resolve(router.currentRoute.value.fullPath);
      if (current.name === "not-found") {
        const first = menuStore.pages.find((p) => p.path)?.path;
        router.replace(first ?? "/login");
      }
    })
    .catch((err) => {
      if (err?.response?.status === 401) {
        useAuthStore().logout();
        router.replace("/login");
      } else {
        console.warn("[menu] refresh ไม่สำเร็จ ใช้เมนูจากแคชต่อ", err);
      }
    });
}

router.beforeEach(async (to) => {
  const token = localStorage.getItem("token");

  if (to.path.toLowerCase() === "/login") {
    return token ? "/dashboard" : true;
  }

  if (!token) return "/login";

  // โหลดเมนูครั้งแรก (รวมกรณี refresh หน้า) แล้วเพิ่ม route จริง
  const menuStore = useMenuStore();
  if (!menuStore.loaded) {
    if (menuStore.hydrateFromCache()) {
      // มีแคช: ใช้ทันที แล้วอัปเดตเบื้องหลัง
      registerMenuRoutes();
      refreshMenusInBackground();
    } else {
      // ไม่มีแคช (login ครั้งแรก): ต้องรอจาก API
      try {
        await menuStore.load();
      } catch (err) {
        console.error("Failed to load menus:", err);
        useAuthStore().logout();
        return "/login";
      }
      registerMenuRoutes();
    }
    // วิ่งซ้ำไปที่ path เดิม เพื่อให้จับ route ที่เพิ่งเพิ่มได้
    return { path: to.path, query: to.query, hash: to.hash, replace: true };
  }

  // path ที่ไม่มีในเมนูของผู้ใช้ (ไม่มีสิทธิ์/ไม่มีหน้านี้) -> ไปหน้าแรกที่เข้าได้
  if (to.name === "not-found") {
    const first = menuStore.pages.find((p) => p.path)?.path;
    if (first) return first;
    useAuthStore().logout();
    return "/login";
  }

  return true;
});

export default router;
