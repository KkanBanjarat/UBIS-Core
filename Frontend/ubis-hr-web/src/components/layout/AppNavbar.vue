<template>
  <header class="sticky top-0 z-30 px-3 pt-3 sm:px-5">
    <div
      class="flex h-14 items-center gap-2 rounded-2xl bg-base-100 px-2 shadow-[0_4px_20px_rgba(0,0,0,0.06)]"
    >
      <!-- Mobile menu -->
      <label
        for="app-drawer"
        class="flex size-9 cursor-pointer items-center justify-center rounded-xl text-base-content/60 transition-colors hover:bg-base-200 hover:text-base-content lg:hidden"
      >
        <Menu class="size-5" />
      </label>

      <!-- Desktop sidebar toggle -->
      <button
        type="button"
        class="hidden size-9 items-center justify-center rounded-xl text-base-content/60 transition-colors hover:bg-base-200 hover:text-base-content lg:flex"
        @click="uiStore.toggleSidebar()"
      >
        <component
          :is="uiStore.sidebarCollapsed ? PanelLeftOpen : PanelLeftClose"
          class="size-[18px]"
        />
      </button>

      <!-- Page title -->
      <div class="ml-1 min-w-0 sm:ml-2">
        <div
          class="truncate text-[13px] font-semibold leading-5 text-base-content"
        >
          {{ currentTitle }}
        </div>

        <div
          class="hidden text-[9px] font-medium uppercase tracking-[0.1em] text-base-content/35 sm:block"
        >
          UBIS CORE
        </div>
      </div>

      <div class="ml-auto flex items-center gap-1">
        <!-- Notification -->
        <button
          type="button"
          class="relative flex size-9 items-center justify-center rounded-xl text-base-content/55 transition-colors hover:bg-base-200 hover:text-base-content"
        >
          <Bell class="size-[18px]" />

          <span
            class="absolute right-[7px] top-[7px] size-1.5 rounded-full bg-primary ring-2 ring-base-100"
          ></span>
        </button>

        <!-- User -->
        <div class="dropdown dropdown-end">
          <div
            tabindex="0"
            role="button"
            class="flex cursor-pointer items-center gap-2 rounded-xl px-2 py-1 transition-colors hover:bg-base-200"
          >
            <!-- Avatar -->
            <div
              class="flex size-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary"
            >
              <span class="text-xs font-bold">
                {{ authStore.displayName?.charAt(0) ?? "U" }}
              </span>
            </div>

            <!-- User info -->
            <div class="hidden min-w-0 text-left sm:block">
              <div
                class="max-w-40 truncate text-xs font-semibold text-base-content"
              >
                {{ authStore.displayName }}
              </div>

              <div class="text-[9px] text-base-content/40">
                UBIS Core
              </div>
            </div>

            <ChevronDown
              class="hidden size-3.5 text-base-content/35 sm:block"
            />
          </div>

          <!-- Dropdown -->
          <ul
            tabindex="0"
            class="dropdown-content menu z-40 mt-2 w-52 rounded-xl border border-base-200 bg-base-100 p-1.5 shadow-[0_15px_40px_rgba(0,0,0,0.10)]"
          >
            <li>
              <a
                class="rounded-lg px-3 py-2.5 text-sm text-error hover:bg-error/10"
                @click="handleLogout"
              >
                <LogOut class="size-4" />
                <span>ออกจากระบบ</span>
              </a>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  Menu,
  PanelLeftClose,
  PanelLeftOpen,
  Bell,
  LogOut,
  ChevronDown,
} from 'lucide-vue-next'

import { useAuthStore } from '../../stores/authStore'
import { useUiStore } from '../../stores/ui'
import { menuGroups } from '../../config/menu'

const router = useRouter()
const route = useRoute()

const authStore = useAuthStore()
const uiStore = useUiStore()

function handleLogout() {
  authStore.logout()
  router.push('/login')
}

const currentTitle = computed(() => {
  for (const group of menuGroups) {
    for (const item of group.items) {
      if (
        route.path === item.path ||
        route.path.startsWith(item.path + '/')
      ) {
        const child = item.children?.find((c) =>
          route.path.startsWith(c.path),
        )

        return child ? child.label : item.label
      }
    }
  }

  return ''
})
</script>