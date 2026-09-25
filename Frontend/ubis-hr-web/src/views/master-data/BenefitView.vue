<template>
  <div class="p-4 sm:p-6 space-y-5 mx-auto">
    <PageHeader title="Benefit Management" description="จัดการสวัสดิการ, กลุ่มสวัสดิการ และสิทธิ์ในแต่ละกลุ่ม">
      <template #actions>
        <div class="relative w-56">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-gray-400 pointer-events-none" />
          <input
            v-model="search"
            type="text"
            placeholder="ค้นหากลุ่มสวัสดิการ..."
            class="input input-bordered input-sm w-full pl-9 text-sm"
          />
        </div>
        <button class="btn btn-ghost btn-sm border border-base-300 gap-1.5" @click="openManageBenefits">
          <Settings2 class="size-4" /> จัดการสวัสดิการ
        </button>
        <button class="btn btn-ghost btn-sm border border-base-300 gap-1.5" @click="openCreateBenefit">
          <Plus class="size-4" /> เพิ่มสวัสดิการ
        </button>
        <button class="btn btn-primary btn-sm gap-1.5" @click="openCreatePlan">
          <Plus class="size-4" /> เพิ่มกลุ่ม
        </button>
      </template>
    </PageHeader>

    <div class="grid grid-cols-1 lg:grid-cols-[320px_1fr] gap-4">
      <!-- ซ้าย: รายการกลุ่มสวัสดิการ -->
      <div class="bg-base-100 rounded-2xl shadow-sm border border-base-200 overflow-hidden flex flex-col">
        <div class="px-4 py-3 border-b border-base-200 flex items-center justify-between">
          <p class="text-xs font-semibold text-base-content/50 uppercase tracking-wide">กลุ่มสวัสดิการ</p>
          <span class="badge badge-ghost badge-sm">{{ filteredPlans.length }}</span>
        </div>
        <div v-if="isLoading" class="p-3 space-y-2">
          <div v-for="i in 3" :key="i" class="skeleton h-16 w-full rounded-xl"></div>
        </div>
        <div v-else-if="filteredPlans.length === 0" class="flex flex-col items-center gap-2 p-10 text-center">
          <FolderKanban class="size-7 text-base-content/25" />
          <p class="text-sm text-base-content/50">ไม่พบกลุ่มสวัสดิการ</p>
        </div>
        <div v-else class="overflow-y-auto p-2 space-y-1.5 flex-1">
          <button
            v-for="plan in filteredPlans"
            :key="plan.id"
            type="button"
            class="w-full text-left rounded-xl border px-3.5 py-3 transition-colors group"
            :class="selectedPlanId === plan.id
              ? 'border-primary bg-primary/5'
              : 'border-transparent hover:bg-base-200/50'"
            @click="selectedPlanId = plan.id">
            <div class="flex items-start justify-between gap-2">
              <div class="min-w-0">
                <div class="flex items-center gap-1.5">
                  <p class="font-medium text-sm truncate" :class="selectedPlanId === plan.id && 'text-primary'">
                    {{ plan.nameTh }}
                  </p>
                  <span v-if="!plan.isActive" class="badge badge-ghost badge-xs">ปิดใช้งาน</span>
                </div>
                <p class="text-xs text-base-content/40 truncate">{{ plan.nameEn }}</p>
              </div>
              <div class="flex items-center gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity shrink-0">
                <button
                  type="button"
                  class="btn btn-ghost btn-xs btn-square text-warning/70 hover:text-warning hover:bg-warning/10"
                  title="แก้ไขกลุ่ม"
                  @click.stop="openEditPlan(plan)"
                >
                  <Edit class="size-3.5" />
                </button>
                <button
                  type="button"
                  class="btn btn-ghost btn-xs btn-square text-error/70 hover:text-error hover:bg-error/10"
                  title="ลบกลุ่ม"
                  @click.stop="confirmDeletePlan(plan)"
                >
                  <Trash2 class="size-3.5" />
                </button>
              </div>
            </div>
            <p class="text-xs text-base-content/45 mt-1.5">{{ planItemCount(plan.id) }} สวัสดิการ</p>
          </button>
        </div>
      </div>

      <!-- ขวา: สวัสดิการในกลุ่มที่เลือก -->
      <div class="bg-base-100 rounded-2xl shadow-sm border border-base-200 overflow-hidden flex flex-col">
        <div class="px-5 py-4 border-b border-base-200 flex items-center justify-between gap-3">
          <div class="min-w-0">
            <p class="font-medium text-base-content/80 truncate">
              {{ selectedPlan ? `สวัสดิการในกลุ่ม: ${selectedPlan.nameTh}` : 'เลือกกลุ่มสวัสดิการ' }}
            </p>
            <p v-if="selectedPlan?.description" class="text-xs text-base-content/40 truncate">
              {{ selectedPlan.description }}
            </p>
          </div>
          <button class="btn btn-primary btn-sm gap-1.5 shrink-0"
            :disabled="!selectedPlanId"
            @click="openCreateItem">
            <Plus class="size-4" /> เพิ่มสวัสดิการในกลุ่ม
          </button>
        </div>

        <!-- ยังไม่ได้เลือกกลุ่ม -->
        <div v-if="!selectedPlanId" class="flex flex-col items-center gap-2 p-16 text-center flex-1 justify-center">
          <ListChecks class="size-8 text-base-content/25" />
          <p class="text-base-content/50 text-sm">เลือกกลุ่มสวัสดิการทางซ้ายเพื่อดูรายละเอียด</p>
        </div>
        <div v-else-if="isLoading" class="p-5 space-y-2">
          <div v-for="i in 4" :key="i" class="skeleton h-12 w-full rounded-lg"></div>
        </div>
        <!-- กลุ่มที่เลือกยังไม่มีสวัสดิการ -->
        <div v-else-if="itemsOfSelectedPlan.length === 0" class="flex flex-col items-center gap-2 p-16 text-center flex-1 justify-center">
          <Gift class="size-8 text-base-content/25" />
          <p class="text-base-content/50 text-sm">กลุ่มนี้ยังไม่มีสวัสดิการ</p>
          <button class="btn btn-primary btn-sm gap-1.5 mt-2" @click="openCreateItem">
            <Plus class="size-4" /> เพิ่มสวัสดิการในกลุ่ม
          </button>
        </div>
        <div v-else class="overflow-x-auto flex-1">
          <table class="table">
            <thead>
              <tr class="text-xs uppercase tracking-wide text-base-content/45 border-b border-base-200">
                <th class="bg-base-100">สวัสดิการ</th>
                <th class="bg-base-100">วงเงิน</th>
                <th class="bg-base-100">สถานะ</th>
                <th class="bg-base-100 text-right">จัดการ</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in itemsOfSelectedPlan" :key="item.id" class="hover:bg-base-200/40 border-b border-base-200/60 last:border-0">
                <td>
                  <div class="font-medium text-base-content/80">{{ item.benefitNameTh }}</div>
                  <div class="font-medium text-base-content/40">{{ item.description || '-' }}</div>
                </td>
                <td class="font-medium text-base-content/80 whitespace-nowrap">{{ formatCurrency(item.limitAmount) }} บาท</td>
                <td>
                  <span class="badge badge-sm font-normal" :class="item.isActive ? 'badge-success text-success-content' : 'badge-ghost'">
                    {{ item.isActive ? 'ใช้งานอยู่' : 'ปิดใช้งาน' }}
                  </span>
                </td>
                <td>
                  <div class="flex items-center justify-end gap-1">
                    <button class="btn btn-ghost btn-xs btn-square text-warning/70 hover:text-warning hover:bg-warning/10" @click="openEditItem(item)">
                      <Edit class="size-4" />
                    </button>
                    <button class="btn btn-ghost btn-xs btn-square text-error/70 hover:text-error hover:bg-error/10" @click="confirmDeleteItem(item)">
                      <Trash2 class="size-4" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>

  <!-- Modal: จัดการรายชื่อสวัสดิการ (Master List) -->
  <dialog ref="manageBenefitsDialogRef" class="modal">
    <div class="modal-box w-11/12 max-w-2xl p-0 overflow-hidden flex flex-col max-h-[85vh]">
      <div class="flex items-center justify-between gap-3 px-6 py-5 border-b border-base-200 shrink-0">
        <div>
          <h3 class="font-semibold text-base leading-tight">จัดการรายชื่อสวัสดิการ</h3>
          <p class="text-xs text-base-content/45 mt-0.5">{{ filteredBenefits.length }} จาก {{ benefits.length }} รายการ</p>
        </div>
        <div class="flex items-center gap-2">
          <button class="btn btn-primary btn-sm gap-1.5" @click="openCreateBenefit">
            <Plus class="size-4" /> เพิ่มสวัสดิการ
          </button>
          <button type="button" class="btn btn-ghost btn-sm btn-square" @click="closeManageBenefits">
            <X class="size-4" />
          </button>
        </div>
      </div>
      <div class="px-6 py-3 border-b border-base-200 shrink-0">
        <div class="relative max-w-xs">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-gray-400 pointer-events-none" />
          <input
            v-model="benefitSearch"
            type="text"
            placeholder="ค้นหาชื่อสวัสดิการ..."
            class="input input-bordered input-sm w-full pl-9 text-sm"
          />
        </div>
      </div>
      <div class="overflow-y-auto flex-1">
        <table class="table">
          <thead>
            <tr class="text-xs uppercase tracking-wide text-base-content/45 border-b border-base-200">
              <th class="bg-base-100">ชื่อสวัสดิการ</th>
              <th class="bg-base-100">สถานะ</th>
              <th class="bg-base-100 text-right">จัดการ</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in filteredBenefits" :key="item.id" class="hover:bg-base-200/40 border-b border-base-200/60 last:border-0">
              <td>
                <div class="font-medium text-base-content/80">{{ item.nameTh }}</div>
                <div class="text-xs text-base-content/40">{{ item.nameEn }}</div>
              </td>
              <td>
                <span class="badge badge-sm font-normal" :class="item.isActive ? 'badge-success text-success-content' : 'badge-ghost'">
                  {{ item.isActive ? 'ใช้งานอยู่' : 'ปิดใช้งาน' }}
                </span>
              </td>
              <td>
                <div class="flex items-center justify-end gap-1">
                  <button class="btn btn-ghost btn-xs btn-square text-warning/70 hover:text-warning hover:bg-warning/10" @click="openEditBenefit(item)">
                    <Edit class="size-4" />
                  </button>
                  <button class="btn btn-ghost btn-xs btn-square text-error/70 hover:text-error hover:bg-error/10" @click="confirmDeleteBenefit(item)">
                    <Trash2 class="size-4" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="benefits.length === 0" class="flex flex-col items-center gap-2 p-14 text-center">
          <Gift class="size-8 text-base-content/25" />
          <p class="text-base-content/50 text-sm">ยังไม่มีสวัสดิการในระบบ</p>
        </div>
        <div v-else-if="filteredBenefits.length === 0" class="flex flex-col items-center gap-2 p-14 text-center">
          <Search class="size-8 text-base-content/25" />
          <p class="text-base-content/50 text-sm">ไม่พบสวัสดิการที่ค้นหา</p>
        </div>
      </div>
    </div>
    <form method="dialog" class="modal-backdrop">
      <button>close</button>
    </form>
  </dialog>

  <BenefitFormModal ref="benefitModalRef" :is-create="isCreateBenefit" @save="handleSaveBenefit" />
  <BenefitPlanFormModal ref="planModalRef" :is-create="isCreatePlan" @save="handleSavePlan" />
  <BenefitPlanItemFormModal
    ref="itemModalRef"
    :is-create="isCreateItem"
    :benefit-options="benefitOptions"
    @save="handleSaveItem"
  />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useBenefitStore } from '../../stores/benefitStore.ts'
