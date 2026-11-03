<template>
  <aside :class="uiStore.sidebarCollapsed ? 'w-[72px]' : 'w-[260px]'"
    class="sidebar-shell flex min-h-full shrink-0 flex-col border-r border-base-200 bg-base-100 transition-[width] duration-300 ease-[cubic-bezier(.2,.8,.2,1)]">
    <!-- Logo -->
    <div class="brand-header relative flex h-[88px] shrink-0 items-center px-3.5"
      :class="uiStore.sidebarCollapsed ? 'justify-center' : 'gap-3'">
      <div class="brand-mark flex h-12 w-[72px] shrink-0 items-center justify-center rounded-2xl border border-base-200/80 bg-white px-2.5 shadow-sm">
        <img :src="ubisLogo" alt="UBIS" class="h-auto w-full object-contain" />
      </div>

      <div class="sidebar-copy brand-copy min-w-0 overflow-hidden whitespace-nowrap"
        :class="uiStore.sidebarCollapsed ? 'is-hidden' : 'is-visible'">
        <div class="flex items-center gap-1.5">
          <span class="text-[13px] font-extrabold leading-4 tracking-[-0.03em] text-base-content">HR WORKSPACE</span>
          <!-- <span class="rounded-md bg-primary/10 px-1.5 py-0.5 text-[9px] font-bold leading-none tracking-[0.08em] text-primary">POD</span> -->
        </div>
        <div class="mt-1.5 space-y-0.5 text-[8px] font-semibold uppercase leading-[1.2] tracking-[0.065em] text-base-content/50">
          <div>People &amp; Organization</div>
          <div>Development</div>
        </div>
      </div>
      <div v-if="!uiStore.sidebarCollapsed" class="brand-accent" aria-hidden="true"></div>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 overflow-y-auto px-2.5 py-4">
      <div v-for="group in navGroups"
        :key="group.label ?? 'root'"
      class="mb-5"
      >
        <!-- Group title -->
        <div v-if="group.label"
          class="sidebar-copy mb-2 px-3 text-[10px] font-semibold uppercase tracking-[0.12em] text-base-content/40"
          :class="uiStore.sidebarCollapsed ? 'is-hidden' : 'is-visible'">
          {{ group.label }}
        </div>
        <ul class="flex flex-col gap-0.5">
          <li v-for="item in group.items" :key="item.key">
            <!-- Parent -->
            <template v-if="item.children?.length">
              <button type="button"
                class="menu-row group relative flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left transition-all duration-200"
                :class="[
                  uiStore.sidebarCollapsed ? 'justify-center gap-0 px-0' : '',
                  isParentActive(item)
                    ? 'bg-primary/10 text-primary shadow-sm shadow-primary/10 ring-1 ring-inset ring-primary/10'
                    : 'text-base-content/60 hover:bg-base-200/70 hover:text-base-content'
                ]"
                @click="toggleMenu(item.key)"
                @mouseenter="uiStore.sidebarCollapsed && showTooltip(item, $event)"
                @mouseleave="scheduleHideTooltip"
              >
                <!-- Active indicator -->
                <span v-if="isParentActive(item)"
                  class="absolute left-0 top-1/2 h-5 w-[3px] -translate-y-1/2 rounded-r-full bg-primary"></span>

                <!-- Icon -->
                <component :is="item.icon"
                  class="menu-icon size-[18px] shrink-0 transition-transform duration-200 group-hover:scale-110"
                  :class="
                    isParentActive(item)
                      ? 'text-primary'
                      : 'text-base-content/45 group-hover:text-base-content/70'"/>

                <!-- Label -->
                <span class="sidebar-copy min-w-0 flex-1 truncate text-[13px] font-medium"
                  :class="uiStore.sidebarCollapsed ? 'is-hidden' : 'is-visible'">
                  {{ item.label }}
                </span>

                <!-- Chevron -->
                <ChevronRight
                  class="sidebar-copy size-3.5 shrink-0 text-base-content/35 transition-transform duration-200"
                  :class="[uiStore.sidebarCollapsed ? 'is-hidden' : 'is-visible', openMenus.has(item.key) ? 'rotate-90' : '']"
                />
              </button>

              <!-- Children -->
              <div class="children-wrap" :class="{ 'children-open': !uiStore.sidebarCollapsed && openMenus.has(item.key) }">
                <div class="children-inner ml-[17px] border-l border-base-200 pl-3">
                  <div class="mt-1 flex flex-col gap-0.5 pb-1">
                  <RouterLink v-for="child in item.children"
                    :key="child.path"
                    :to="child.path"
                    class="group relative flex items-center rounded-md px-3 py-2 text-[12.5px] transition-colors duration-150"
                    :class="
                      isExactActive(child.path)
                        ? 'bg-primary/10 font-medium text-primary'
                        : 'text-base-content/55 hover:bg-base-200/70 hover:text-base-content'
                    ">
                    <!-- Child active dot -->
                    <span v-if="isExactActive(child.path)" 
                      class="absolute -left-[17px] size-1.5 rounded-full bg-primary ring-[3px] ring-base-100"></span>
                    {{ child.label }}
                  </RouterLink>
                  </div>
                </div>
              </div>
            </template>

            <!-- Single item -->
            <RouterLink
              v-else
              :to="item.path"
              @mouseenter="uiStore.sidebarCollapsed && showTooltip(item, $event)"
              @mouseleave="scheduleHideTooltip"
              class="menu-row group relative flex items-center gap-3 rounded-xl px-3 py-2.5 transition-all duration-200"
              :class="[
                uiStore.sidebarCollapsed ? 'justify-center gap-0 px-0' : '',
                isExactActive(item.path)
                  ? 'bg-primary/10 text-primary shadow-sm shadow-primary/10 ring-1 ring-inset ring-primary/10'
                  : 'text-base-content/60 hover:bg-base-200/70 hover:text-base-content'
              ]"
            >
              <!-- Active indicator -->
              <span v-if="isExactActive(item.path)"
                class="absolute left-0 top-1/2 h-5 w-[3px] -translate-y-1/2 rounded-r-full bg-primary"></span>

              <!-- Icon -->
              <component
                :is="item.icon"
                class="menu-icon size-[18px] shrink-0 transition-transform duration-200 group-hover:scale-110"
                :class="
                  isExactActive(item.path)
                    ? 'text-primary'
                    : 'text-base-content/45 group-hover:text-base-content/70'
                "
              />

              <!-- Label -->
              <span class="sidebar-copy min-w-0 truncate text-[13px] font-medium"
                :class="uiStore.sidebarCollapsed ? 'is-hidden' : 'is-visible'"
              >
                {{ item.label }}
              </span>
            </RouterLink>
          </li>
        </ul>
      </div>
    </nav>

    <!-- Bottom status -->
    <div v-show="!uiStore.sidebarCollapsed"
      class="border-t border-base-200 p-3"
    >
      <div class="flex items-center gap-2 px-2 py-2">
        <span class="status-pulse size-2 rounded-full bg-success"></span>

        <span class="text-[11px] font-medium text-base-content/55">
          System Online
        </span>

        <span class="ml-auto text-[9px] text-base-content/30"> v1.0 </span>
      </div>
    </div>
  </aside>
  <Teleport to="body">
    <div v-if="uiStore.sidebarCollapsed && hoveredItem" class="sidebar-tooltip border border-base-300 bg-base-100 text-base-content"
      @mouseenter="cancelHideTooltip" @mouseleave="hideTooltip"
      :style="{ top: `${tooltipPosition.top}px`, left: `${tooltipPosition.left}px` }">
      <div class="text-[13px] font-semibold">{{ hoveredItem.label }}</div>
      <div v-if="hoveredItem.children?.length" class="mt-2 border-t border-base-content/10 pt-2">
        <RouterLink v-for="child in hoveredItem.children" :key="child.path" :to="child.path"
          class="block rounded-md py-1.5 text-xs text-base-content/65 transition-colors hover:bg-base-200 hover:text-primary">
          {{ child.label }}
        </RouterLink>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, type Component } from "vue";
