<template>
  <div class="p-4 sm:p-6 space-y-5 mx-auto">
    <!-- Header -->
    <div>
      <h1 class="text-xl font-semibold text-base-content">บริษัท</h1>
      <p class="text-sm text-base-content/50 mt-0.5">{{ companies.length }} บริษัททั้งหมด</p>
    </div>

    <!-- Search -->
    <div class="relative max-w-sm">
      <Search class="absolute left-3.5 top-1/2 -translate-y-1/2 size-4 text-gray-400 pointer-events-none z-10" />
      <input
        v-model="search"
        type="text"
        placeholder="ค้นหาชื่อบริษัท, รหัสบริษัท..."
        class="input input-bordered w-full pl-10 text-sm focus:outline-none focus:border-primary transition-colors"
      />
    </div>

    <!-- Loading skeleton -->
    <div v-if="isLoading" class="grid grid-cols-[repeat(auto-fit,minmax(260px,1fr))] gap-4">
      <div v-for="i in 6" :key="i" class="skeleton h-32 w-full rounded-2xl"></div>
    </div>

    <!-- Error -->
    <div
      v-else-if="errorMessage"
      class="flex flex-col items-center gap-2 p-14 text-center bg-base-100 rounded-2xl border border-base-200"
    >
      <CircleAlert class="size-8 text-error/70" />
      <p class="text-error text-sm">{{ errorMessage }}</p>
    </div>

    <template v-else>
      <!-- Empty state -->
      <div
        v-if="filteredCompanies.length === 0"
        class="flex flex-col items-center gap-2 p-14 text-center bg-base-100 rounded-2xl border border-base-200"
      >
        <Building2 class="size-8 text-base-content/25" />
        <p class="text-base-content/50 text-sm">ไม่พบข้อมูลบริษัท</p>
      </div>

      <!-- Grouped cards -->
      <div v-for="group in groupedCompanies" :key="group.name" class="space-y-3">
        <div class="flex items-center gap-2 text-xs font-semibold text-base-content/45 uppercase tracking-wide">
          <Layers class="size-3.5" />
          เครือ {{ group.name }}
          <span class="badge badge-ghost badge-sm font-normal">{{ group.items.length }}</span>
        </div>

        <div class="grid grid-cols-[repeat(auto-fit,minmax(260px,1fr))] gap-4">
          <div
            v-for="company in group.items"
            :key="company.id"
            class="bg-base-100 rounded-2xl shadow-sm border border-base-200 p-5 space-y-3 hover:shadow-md transition-shadow"
          >
            <div class="flex items-start justify-between gap-2">
              <div class="size-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center shrink-0 font-semibold text-sm">
                {{ company.code }}
              </div>
              <span
                class="badge badge-sm font-normal whitespace-nowrap"
                :class="company.isActive ? 'badge-success text-success-content' : 'badge-ghost'"
              >
                {{ company.isActive ? 'ใช้งานอยู่' : 'ปิดใช้งาน' }}
              </span>
            </div>

            <div>
              <h3 class="font-semibold text-base-content leading-snug">{{ company.nameTh }}</h3>
              <p class="text-xs text-base-content/50 mt-0.5">{{ company.nameEn }}</p>
            </div>

            <!-- Branch accordion -->
            <!-- Branch accordion -->
<div class="pt-3 border-t border-base-200">
  <button
    type="button"
    class="w-full flex items-center justify-between text-xs font-medium text-base-content/70 hover:text-primary rounded-lg px-2 py-1.5 -mx-2 hover:bg-primary/5 transition-colors"
    @click="toggleBranches(company.id)"
  >
    <span class="flex items-center gap-1.5">
      <MapPin class="size-3.5" />
      {{ branchesOf(company.id).length }} สาขา
    </span>
    <ChevronDown
      class="size-3.5 transition-transform duration-200"
      :class="expandedIds.has(company.id) && 'rotate-180'"
    />
  </button>

  <Transition
    enter-active-class="transition-all duration-200 ease-out"
    enter-from-class="opacity-0 -translate-y-1 grid-rows-[0fr]"
    enter-to-class="opacity-100 translate-y-0 grid-rows-[1fr]"
    leave-active-class="transition-all duration-150 ease-in"
    leave-from-class="opacity-100 grid-rows-[1fr]"
    leave-to-class="opacity-0 grid-rows-[0fr]"
  >
    <div v-if="expandedIds.has(company.id)" class="grid overflow-hidden">
      <div class="min-h-0 pt-2.5 flex flex-wrap gap-1.5">
        <div
          v-for="branch in branchesOf(company.id)"
          :key="branch.id"
          class="inline-flex items-center gap-1.5 pl-2 pr-2.5 py-1 rounded-full text-xs border"
          :class="branch.isActive
            ? 'bg-base-200/60 border-base-200 text-base-content/75'
            : 'bg-base-200/30 border-base-200 text-base-content/35'"
        >
          <component
            :is="branch.nameEn === 'Headquarters' ? Star : Factory"
            class="size-3 shrink-0"
            :class="branch.isActive ? 'text-primary/70' : 'text-base-content/30'"
          />
          <span class="font-medium">{{ branch.nameTh }}</span>
          <span class="text-[10px] opacity-60">{{ branch.code }}</span>
        </div>

        <div v-if="branchesOf(company.id).length === 0" class="text-xs text-base-content/35 py-1">
          ยังไม่มีสาขา
        </div>
      </div>
    </div>
  </Transition>
</div>

            <div class="flex items-center justify-between text-xs text-base-content/40">
              <span>รหัส {{ company.code }}</span>
              <span>อัปเดต {{ formatDate(company.updatedAt) }}</span>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useCompanyStore } from '../../stores/companyStore'
import { Search, Building2, CircleAlert, Layers, MapPin, ChevronDown, Star, Factory } from 'lucide-vue-next'
import type { Company } from '../../types/Company'

const companyStore = useCompanyStore()
const { companies, isLoading, errorMessage } = storeToRefs(companyStore)

const search = ref('')
const expandedIds = ref<Set<string>>(new Set())

function toggleBranches(companyId: string) {
  const next = new Set(expandedIds.value)
  if (next.has(companyId)) next.delete(companyId)
  else next.add(companyId)
  expandedIds.value = next
}

function branchesOf(companyId: string) {
  return companyStore.branchesByCompanyId.get(companyId) ?? []
}

const filteredCompanies = computed(() => {
  const q = search.value.trim().toLowerCase()
  if (!q) return companies.value
  return companies.value.filter(
    (c) =>
      c.nameTh.toLowerCase().includes(q) ||
      c.nameEn.toLowerCase().includes(q) ||
      c.code.toLowerCase().includes(q)
  )
})

const groupedCompanies = computed(() => {
  const groups = new Map<string, Company[]>()
  for (const c of filteredCompanies.value) {
    const key = c.groupName || 'ไม่ระบุกลุ่ม'
    if (!groups.has(key)) groups.set(key, [])
    groups.get(key)!.push(c)
  }
  return Array.from(groups.entries()).map(([name, items]) => ({ name, items }))
})

function formatDate(dateStr: string) {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleDateString('th-TH', { year: 'numeric', month: 'short', day: 'numeric' })
}

onMounted(() => companyStore.fetchAll())
</script>