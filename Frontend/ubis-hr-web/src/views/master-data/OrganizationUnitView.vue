<template>
  <div class="p-4 sm:p-6 space-y-5 mx-auto">
    <PageHeader title="โครงสร้างองค์กร" description="จัดการหน่วยงาน (สายงาน / ฝ่าย / แผนก / ส่วนงาน)">
      <template #actions>
        <button class="btn btn-primary btn-sm gap-1.5" @click="openCreateModal">
          <Plus class="size-4" />
          เพิ่มหน่วยงาน
        </button>
      </template>
    </PageHeader>

    <div class="bg-base-100 rounded-2xl shadow-sm border border-base-200 overflow-hidden">
      <!-- Toolbar: Search + Type Filter + PageSize -->
      <div class="flex flex-col sm:flex-row gap-3 sm:items-center justify-between px-5 py-4 border-b border-base-200">
        <div class="flex flex-col sm:flex-row gap-3 flex-1">
          <div class="relative flex-1 min-w-[200px] max-w-sm">
            <Search class="absolute left-3.5 top-1/2 -translate-y-1/2 size-4 text-gray-400 pointer-events-none z-10" />
            <input
              v-model="filter.search"
              @input="onSearchInput"
              type="text"
              placeholder="ค้นหารหัส, ชื่อหน่วยงาน..."
              class="input input-bordered w-full pl-10 text-sm focus:outline-none focus:border-primary transition-colors"
            />
          </div>
          <div class="w-full sm:w-56">
            <FormSelect v-model="filter.type" :options="typeFilterOptions" placeholder="ประเภท: ทั้งหมด" />
          </div>
        </div>
        <select v-model.number="filter.pageSize" class="select select-bordered select-sm w-24">
          <option :value="10">10</option>
          <option :value="20">20</option>
          <option :value="50">50</option>
        </select>
      </div>

      <div v-if="isLoading" class="p-5 space-y-2">
        <div v-for="i in 5" :key="i" class="skeleton h-12 w-full rounded-lg"></div>
      </div>

      <div v-else-if="errorMessage" class="flex flex-col items-center gap-2 p-14 text-center">
        <CircleAlert class="size-8 text-error/70" />
        <p class="text-error text-sm">{{ errorMessage }}</p>
      </div>

      <template v-else>
        <div v-if="organizationUnits.length === 0" class="flex flex-col items-center gap-2 p-14 text-center">
          <Network class="size-8 text-base-content/25" />
          <p class="text-base-content/50 text-sm">ไม่พบข้อมูลหน่วยงาน</p>
        </div>

        <template v-else>
          <div class="overflow-x-auto">
            <table class="table min-w-[720px]">
              <thead>
                <tr class="text-xs uppercase tracking-wide text-base-content/45 border-b border-base-200">
                  <th class="bg-base-100">รหัส</th>
                  <th class="bg-base-100">ชื่อหน่วยงาน</th>
                  <th class="bg-base-100">ชื่อย่อ</th>
                  <th class="bg-base-100">ประเภท</th>
                  <th class="bg-base-100 text-right">จัดการ</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in organizationUnits"
                  :key="item.id"
                  class="hover:bg-base-200/40 transition-colors border-b border-base-200/60 last:border-0">
                  <td>
                    <div class="badge badge-soft badge-info badge-sm font-medium">{{ item.code || '-' }}</div>
                  </td>
                  <td>
                    <div class="font-semibold text-sm text-base-content/80">{{ item.nameTh }}</div>
                    <div class="text-sm text-base-content/40">{{ item.nameEn }}</div>
                  </td>
                  <td class="text-base-content/70 text-sm ">{{ item.shortName || '-' }}</td>
                  <td>
                    <span class="badge badge-sm font-normal whitespace-nowrap badge-ghost text-sm ">
                      {{ typeLabel(item.type) }}
                    </span>
                  </td>
                  <td>
                    <div class="flex items-center justify-end gap-1">
                      <button
                        class="btn btn-ghost btn-xs btn-square text-warning/70 hover:text-warning hover:bg-warning/10"
                        title="แก้ไขข้อมูล"
                        @click="openEditModal(item)"
                      >
                        <Edit class="size-4" />
                      </button>
                      <button
                        class="btn btn-ghost btn-xs btn-square text-error/70 hover:text-error hover:bg-error/10"
                        title="ลบ"
                        @click="confirmDelete(item)"
                      >
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

  <OrganizationUnitFormModal
    ref="formModalRef"
    :is-create="isCreate"
    :type-options="typeFormOptions"
    @save="handleSave"
  />
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useOrganizationUnitStore, type OrganizationUnitFilter } from '../../stores/organizationUnitStore.ts'
import PageHeader from '../../components/ui/PageHeader.vue'
import FormSelect from '../../components/ui/FormSelect.vue'
import OrganizationUnitFormModal from '../../components/organization-unit/OrganizationUnitFormModal.vue'
import type { OrganizationUnit } from '../../types/OrganizationUnit'
import { notify, extractErrorMessage } from '../../utils/notify'
import {
  Search,
  Plus,
  Edit,
  Trash2,
  CircleAlert,
  Network,
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
} from 'lucide-vue-next'