import { useRoute } from "vue-router";
import { ChevronRight } from "lucide-vue-next";
import ubisLogo from "../../assets/img/LOGO_UBIS.png";
import { useUiStore } from "../../stores/ui";
import { useMenuStore } from "../../stores/menuStore";
import { resolveIcon } from "../../utils/menuIcons";
import type { MenuNode } from "../../types/Menu";

interface NavItem {
  key: string;
  path: string;
  label: string;
  icon: Component;
  children?: { path: string; label: string }[];
}
interface NavGroup {
  label: string | null;
  items: NavItem[];
}

const route = useRoute();
const uiStore = useUiStore();
const menuStore = useMenuStore();

const openMenus = ref<Set<string>>(new Set());
const hoveredItem = ref<NavItem | null>(null);
const tooltipPosition = ref({ top: 0, left: 0 });
let tooltipHideTimer: ReturnType<typeof setTimeout> | undefined;

function showTooltip(item: NavItem, event: MouseEvent) {
  const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
  cancelHideTooltip();
  hoveredItem.value = item;
  tooltipPosition.value = { top: rect.top, left: rect.right + 10 };
}

function scheduleHideTooltip() {
  tooltipHideTimer = setTimeout(hideTooltip, 180);
}

function cancelHideTooltip() {
  if (tooltipHideTimer) clearTimeout(tooltipHideTimer);
  tooltipHideTimer = undefined;
}

