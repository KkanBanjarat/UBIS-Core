import { defineStore } from "pinia";
import { jwtDecode } from "jwt-decode";
import accessApi from "../services/accessApi";
import type { MenuNode } from "../types/Menu";

const CACHE_KEY = "menuCache";
const CACHE_VERSION = 1;

let loadingPromise: Promise<void> | null = null;

function flattenPages(nodes: MenuNode[]): MenuNode[] {
  return nodes.flatMap((n) =>
    n.nodeType === "Page" ? [n] : flattenPages(n.children),
  );
}

function currentUserId(): string | null {
  try {
    const token = localStorage.getItem("token");
    return token ? jwtDecode<{ sub: string }>(token).sub : null;
  } catch {
    return null;
  }
}

function readCache(): MenuNode[] | null {
  try {
    const raw = localStorage.getItem(CACHE_KEY);
    if (!raw) return null;
    const c = JSON.parse(raw);
    if (
      c.v !== CACHE_VERSION ||
      c.userId !== currentUserId() ||
      !Array.isArray(c.tree)
    )
      return null;
    return c.tree as MenuNode[];
  } catch {
    return null;
  }
}

function writeCache(tree: MenuNode[]) {
  try {
    const userId = currentUserId();
    if (!userId) return;
    localStorage.setItem(
      CACHE_KEY,
      JSON.stringify({ v: CACHE_VERSION, userId, tree }),
    );
  } catch {
    // เขียนแคชไม่ได้ก็ไม่เป็นไร ระบบยังทำงานต่อได้
  }
}

async function fetchTree(): Promise<MenuNode[]> {
  const res = await accessApi.get<MenuNode[]>("/me/menus");
  return res.data;
}

export const useMenuStore = defineStore("menu", {
  state: () => ({
    tree: [] as MenuNode[],
    loaded: false,
    errorMessage: "",
  }),
  getters: {
    pages: (state): MenuNode[] => flattenPages(state.tree),

    // ระดับสิทธิ์ของหน้า (0 = เข้าไม่ได้) ค้นจาก PermissionCode ของเมนู เช่น "benefit-claim"
    levelOf(): (code: string) => number {
      return (code) =>
        this.pages.find((p) => p.permissionCode === code)?.accessLevel ?? 0;
    },
  },
  actions: {
    /** ใช้เมนูจากแคช (ถ้ามีและเป็นของผู้ใช้คนนี้) คืน true ถ้าใช้ได้ */
    hydrateFromCache(): boolean {
      const cached = readCache();
      if (!cached) return false;
      this.tree = cached;
      this.loaded = true;
      return true;
    },

    /** โหลดจาก API แล้วรอ (ใช้ตอนไม่มีแคช) */
    async load(force = false) {
      if (this.loaded && !force) return;
      if (loadingPromise) return loadingPromise;

      loadingPromise = (async () => {
        try {
          this.errorMessage = "";
          const tree = await fetchTree();
          this.tree = tree;
          this.loaded = true;
          writeCache(tree);
        } catch (err) {
          this.loaded = false;
          this.errorMessage = "โหลดเมนูไม่สำเร็จ";
          throw err;
        } finally {
          loadingPromise = null;
        }
      })();

      return loadingPromise;
    },

    /** ดึงของใหม่เบื้องหลัง คืน true ถ้าเมนูเปลี่ยนจากที่ถืออยู่ */
    async refresh(): Promise<boolean> {
      const fresh = await fetchTree();
      const changed = JSON.stringify(fresh) !== JSON.stringify(this.tree);
      this.tree = fresh;
      this.loaded = true;
      writeCache(fresh);
      return changed;
    },

    reset() {
      this.tree = [];
      this.loaded = false;
      this.errorMessage = "";
      try {
        localStorage.removeItem(CACHE_KEY);
      } catch {
        // ไม่เป็นไร
      }
    },
  },
});
