<template>
  <aside :class="uiStore.sidebarCollapsed ? 'w-[72px]' : 'w-[260px]'"
    class="flex min-h-full shrink-0 flex-col border-r border-base-200 bg-base-100 transition-[width] duration-200">
    <!-- Logo -->
    <div class="flex h-16 shrink-0 items-center px-4"
      :class="uiStore.sidebarCollapsed ? 'justify-center' : 'gap-3'">
      <div class="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary text-sm font-bold text-primary-content">
        U
      </div>

      <div v-show="!uiStore.sidebarCollapsed"
        class="min-w-0 overflow-hidden whitespace-nowrap">
        <div class="text-[13px] font-bold leading-4 tracking-[-0.01em] text-base-content">
          UBIS CORE
        </div>

        <div class="mt-0.5 text-[9px] font-medium uppercase tracking-[0.08em] text-base-content/40">
          Business Platform
        </div>
      </div>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 overflow-y-auto px-2.5 py-4">
      <div v-for="group in visibleMenuGroups"
        :key="group.label ?? 'root'"
        class="mb-5"
      >
        <!-- Group title -->
        <div v-if="group.label"
          v-show="!uiStore.sidebarCollapsed"
          class="mb-2 px-3 text-[10px] font-semibold uppercase tracking-[0.08em] text-base-content/40">
          {{ group.label }}
        </div>
        <ul class="flex flex-col gap-0.5">
          <li v-for="item in group.items" :key="item.path">
            <!-- Parent -->
            <template v-if="item.children?.length">
              <button type="button"
                class="group relative flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left transition-colors duration-150"
                :class="
                  isParentActive(item)
                    ? 'bg-primary/10 text-primary'
                    : 'text-base-content/60 hover:bg-base-200/70 hover:text-base-content'
                "
                @click="toggleMenu(item.path)"
              >
                <!-- Active indicator -->
                <span v-if="isParentActive(item)"
                  class="absolute left-0 top-1/2 h-5 w-[3px] -translate-y-1/2 rounded-r-full bg-primary"></span>

                <!-- Icon -->
                <component :is="item.icon"
                  class="size-[18px] shrink-0"
                  :class="
                    isParentActive(item)
                      ? 'text-primary'
                      : 'text-base-content/45 group-hover:text-base-content/70'"/>

                <!-- Label -->
                <span v-show="!uiStore.sidebarCollapsed"
                  class="min-w-0 flex-1 truncate text-[13px] font-medium">
                  {{ item.label }}
                </span>

                <!-- Chevron -->
                <ChevronRight
                  v-show="!uiStore.sidebarCollapsed"
                  class="size-3.5 shrink-0 text-base-content/35 transition-transform duration-200"
                  :class="openMenus.has(item.path) ? 'rotate-90' : ''"
                />
              </button>

              <!-- Children -->
              <div v-show="!uiStore.sidebarCollapsed && openMenus.has(item.path)"
                class="ml-[17px] border-l border-base-200 pl-3">
                <div class="mt-1 flex flex-col gap-0.5">
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
            </template>

            <!-- Single item -->
            <RouterLink
              v-else
              :to="item.path"
              class="group relative flex items-center gap-3 rounded-lg px-3 py-2.5 transition-colors duration-150"
              :class="
                isExactActive(item.path)
                  ? 'bg-primary/10 text-primary'
                  : 'text-base-content/60 hover:bg-base-200/70 hover:text-base-content'
              "
            >
              <!-- Active indicator -->
              <span v-if="isExactActive(item.path)"
                class="absolute left-0 top-1/2 h-5 w-[3px] -translate-y-1/2 rounded-r-full bg-primary"></span>

              <!-- Icon -->
              <component
                :is="item.icon"
                class="size-[18px] shrink-0"
                :class="
                  isExactActive(item.path)
                    ? 'text-primary'
                    : 'text-base-content/45 group-hover:text-base-content/70'
                "
              />

              <!-- Label -->
              <span
                v-show="!uiStore.sidebarCollapsed"
                class="min-w-0 truncate text-[13px] font-medium"
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
        <span class="size-1.5 rounded-full bg-success"></span>

        <span class="text-[11px] font-medium text-base-content/55">
          System Online
        </span>

        <span class="ml-auto text-[9px] text-base-content/30"> v1.0 </span>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { useRoute } from "vue-router";
import { ChevronRight } from "lucide-vue-next";
import { useUiStore } from "../../stores/ui";
import { useAuthStore } from "../../stores/authStore";
import { menuGroups } from "../../config/menu";

const route = useRoute();
const uiStore = useUiStore();
const authStore = useAuthStore();

const openMenus = ref<Set<string>>(new Set());

const visibleMenuGroups = computed(() =>
  menuGroups
    .map((group) => ({
      ...group,
      items: group.items.filter(
        (item) => !item.permission || authStore.hasPermission(item.permission),
      ),
    }))
    .filter((group) => group.items.length > 0),
);

function toggleMenu(path: string) {
  const next = new Set(openMenus.value);

  if (next.has(path)) {
    next.delete(path);
  } else {
    next.add(path);
  }

  openMenus.value = next;
}

function isExactActive(path: string): boolean {
  return route.path === path || route.path.startsWith(path + "/");
}

function isParentActive(item: { children?: { path: string }[] }): boolean {
  return item.children?.some((c) => route.path.startsWith(c.path)) ?? false;
}
</script>