function hideTooltip() {
  hoveredItem.value = null;
}

function toItem(n: MenuNode): NavItem {
  if (n.nodeType === "Module") {
    return {
      key: n.code,
      path: "",
      label: n.label,
      icon: resolveIcon(n.icon),
      children: n.children
        .filter((c) => c.nodeType === "Page" && c.path)
        .map((c) => ({ path: c.path!, label: c.label })),
    };
  }
  return {
    key: n.code,
    path: n.path ?? "",
    label: n.label,
    icon: resolveIcon(n.icon),
  };
}

const navGroups = computed<NavGroup[]>(() => {
  const roots = menuStore.tree;
  const groups: NavGroup[] = [];

  // หน้า/โมดูลที่ไม่มีหมวด -> กลุ่มแรก (ไม่มีหัวข้อ)
  const topItems = roots.filter((n) => n.nodeType !== "Category").map(toItem);
  if (topItems.length) groups.push({ label: null, items: topItems });

  // แต่ละ Category เป็นกลุ่มที่มีหัวข้อ
  for (const cat of roots.filter((n) => n.nodeType === "Category")) {
    const items = cat.children.filter((c) => c.nodeType !== "Category").map(toItem);
    if (items.length) groups.push({ label: cat.label, items });
  }

  return groups;
});

function toggleMenu(key: string) {
  const next = new Set(openMenus.value);
  if (next.has(key)) next.delete(key);
  else next.add(key);
  openMenus.value = next;
}

function isExactActive(path: string): boolean {
  return route.path === path || route.path.startsWith(path + "/");
}

function isParentActive(item: { children?: { path: string }[] }): boolean {
  return item.children?.some((c) => route.path.startsWith(c.path)) ?? false;
}
</script>

<style scoped>
.brand-accent {
  position: absolute;
  right: 14px;
  bottom: 0;
  left: 14px;
  height: 2px;
  border-radius: 999px;
  background: linear-gradient(90deg, transparent, color-mix(in srgb, var(--color-primary) 40%, transparent), transparent);
}

.brand-mark {
  transition: transform 220ms ease, box-shadow 220ms ease;
}

.brand-mark:hover {
  transform: translateY(-1px);
  box-shadow: 0 5px 14px rgb(15 23 42 / 10%);
}

.children-wrap {
  display: grid;
  grid-template-rows: 0fr;
  opacity: 0;
  transition: grid-template-rows 240ms ease, opacity 180ms ease;
}

.sidebar-copy {
  transition: opacity 180ms ease, transform 220ms ease, max-width 260ms ease;
}

.sidebar-copy.is-visible {
  max-width: 220px;
  opacity: 1;
  transform: translateX(0);
  transition-delay: 70ms;
}

.sidebar-copy.is-hidden {
  max-width: 0;
  opacity: 0;
  transform: translateX(-6px);
  pointer-events: none;
}

.menu-row {
  transition: background-color 180ms ease, color 180ms ease, box-shadow 180ms ease, padding 260ms ease;
}

.children-wrap.children-open {
  grid-template-rows: 1fr;
  opacity: 1;
}

.children-inner {
  min-height: 0;
  overflow: hidden;
}

.status-pulse {
  position: relative;
  box-shadow: 0 0 0 0 color-mix(in srgb, currentColor 35%, transparent);
  animation: status-pulse 2s ease-out infinite;
}

@keyframes status-pulse {
  0% { box-shadow: 0 0 0 0 rgb(34 197 94 / 35%); }
  70%, 100% { box-shadow: 0 0 0 7px rgb(34 197 94 / 0%); }
}

.sidebar-tooltip {
  position: fixed;
  z-index: 100;
  width: max-content;
  min-width: 150px;
  max-width: 240px;
  border-radius: 0.85rem;
  padding: 0.75rem 0.9rem;
  box-shadow: 0 12px 32px rgb(15 23 42 / 18%);
  animation: tooltip-in 140ms ease-out both;
  pointer-events: auto;
}

@keyframes tooltip-in {
  from { opacity: 0; transform: translateX(-4px) scale(0.98); }
  to { opacity: 1; transform: translateX(0) scale(1); }
}

@media (prefers-reduced-motion: reduce) {
  .children-wrap, .sidebar-copy, .menu-row, .status-pulse, .sidebar-tooltip { animation: none; transition: none; }
}
</style>
