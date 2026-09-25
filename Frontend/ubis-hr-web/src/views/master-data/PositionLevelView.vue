<template>
  <div class="p-4 sm:p-6 space-y-5 mx-auto">
    <PageHeader title="ระดับตำแหน่ง" description="จัดการระดับตำแหน่งงาน (Job Level) L1 - L14">
      <template #actions>
        <button class="btn btn-primary btn-sm gap-1.5" @click="openCreateModal">
          <Plus class="size-4" />
          เพิ่มระดับตำแหน่ง
        </button>
      </template>
    </PageHeader>

    <div class="bg-base-100 rounded-2xl shadow-sm border border-base-200 overflow-hidden">
      <!-- Toolbar: Search + PageSize -->
      <div class="flex flex-col sm:flex-row gap-3 sm:items-center justify-between px-5 py-4 border-b border-base-200">
        <div class="relative flex-1 min-w-[240px] max-w-sm">
          <Search class="absolute left-3.5 top-1/2 -translate-y-1/2 size-4 text-gray-400 pointer-events-none z-10" />
          <input
            v-model="filter.search"
            @input="onSearchInput"
            type="text"
            placeholder="ค้นหารหัส, ชื่อระดับตำแหน่ง..."
            class="input input-bordered w-full pl-10 text-sm focus:outline-none focus:border-primary transition-colors"
          />
        </div>
        <select v-model.number="filter.pageSize" class="select select-bordered select-sm w-24">
          <option :value="10">10</option>
          <option :value="20">20</option>
          <option :value="50">50</option>
        </select>
      </div>

      <!-- Loading skeleton -->
      <div v-if="isLoading" class="p-5 space-y-2">
        <div v-for="i in 5" :key="i" class="skeleton h-12 w-full rounded-lg"></div>
      </div>

      <!-- Error -->
      <div v-else-if="errorMessage" class="flex flex-col items-center gap-2 p-14 text-center">
        <CircleAlert class="size-8 text-error/70" />
        <p class="text-error text-sm">{{ errorMessage }}</p>
      </div>

      <template v-else>
        <!-- Empty state -->
        <div v-if="positionLevels.length === 0" class="flex flex-col items-center gap-2 p-14 text-center">
          <LayoutList class="size-8 text-base-content/25" />
          <p class="text-base-content/50 text-sm">ไม่พบข้อมูลระดับตำแหน่ง</p>
        </div>

        <template v-else>
          <div class="overflow-x-auto">
            <table class="table min-w-[720px]">
              <thead>
                <tr class="text-sm uppercase tracking-wide text-base-content/45 border-b border-base-200">
                  <th class="bg-base-100">รหัส</th>
                  <th class="bg-base-100">Level</th>
                  <th class="bg-base-100">Track</th>
                  <th class="bg-base-100">ชื่อระดับตำแหน่ง</th>
                  <th class="bg-base-100">ขอบเขตการใช้งาน</th>
                  <th class="bg-base-100">สถานะ</th>
                  <th class="bg-base-100 text-right">จัดการ</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in positionLevels"
                  :key="item.id"
                  class="hover:bg-base-200/40 transition-colors border-b border-base-200/60 last:border-0">
                  <td>
                    <div class="badge badge-soft badge-info badge-sm font-semibold">{{ item.code }}</div>
                  </td>
                  <td class="font-medium text-base-content/80">L{{ item.level }}</td>
                  <td class="text-base-content/70">{{ item.track || '-' }}</td>
                  <td>
                    <div class="font-semibold text-base-content/80">{{ item.nameTh }}</div>
                    <div class="text-sm text-base-content/40">{{ item.nameEn }}</div>
                  </td>
                  <td>
                    <span class="badge badge-sm font-normal whitespace-nowrap text-sm"
                      :class="item.isSubsidiary ? 'badge-warning' : 'badge-ghost'">
                      {{ item.isSubsidiary ? 'เฉพาะบริษัทในเครือ' : 'ใช้ได้ทุกบริษัท (Global)' }}
                    </span>
                  </td>
                  <td>
                    <span class="badge badge-sm font-normal whitespace-nowrap text-sm"
                      :class="item.isActive ? 'badge-success text-success-content' : 'badge-ghost'">
                      {{ item.isActive ? 'ใช้งานอยู่' : 'ปิดใช้งาน' }}
                    </span>
                  </td>
                  <td>
                    <div class="flex items-center justify-end gap-1">
                      <button
                        class="btn btn-ghost btn-xs btn-square text-warning/70 hover:text-warning hover:bg-warning/10"
                        title="แก้ไขข้อมูล"
                        @click="openEditModal(item)">
                        <Edit class="size-4" />
                      </button>
                      <button class="btn btn-ghost btn-xs btn-square text-error/70 hover:text-error hover:bg-error/10"
                        title="ลบ"
                        @click="confirmDelete(item)">
                        <Trash2 class="size-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <!-- Pagination -->
          <div class="flex flex-col sm:flex-row items-center justify-between gap-3 px-5 py-4 border-t border-base-200">
            <p class="text-xs text-base-content/45">
              แสดง {{ (filter.page - 1) * filter.pageSize + 1 }}–{{ Math.min(filter.page * filter.pageSize, totalCount) }}
              จาก {{ totalCount }} รายการ
            </p>
            <div class="join">
              <button class="join-item btn btn-sm btn-ghost" :disabled="filter.page === 1" @click="goToPage(1)">
                <ChevronsLeft class="size-4" />
              </button>
              <button class="join-item btn btn-sm btn-ghost" :disabled="filter.page === 1" @click="goToPage(filter.page - 1)">
                <ChevronLeft class="size-4" />
              </button>
              <button
                v-for="p in pageWindow"
                :key="p"
                class="join-item btn btn-sm"
                :class="p === filter.page ? 'btn-primary' : 'btn-ghost'"
                @click="goToPage(p)"
              >
                {{ p }}
              </button>
              <button class="join-item btn btn-sm btn-ghost" :disabled="filter.page >= totalPages" @click="goToPage(filter.page + 1)">
                <ChevronRight class="size-4" />
              </button>
              <button class="join-item btn btn-sm btn-ghost" :disabled="filter.page >= totalPages" @click="goToPage(totalPages)">
                <ChevronsRight class="size-4" />
              </button>
            </div>
          </div>
        </template>
      </template>
    </div>
  </div>

  <PositionLevelFormModal ref="formModalRef" :is-create="isCreate" @save="handleSave" />
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { usePositionLevelStore } from '../../stores/positionLevelStore.ts'
import PageHeader from '../../components/ui/PageHeader.vue'
import PositionLevelFormModal from '../../components/position-level/PositionLevelFormModal.vue'
import type { PositionLevel, PositionLevelFilter } from '../../types/PositionLevel'
import { notify, extractErrorMessage } from '../../utils/notify'
import {
  Search,
  Plus,
  Edit,
  Trash2,
  CircleAlert,
  LayoutList,
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
} from 'lucide-vue-next'