import PageHeader from '../../components/ui/PageHeader.vue'
import BenefitFormModal from '../../components/benefit/BenefitFormModal.vue'
import BenefitPlanFormModal from '../../components/benefit/BenefitPlanFormModal.vue'
import BenefitPlanItemFormModal from '../../components/benefit/BenefitPlanItemFormModal.vue'
import type { Benefit, BenefitPlan, BenefitPlanItem } from '../../types/Benefit'
import { notify, extractErrorMessage } from '../../utils/notify'
import { Plus, Edit, Trash2, Search, Settings2, FolderKanban, ListChecks, Gift, X } from 'lucide-vue-next'

const benefitStore = useBenefitStore()
const { benefits, benefitPlans, benefitPlanItems, isLoading } = storeToRefs(benefitStore)

const search = ref('')
const benefitSearch = ref('')
const selectedPlanId = ref<string | null>(null)

// ========================================
// Computed
// ========================================
const benefitOptions = computed(() => benefitStore.benefitOptions)

const filteredPlans = computed(() => {
  const q = search.value.trim().toLowerCase()
  if (!q) return benefitPlans.value
  return benefitPlans.value.filter(
    (p) => p.nameTh.toLowerCase().includes(q) || p.nameEn.toLowerCase().includes(q)
  )
})

