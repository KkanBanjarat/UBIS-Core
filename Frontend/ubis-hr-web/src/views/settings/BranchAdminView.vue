<template>
  <div class="p-4 sm:p-6 space-y-5 mx-auto">
    <PageHeader
      title="ผู้ดูแลสาขา"
      description="กำหนดผู้ดูแลแต่ละสาขา และเลือกผู้ดูแลหลักที่จะเป็นผู้อนุมัติเอกสาร"
    />

    <!-- แจ้งเตือนสาขาที่ยังไม่มีผู้ดูแลหลัก -->
    <div
      v-if="!isLoading && branchesWithoutPrimary.length > 0"
      class="flex items-start gap-3 rounded-xl border border-warning/30 bg-warning/5 px-4 py-3.5"
    >
      <TriangleAlert class="size-5 shrink-0 text-warning mt-0.5" />
      <div>
        <p class="text-sm font-medium text-base-content">
          มี {{ branchesWithoutPrimary.length }} สาขาที่ยังไม่มีผู้ดูแลหลัก
        </p>
        <p class="text-xs text-base-content/55 mt-1">
          เอกสารของพนักงานในสาขาเหล่านี้จะส่งอนุมัติไม่ได้
          ({{ branchesWithoutPrimary.map(b => b.branchCode).join(', ') }})
        </p>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="isLoading" class="space-y-4">
      <div v-for="i in 3" :key="i" class="rounded-2xl border border-base-200 bg-base-100 p-5">
        <div class="skeleton h-5 w-40 mb-4"></div>
        <div class="skeleton h-14 w-full rounded-xl"></div>
      </div>
    </div>

    <!-- Error -->
    <div
      v-else-if="errorMessage"
      class="flex flex-col items-center gap-2 p-14 text-center bg-base-100 rounded-2xl border border-base-200"
    >
      <CircleAlert class="size-8 text-error/70" />
      <p class="text-error text-sm">{{ errorMessage }}</p>
    </div>

    <!-- Content -->
    <div v-else class="space-y-4">
      <div
        v-for="group in groups"
        :key="group.branchId"
        class="overflow-hidden rounded-2xl border bg-base-100 shadow-sm"
        :class="hasPrimary(group) ? 'border-base-200' : 'border-warning/40'"
      >
        <!-- Branch Header -->
        <div class="flex items-center justify-between gap-3 border-b border-base-200 bg-base-200/40 px-5 py-4">
          <div class="flex items-center gap-3 min-w-0">
            <div
              class="flex size-9 shrink-0 items-center justify-center rounded-lg shadow-sm"
              :class="hasPrimary(group) ? 'bg-primary text-primary-content' : 'bg-warning text-warning-content'"
            >
              <Building2 class="size-4" />
            </div>
            <div class="min-w-0">
              <div class="flex items-center gap-2">
                <h3 class="text-sm font-semibold truncate">{{ group.branchNameTh }}</h3>
                <span class="badge badge-ghost badge-sm shrink-0">{{ group.branchCode }}</span>
              </div>
              <p class="text-xs text-base-content/45 mt-0.5">
                {{ group.admins.length }} ผู้ดูแล
                <span v-if="!hasPrimary(group)" class="text-warning font-medium"> · ยังไม่มีผู้ดูแลหลัก</span>
              </p>
            </div>
          </div>

          <button
            class="btn btn-ghost btn-sm gap-1.5 border border-base-300 shrink-0"
            @click="openCreateModal(group)"
          >
            <Plus class="size-3.5" /> เพิ่มผู้ดูแล
          </button>
        </div>

        <!-- Admin List -->
        <div v-if="group.admins.length > 0" class="divide-y divide-base-200">
          <div
            v-for="admin in group.admins"
            :key="admin.id"
            class="group flex items-center gap-3 px-5 py-3.5 transition-colors hover:bg-base-200/30"
          >
            <div
              class="flex size-9 shrink-0 items-center justify-center rounded-full text-xs font-semibold"
              :class="admin.isPrimary ? 'bg-primary/10 text-primary' : 'bg-base-200 text-base-content/50'"
            >
              {{ initials(admin.employeeNameTh) }}
            </div>

            <div class="min-w-0 flex-1">
              <div class="flex items-center gap-2 flex-wrap">
                <p class="text-sm font-medium truncate">{{ admin.employeeNameTh || '-' }}</p>
                <span v-if="admin.isPrimary" class="badge badge-primary badge-sm gap-1">
                  <Star class="size-3" /> ผู้ดูแลหลัก
                </span>
              </div>
              <p class="text-[11px] text-base-content/35 mt-0.5">
                แก้ไขล่าสุด {{ formatDateTime(admin.updatedAt) }} โดย {{ admin.updatedBy }}
              </p>
            </div>

            <div class="flex items-center gap-1 shrink-0">
              <button
                v-if="!admin.isPrimary"
                class="btn btn-ghost btn-xs gap-1 text-base-content/45 hover:text-primary hover:bg-primary/10"
                title="ตั้งเป็นผู้ดูแลหลัก"
                @click="confirmSetPrimary(group, admin)"
              >
                <Star class="size-3.5" />
                <span class="hidden sm:inline">ตั้งเป็นหลัก</span>
              </button>

              <button
                class="btn btn-ghost btn-xs btn-square text-error/60 hover:text-error hover:bg-error/10"
                title="ลบ"
                @click="confirmDelete(group, admin)"
              >
                <Trash2 class="size-3.5" />
              </button>
            </div>
          </div>
        </div>

        <!-- Empty -->
        <div v-else class="flex flex-col items-center gap-2 py-8 text-center">
          <UserX class="size-7 text-base-content/25" />
          <p class="text-xs text-base-content/45">สาขานี้ยังไม่มีผู้ดูแล</p>
        </div>
      </div>
    </div>
  </div>

  <BranchAdminFormModal ref="formModalRef" :employee-options="allowedEmployees" @save="handleSave" />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { usePodAdminBranchStore } from '../../stores/podAdminBranchStore'