const positionLevelStore = usePositionLevelStore()
const { positionLevels, totalCount, isLoading, errorMessage } = storeToRefs(positionLevelStore)

const filter = reactive<PositionLevelFilter>({ search: '', page: 1, pageSize: 10 })

// ========================================
// Computed
// ========================================
const totalPages = computed(() => Math.ceil(totalCount.value / filter.pageSize) || 1)

const pageWindow = computed(() => {
  const maxButtons = 5
  let start = Math.max(1, filter.page - Math.floor(maxButtons / 2))
  let end = start + maxButtons - 1
  if (end > totalPages.value) {
    end = totalPages.value
    start = Math.max(1, end - maxButtons + 1)
  }
  const pages: number[] = []
  for (let p = start; p <= end; p++) pages.push(p)
  return pages
})

// ========================================
// Fetch
// ========================================
function fetchData() {
  return positionLevelStore.fetchList(filter)
}

function resetPageAndFetch() {
  filter.page = 1
  fetchData()
}

let debounceTimer: ReturnType<typeof setTimeout>
function onSearchInput() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(resetPageAndFetch, 300)
}

function goToPage(p: number) {
  filter.page = p
  fetchData()
}

watch(() => filter.pageSize, resetPageAndFetch)

// ========================================
// Modal
// ========================================
const formModalRef = ref<InstanceType<typeof PositionLevelFormModal>>()
const isCreate = ref(true)
const editingId = ref<string | null>(null)

function openCreateModal() {
  isCreate.value = true
  editingId.value = null
  formModalRef.value?.open()
}

function openEditModal(item: PositionLevel) {
  isCreate.value = false
  editingId.value = item.id
  formModalRef.value?.open(item)
}

async function handleSave(payload: any, isCreateMode: boolean) {
  try {
    if (isCreateMode) await positionLevelStore.create(payload)
    else await positionLevelStore.update(editingId.value!, payload)

    formModalRef.value?.close()
    if (isCreateMode) filter.page = 1
    await fetchData()
    await notify.success(isCreateMode ? 'เพิ่มระดับตำแหน่งสำเร็จ' : 'บันทึกการแก้ไขสำเร็จ')
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'เกิดข้อผิดพลาด', formModalRef.value?.getDialogEl())
    console.error('Save error:', err)
  }
}

// ========================================
// Delete
// ========================================
async function confirmDelete(item: PositionLevel) {
  const ok = await notify.confirm(`ยืนยันลบระดับตำแหน่ง "${item.nameTh}" ใช่หรือไม่`, 'ยืนยันการลบ')
  if (!ok) return

  try {
    await positionLevelStore.remove(item.id)
    await notify.success('ลบระดับตำแหน่งสำเร็จ')

    // ถ้าลบรายการสุดท้ายของหน้า ให้ถอยกลับไปหน้าก่อนหน้า
    if (positionLevels.value.length === 1 && filter.page > 1) {
      filter.page -= 1
    }
    await fetchData()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ลบไม่สำเร็จ')
    console.error('Delete error:', err)
  }
}

// ========================================
// Lifecycle
// ========================================
onMounted(fetchData)
</script>