const filteredBenefits = computed(() => {
  const q = benefitSearch.value.trim().toLowerCase()
  if (!q) return benefits.value
  return benefits.value.filter(
    (b) => b.nameTh.toLowerCase().includes(q) || b.nameEn.toLowerCase().includes(q)
  )
})

const selectedPlan = computed(
  () => benefitPlans.value.find((p) => p.id === selectedPlanId.value) ?? null
)

const itemsOfSelectedPlan = computed(() =>
  benefitPlanItems.value.filter((item) => item.benefitPlanId === selectedPlanId.value)
)

function planItemCount(planId: string) {
  return benefitStore.itemCountByPlanId.get(planId) ?? 0
}

function formatCurrency(n: number) {
  return n.toLocaleString('th-TH', { minimumFractionDigits: 0, maximumFractionDigits: 2 })
}

// ========================================
// Fetch
// ========================================
async function fetchAll() {
  try {
    await benefitStore.fetchAll()

    // เลือกกลุ่มแรกให้อัตโนมัติ ถ้ายังไม่ได้เลือก หรือกลุ่มที่เลือกไว้ถูกลบไปแล้ว
    if (!selectedPlanId.value || !benefitPlans.value.some((p) => p.id === selectedPlanId.value)) {
      selectedPlanId.value = benefitPlans.value[0]?.id ?? null
    }
  } catch {
    await notify.error('โหลดข้อมูลสวัสดิการไม่สำเร็จ')
  }
}

