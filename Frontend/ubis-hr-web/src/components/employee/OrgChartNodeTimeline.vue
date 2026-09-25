```vue
<template>
  <div class="flex gap-3">
    <!-- Timeline -->
    <div class="flex w-6 shrink-0 flex-col items-center">
      <!-- Node -->
      <div
        class="relative z-10 mt-1 flex size-5 shrink-0 items-center justify-center rounded-full border-2 transition-all duration-200"
        :class="
          node.isSelf
            ? variant === 'modal'
              ? 'border-primary bg-primary shadow-sm shadow-primary/30'
              : 'border-emerald-500 bg-emerald-500 shadow-sm shadow-emerald-200'
            : variant === 'modal'
              ? 'border-base-300 bg-base-100'
              : 'border-slate-300 bg-white'
        "
      >
        <span
          class="size-1.5 rounded-full"
          :class="
            node.isSelf
              ? 'bg-white'
              : variant === 'modal'
                ? 'bg-base-content/30'
                : 'bg-slate-300'
          "
        ></span>
      </div>

      <!-- Connector -->
      <span
        v-if="node.children?.length"
        class="mt-1 w-px flex-1"
        :class="
          variant === 'modal'
            ? node.isSelf
              ? 'bg-primary/20'
              : 'bg-base-300'
            : node.isSelf
              ? 'bg-emerald-200'
              : 'bg-slate-200'
        "
      ></span>
    </div>

    <!-- Content -->
    <div class="min-w-0 flex-1 pb-4">
      <!-- Employee Card -->
      <div
        class="group relative rounded-xl border px-3.5 py-3 transition-all duration-200"
        :class="
          node.isSelf
            ? variant === 'modal'
              ? 'border-primary/25 bg-primary/[0.045] shadow-sm shadow-primary/5'
              : 'border-emerald-200 bg-emerald-50/50 shadow-sm shadow-emerald-100'
            : variant === 'modal'
              ? 'border-base-200 bg-base-100 hover:border-base-300 hover:shadow-sm'
              : 'border-slate-200 bg-white hover:border-slate-300 hover:shadow-sm'
        "
      >
        <!-- Self indicator -->
        <div
          v-if="node.isSelf"
          class="absolute right-3 top-3"
        >
          <span
            class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-semibold"
            :class="
              variant === 'modal'
                ? 'bg-primary/10 text-primary'
                : 'bg-emerald-100 text-emerald-700'
            "
          >
            <span
              class="size-1.5 rounded-full"
              :class="variant === 'modal' ? 'bg-primary' : 'bg-emerald-500'"
            ></span>
            คุณ
          </span>
        </div>

        <div class="flex items-center gap-3">
          <!-- Avatar -->
          <div
            class="flex size-9 shrink-0 items-center justify-center rounded-full text-xs font-semibold ring-2"
            :class="
              node.isSelf
                ? variant === 'modal'
                  ? 'bg-primary text-primary-content ring-primary/10'
                  : 'bg-emerald-100 text-emerald-700 ring-emerald-100'
                : variant === 'modal'
                  ? 'bg-base-200 text-base-content/60 ring-base-100'
                  : 'bg-slate-100 text-slate-500 ring-white'
            "
          >
            {{ getInitials(node.fullNameTh) }}
          </div>

          <!-- Info -->
          <div class="min-w-0 flex-1 pr-8">
            <p
              class="truncate text-sm"
              :class="
                node.isSelf
                  ? variant === 'modal'
                    ? 'font-semibold text-primary'
                    : 'font-semibold text-emerald-700'
                  : variant === 'modal'
                    ? 'font-medium text-base-content'
                    : 'font-medium text-slate-700'
              "
            >
              {{ node.fullNameTh }}
            </p>

            <p
              class="mt-0.5 truncate text-xs"
              :class="
                variant === 'modal'
                  ? 'text-base-content/45'
                  : 'text-slate-400'
              "
            >
              {{ node.positionNameTh || '-' }}
            </p>
          </div>
        </div>

        <!-- Self description -->
        <div
          v-if="node.isSelf"
          class="mt-2.5 flex items-center gap-1.5 border-t pt-2"
          :class="
            variant === 'modal'
              ? 'border-primary/10 text-primary/60'
              : 'border-emerald-100 text-emerald-600/70'
          "
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.8"
            class="size-3.5"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M15 10.5a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M19.5 10.5c0 7-7.5 10.5-7.5 10.5S4.5 17.5 4.5 10.5a7.5 7.5 0 1 1 15 0Z"
            />
          </svg>

          <span class="text-[10px] font-medium">
            ตำแหน่งปัจจุบัน
          </span>
        </div>
      </div>

      <!-- Children -->
      <div
        v-if="node.children?.length"
        class="mt-3 space-y-0"
      >
        <OrgChartNode
          v-for="child in node.children"
          :key="child.id"
          :node="child"
          :variant="variant"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { EmployeeOrgChartNode } from '../../types/Employee'

defineOptions({ name: 'OrgChartNode' })

defineProps<{
  node: EmployeeOrgChartNode
  variant?: 'modal' | 'dashboard'
}>()

function getInitials(name?: string | null) {
  if (!name) return '?'

  const parts = name.trim().split(/\s+/)

  if (parts.length >= 2) {
    return `${parts[0].charAt(0)}${parts[1].charAt(0)}`
  }

  return name.slice(0, 2)
}
</script>
```