const orgUnitStore = useOrganizationUnitStore()
const { organizationUnits, totalCount, isLoading, errorMessage } = storeToRefs(orgUnitStore)

const filter = reactive<OrganizationUnitFilter>({
  search: '',
  type: null,
  page: 1,
  pageSize: 10,
})

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

// ตัวเลือกประเภท — ใช้ชุดเดียวกันทั้ง Filter และ Form
const typeFilterOptions = computed(() => orgUnitStore.typeOptions)
const typeFormOptions = computed(() => orgUnitStore.typeOptions)

function typeLabel(type: string) {
  return orgUnitStore.typeLabelMap.get(type) ?? type
}

// ========================================
// Fetch
// ========================================
function fetchData() {
  return orgUnitStore.fetchList(filter)
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

// ========================================
// Watchers
// ========================================
watch(() => filter.type, resetPageAndFetch)
watch(() => filter.pageSize, resetPageAndFetch)

// ========================================
// Modal
// ========================================
const formModalRef = ref<InstanceType<typeof OrganizationUnitFormModal>>()
const isCreate = ref(true)
const editingId = ref<string | null>(null)

function openCreateModal() {
  isCreate.value = true
  editingId.value = null
  formModalRef.value?.open()
}

function openEditModal(item: OrganizationUnit) {
  isCreate.value = false
  editingId.value = item.id
  formModalRef.value?.open(item)
}

async function handleSave(payload: any, isCreateMode: boolean) {
  try {
    if (isCreateMode) await orgUnitStore.create(payload)
    else await orgUnitStore.update(editingId.value!, payload)

    formModalRef.value?.close()
    if (isCreateMode) filter.page = 1
    await fetchData()
    await notify.success(isCreateMode ? 'เพิ่มหน่วยงานสำเร็จ' : 'บันทึกการแก้ไขสำเร็จ')
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'เกิดข้อผิดพลาด', formModalRef.value?.getDialogEl())
    console.error('Save error:', err)
  }
}

// ========================================
// Delete
// ========================================
async function confirmDelete(item: OrganizationUnit) {
  const ok = await notify.confirm(`ยืนยันลบหน่วยงาน "${item.nameTh}" ใช่หรือไม่`, 'ยืนยันการลบ')
  if (!ok) return

  try {
    await orgUnitStore.remove(item.id)
    await notify.success('ลบหน่วยงานสำเร็จ')

    // ถ้าลบรายการสุดท้ายของหน้า ให้ถอยกลับไปหน้าก่อนหน้า
    if (organizationUnits.value.length === 1 && filter.page > 1) {
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
onMounted(() => {
  // ไม่ต้องรอกัน — ตารางขึ้นได้เลยโดยไม่ต้องรอ Dropdown ประเภทโหลดเสร็จ
  orgUnitStore.fetchLevelTypes()   // มี Cache ในตัว เข้ารอบ 2 จะไม่ยิงซ้ำ
  fetchData()
})
</script>