onMounted(fetchAll)

// ========================================
// จัดการรายชื่อสวัสดิการ (Modal)
// ========================================
const manageBenefitsDialogRef = ref<HTMLDialogElement>()

function openManageBenefits() {
  manageBenefitsDialogRef.value?.showModal()
}
function closeManageBenefits() {
  manageBenefitsDialogRef.value?.close()
}

// ========================================
// Benefit
// ========================================
const benefitModalRef = ref<InstanceType<typeof BenefitFormModal>>()
const isCreateBenefit = ref(true)
const editingBenefitId = ref<string | null>(null)

function openCreateBenefit() {
  isCreateBenefit.value = true
  editingBenefitId.value = null
  benefitModalRef.value?.open()
}

function openEditBenefit(item: Benefit) {
  isCreateBenefit.value = false
  editingBenefitId.value = item.id
  benefitModalRef.value?.open(item)
}

async function handleSaveBenefit(payload: any, isCreateMode: boolean) {
  try {
    if (isCreateMode) await benefitStore.createBenefit(payload)
    else await benefitStore.updateBenefit(editingBenefitId.value!, payload)

    benefitModalRef.value?.close()
    await fetchAll()

    // Modal "จัดการรายชื่อสวัสดิการ" ยังเปิดอยู่ ต้องระบุ target เป็น Dialog นั้น
    await notify.success(
      isCreateMode ? 'เพิ่มสวัสดิการสำเร็จ' : 'บันทึกการแก้ไขสำเร็จ',
      'สำเร็จ',
      manageBenefitsDialogRef.value
    )
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'เกิดข้อผิดพลาด', benefitModalRef.value?.getDialogEl())
  }
}

