<template>
  <div class="flex flex-col items-center">
    <div class="relative rounded-xl border shadow-sm px-4 py-3 text-center w-full md:w-auto transition-transform hover:-translate-y-0.5"
      :class="[
        node.isSelf
          ? (variant === 'modal' ? 'border-2 border-primary bg-primary/5' : 'border-2 border-emerald-500 bg-emerald-50')
          : (variant === 'modal' ? 'border-base-200 bg-base-100' : 'border-slate-200 bg-white')
      ]"
    >
      <div class="mx-auto mb-1.5 flex h-9 w-9 items-center justify-center rounded-full text-sm font-semibold"
        :class="node.isSelf
          ? (variant === 'modal' ? 'bg-primary text-primary-content' : 'bg-emerald-600 text-white')
          : (variant === 'modal' ? 'bg-base-200 text-base-content/60' : 'bg-slate-100 text-slate-500')"
      >
        {{ node.fullNameTh?.charAt(0) }}
      </div>
      <p class="text-sm font-semibold break-words leading-snug"
        :class="node.isSelf
          ? (variant === 'modal' ? 'text-primary' : 'text-emerald-700')
          : (variant === 'modal' ? 'text-base-content/80' : 'text-slate-700')">
        {{ node.fullNameTh }}
      </p>
      <p class="text-xs break-words leading-snug mt-0.5"
        :class="variant === 'modal' ? 'text-base-content/40' : 'text-slate-400'"
      >
        {{ node.positionNameTh || '-' }}
      </p>
    </div>

    <template v-if="node.children && node.children.length > 0">
      <div class="w-px h-5" :class="variant === 'modal' ? 'bg-base-300' : 'bg-slate-300'"></div>

      <div class="relative flex gap-6">
        <div v-if="node.children.length > 1"
          class="absolute top-0 h-px"
          :class="variant === 'modal' ? 'bg-base-300' : 'bg-slate-300'"
          style="left: 6rem; right: 6rem;"
        ></div>

        <div v-for="child in node.children" :key="child.id" class="flex flex-col items-center">
          <div class="w-px h-5" :class="variant === 'modal' ? 'bg-base-300' : 'bg-slate-300'"></div>
          <OrgChartNode :node="child" :variant="variant" />
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { EmployeeOrgChartNode } from '../../types/Employee'

defineOptions({ name: 'OrgChartNode' })

defineProps<{
  node: EmployeeOrgChartNode
  variant?: 'modal' | 'dashboard'
}>()
</script>