import hrApi from '../../services/hrApi'
import PageHeader from '../../components/ui/PageHeader.vue'
import BranchAdminFormModal from '../../components/settings/BranchAdminFormModal.vue'
import type { EmployeeOption } from '../../components/ui/EmployeeSelect.vue'
import type { BranchAdminGroup, BranchAdminItem } from '../../types/PodAdminBranch'
import { notify, extractErrorMessage } from '../../utils/notify'
import { Plus, Trash2, CircleAlert, Building2, Star, UserX, TriangleAlert } from 'lucide-vue-next'

const podAdminStore = usePodAdminBranchStore()
const { groups, isLoading, errorMessage } = storeToRefs(podAdminStore)

const allowedEmployees = ref<EmployeeOption[]>([])
const formModalRef = ref<InstanceType<typeof BranchAdminFormModal>>()

const branchesWithoutPrimary = computed(() => podAdminStore.branchesWithoutPrimary)

// ========================================
// Helpers
// ========================================
function hasPrimary(group: BranchAdminGroup) {
  return group.admins.some((a) => a.isPrimary)
}

function initials(name: string | null) {
  return name?.charAt(0)?.toUpperCase() ?? '?'
}

function formatDateTime(d: string) {
  return new Date(d).toLocaleString('th-TH', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

// ========================================
// Actions
// ========================================
function openCreateModal(group: BranchAdminGroup) {
  // ถ้าสาขานี้ยังไม่มีผู้ดูแลหลัก ให้ติ๊ก "ตั้งเป็นหลัก" ไว้ให้เลย
  formModalRef.value?.open(group.branchId, group.branchNameTh, !hasPrimary(group))
}

async function handleSave(payload: { employeeId: string; branchId: string; isPrimary: boolean }) {
  try {
    await podAdminStore.create(payload)
    formModalRef.value?.close()
    await podAdminStore.fetchAll()
    await notify.success('เพิ่มผู้ดูแลสาขาสำเร็จ')
  } catch (err) {
    // Error จาก Backend (เช่นไม่พบบัญชีผู้ใช้งาน) แสดงใน Modal เลย ไม่ต้องปิด
    formModalRef.value?.setError(extractErrorMessage(err))
  }
}

async function confirmSetPrimary(group: BranchAdminGroup, admin: BranchAdminItem) {
  const current = group.admins.find((a) => a.isPrimary)
  const message = current
    ? `ยืนยันเปลี่ยนผู้ดูแลหลักของสาขา ${group.branchNameTh}\nจาก "${current.employeeNameTh}" เป็น "${admin.employeeNameTh}" ใช่หรือไม่`
    : `ยืนยันตั้ง "${admin.employeeNameTh}" เป็นผู้ดูแลหลักของสาขา ${group.branchNameTh} ใช่หรือไม่`

  const ok = await notify.confirm(message, 'ยืนยันเปลี่ยนผู้ดูแลหลัก')
  if (!ok) return

  try {
    await podAdminStore.setPrimary(admin.id)
    await notify.success('ตั้งผู้ดูแลหลักสำเร็จ')
    await podAdminStore.fetchAll()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ตั้งผู้ดูแลหลักไม่สำเร็จ')
  }
}

async function confirmDelete(group: BranchAdminGroup, admin: BranchAdminItem) {
  const warning = admin.isPrimary
    ? '\n\n⚠️ คนนี้เป็นผู้ดูแลหลัก หากลบแล้วเอกสารของสาขานี้จะส่งอนุมัติไม่ได้จนกว่าจะตั้งคนใหม่'
    : ''

  const ok = await notify.confirm(
    `ยืนยันลบ "${admin.employeeNameTh}" ออกจากผู้ดูแลสาขา ${group.branchNameTh} ใช่หรือไม่${warning}`,
    'ยืนยันการลบ'
  )
  if (!ok) return

  try {
    await podAdminStore.remove(admin.id)
    await notify.success('ลบผู้ดูแลสาขาสำเร็จ')
    await podAdminStore.fetchAll()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ลบไม่สำเร็จ')
  }
}

// ========================================
// Lifecycle
// ========================================
onMounted(async () => {
  podAdminStore.fetchAll()
  try {
    const res = await hrApi.post('/Employees/search', { pageSize: 500, status: 'Active' })
    allowedEmployees.value = res.data.items.map((x: any) => ({
      id: x.id,
      empId: x.empId,
      fullNameTh: `${x.firstNameTh} ${x.lastNameTh}`,
      positionNameTh: x.positionNameTh,
    }))
  } catch (err) {
    console.error('Failed to load employees:', err)
  }
})
</script>