async function confirmDeleteBenefit(item: Benefit) {
  const ok = await notify.confirm(
    `ยืนยันลบสวัสดิการ "${item.nameTh}" ใช่หรือไม่`,
    'ยืนยันการลบ',
    manageBenefitsDialogRef.value
  )
  if (!ok) return

  try {
    await benefitStore.removeBenefit(item.id)
    await notify.success('ลบสวัสดิการสำเร็จ', 'สำเร็จ', manageBenefitsDialogRef.value)
    await fetchAll()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ลบไม่สำเร็จ', manageBenefitsDialogRef.value)
  }
}

// ========================================
// Benefit Plan (กลุ่มสวัสดิการ)
// ========================================
const planModalRef = ref<InstanceType<typeof BenefitPlanFormModal>>()
const isCreatePlan = ref(true)
const editingPlanId = ref<string | null>(null)

function openCreatePlan() {
  isCreatePlan.value = true
  editingPlanId.value = null
  planModalRef.value?.open()
}

function openEditPlan(item: BenefitPlan) {
  isCreatePlan.value = false
  editingPlanId.value = item.id
  planModalRef.value?.open(item)
}

async function handleSavePlan(payload: any, isCreateMode: boolean) {
  try {
    if (isCreateMode) {
      const created = await benefitStore.createPlan(payload)
      planModalRef.value?.close()
      await fetchAll()
      selectedPlanId.value = created.id   // เลือกกลุ่มที่เพิ่งสร้างให้เลย
    } else {
      await benefitStore.updatePlan(editingPlanId.value!, payload)
      planModalRef.value?.close()
      await fetchAll()
    }

    await notify.success(isCreateMode ? 'เพิ่มกลุ่มสวัสดิการสำเร็จ' : 'บันทึกการแก้ไขสำเร็จ')
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'เกิดข้อผิดพลาด', planModalRef.value?.getDialogEl())
  }
}

async function confirmDeletePlan(item: BenefitPlan) {
  const ok = await notify.confirm(`ยืนยันลบกลุ่มสวัสดิการ "${item.nameTh}" ใช่หรือไม่`, 'ยืนยันการลบ')
  if (!ok) return

  try {
    await benefitStore.removePlan(item.id)
    await notify.success('ลบกลุ่มสวัสดิการสำเร็จ')
    if (selectedPlanId.value === item.id) selectedPlanId.value = null
    await fetchAll()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ลบไม่สำเร็จ')
  }
}

// ========================================
// Benefit Plan Item (สวัสดิการในกลุ่ม)
// ========================================
const itemModalRef = ref<InstanceType<typeof BenefitPlanItemFormModal>>()
const isCreateItem = ref(true)
const editingItemId = ref<string | null>(null)

function openCreateItem() {
  if (!selectedPlanId.value || !selectedPlan.value) return
  isCreateItem.value = true
  editingItemId.value = null
  itemModalRef.value?.open(selectedPlanId.value, selectedPlan.value.nameTh)
}

function openEditItem(item: BenefitPlanItem) {
  if (!selectedPlan.value) return
  isCreateItem.value = false
  editingItemId.value = item.id
  itemModalRef.value?.open(item.benefitPlanId, selectedPlan.value.nameTh, item)
}

async function handleSaveItem(payload: any, isCreateMode: boolean) {
  try {
    if (isCreateMode) await benefitStore.createItem(payload)
    else await benefitStore.updateItem(editingItemId.value!, payload)

    itemModalRef.value?.close()
    await fetchAll()
    await notify.success(isCreateMode ? 'เพิ่มสวัสดิการในกลุ่มสำเร็จ' : 'บันทึกการแก้ไขสำเร็จ')
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'เกิดข้อผิดพลาด', itemModalRef.value?.getDialogEl())
  }
}

async function confirmDeleteItem(item: BenefitPlanItem) {
  const ok = await notify.confirm(`ยืนยันลบ "${item.benefitNameTh}" ออกจากกลุ่มนี้ใช่หรือไม่`, 'ยืนยันการลบ')
  if (!ok) return

  try {
    await benefitStore.removeItem(item.id)
    await notify.success('ลบสวัสดิการออกจากกลุ่มสำเร็จ')
    await fetchAll()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ลบไม่สำเร็จ')
  }
}
</script>