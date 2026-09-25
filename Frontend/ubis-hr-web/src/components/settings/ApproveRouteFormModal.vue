<template>
  <dialog ref="dialogRef" class="modal">
    <div class="modal-box w-11/12 max-w-lg overflow-hidden rounded-2xl p-0 flex flex-col">
      <!-- Header -->
      <div class="flex shrink-0 items-start justify-between gap-4 border-b border-base-200 px-6 py-5">
        <div class="flex items-start gap-3">
          <div class="flex size-11 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
            <GitBranch class="size-5" />
          </div>
          <div>
            <h3 class="text-lg font-semibold leading-tight text-base-content">
              {{ isEditMode ? "แก้ไขขั้นตอนอนุมัติ" : "เพิ่มขั้นตอนอนุมัติ" }}
            </h3>
            <p class="mt-1.5 text-sm text-base-content/50">
              กำหนดว่าขั้นตอนนี้ใครเป็นผู้อนุมัติ
            </p>
          </div>
        </div>
        <button
          type="button"
          class="btn btn-ghost btn-sm btn-square rounded-lg"
          @click="close"
        >
          <X class="size-4" />
        </button>
      </div>

      <form
        @submit.prevent="handleSubmit"
        class="flex min-h-0 flex-1 flex-col"
      >
        <!-- Form -->
        <div class="flex-1 space-y-5 overflow-y-auto px-6 py-5">
          <!-- Document Type / Step -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="mb-1.5 block text-sm font-medium text-base-content/65">
                ประเภทเอกสาร
                <span class="text-error">*</span>
              </label>

              <input
                v-model="form.docType"
                type="text"
                required
                placeholder="เช่น PrettyCash"
                class="input input-bordered h-10 w-full bg-base-100 text-sm focus:border-primary"
              />
            </div>

            <div>
              <label
                class="mb-1.5 block text-sm font-medium text-base-content/65"
              >
                ลำดับขั้นตอน
                <span class="text-error">*</span>
              </label>

              <input
                v-model.number="form.stepNo"
                type="number"
                min="1"
                required
                class="input input-bordered h-10 w-full bg-base-100 text-sm focus:border-primary"
              />
            </div>
          </div>

          <!-- Step Name -->
          <div>
            <label
              class="mb-1.5 block text-sm font-medium text-base-content/65"
            >
              ชื่อขั้นตอน
              <span class="text-error">*</span>
            </label>

            <input
              v-model="form.stepName"
              type="text"
              required
              placeholder="เช่น หัวหน้าสายงาน"
              class="input input-bordered h-10 w-full bg-base-100 text-sm focus:border-primary"
            />
          </div>

          <!-- Approver Type -->
          <div>
            <FormSelect
              v-model="form.approverType"
              label="ประเภทผู้อนุมัติ"
              required
              :options="approverTypeOptions"
              @update:model-value="onApproverTypeChange"
            />

            <p class="mt-1.5 pl-0.5 text-xs text-base-content/50">
              {{ approverTypeHint }}
            </p>
          </div>

          <!-- ManagerChain -->
          <div v-if="form.approverType === 'ManagerChain'">
            <label
              class="mb-1.5 block text-sm font-medium text-base-content/65"
            >
              ระดับตำแหน่งขั้นต่ำ
              <span class="text-error">*</span>
            </label>

            <input
              v-model.number="form.minPositionLevel"
              type="number"
              min="1"
              max="14"
              required
              placeholder="เช่น 8"
              class="input input-bordered h-10 w-full bg-base-100 text-sm focus:border-primary"
            />

            <p class="mt-1.5 pl-0.5 text-xs leading-5 text-base-content/50">
              ระบบจะไล่หาหัวหน้าขึ้นไปเรื่อยๆ จนเจอคนแรกที่ระดับตำแหน่งถึงเกณฑ์นี้
            </p>
          </div>

          <!-- FixedEmployee -->
          <div v-if="form.approverType === 'FixedEmployee'">
            <EmployeeSelect
              :model-value="form.fixedEmployeeId"
              label="ผู้อนุมัติ"
              required
              :options="employeeOptions"
              @change="(id) => (form.fixedEmployeeId = id ?? null)"
            />

            <p class="mt-1.5 pl-0.5 text-xs leading-5 text-base-content/50">
              ไม่ว่าใครเป็นผู้ขอ เอกสารจะมาที่คนนี้เสมอ
            </p>
          </div>

          <!-- BranchAdmin -->
          <div
            v-if="form.approverType === 'BranchAdmin'"
            class="rounded-lg border border-info/20 bg-info/5 px-4 py-3"
          >
            <p class="text-sm leading-6 text-base-content/65">
              ระบบจะใช้
              <span class="font-semibold">ผู้ดูแลสาขาหลัก</span>
              ของสาขาที่ผู้ขอเบิกสังกัดอยู่โดยอัตโนมัติ
              ไม่ต้องระบุเพิ่ม
            </p>
          </div>

          <!-- Active -->
          <label class="flex cursor-pointer items-center gap-2.5 pt-1">
            <input
              v-model="form.isActive"
              type="checkbox"
              class="checkbox checkbox-md checkbox-primary"
            />

            <span class="text-sm font-medium text-base-content">
              เปิดใช้งานขั้นตอนนี้
            </span>
          </label>

          <!-- Error -->
          <div
            v-if="errorMessage"
            class="flex items-center gap-2 rounded-lg bg-error/10 px-4 py-3 text-sm font-medium text-error"
          >
            <CircleAlert class="size-4 shrink-0" />
            {{ errorMessage }}
          </div>
        </div>

        <!-- Footer -->
        <div
          class="flex shrink-0 justify-end gap-2 border-t border-base-200 bg-base-100 px-6 py-4"
        >
          <button
            type="button"
            class="btn btn-ghost btn-sm rounded-lg px-5 text-sm"
            @click="close"
          >
            ยกเลิก
          </button>

          <button
            type="submit"
            class="btn btn-primary btn-sm rounded-lg px-6 text-sm"
            :disabled="isSubmitting"
          >
            <span
              v-if="isSubmitting"
              class="loading loading-spinner loading-xs"
            ></span>

            {{ isSubmitting ? "กำลังบันทึก..." : "บันทึก" }}
          </button>
        </div>
      </form>
    </div>

    <form method="dialog" class="modal-backdrop">
      <button>close</button>
    </form>
  </dialog>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import FormSelect from '../ui/FormSelect.vue'
import EmployeeSelect, { type EmployeeOption } from '../ui/EmployeeSelect.vue'
import type { RouteApprove, ApproverType } from '../../types/RouteApprove'
import { GitBranch, X, CircleAlert } from 'lucide-vue-next'

defineProps<{ employeeOptions: EmployeeOption[] }>()
const emit = defineEmits<{ save: [data: any, isCreate: boolean] }>()

const dialogRef = ref<HTMLDialogElement>()
const isSubmitting = ref(false)
const isEditMode = ref(false)
const errorMessage = ref('')

const approverTypeOptions = [
  { id: 'ManagerChain', label: 'ไล่ตามสายบังคับบัญชา' },
  { id: 'FixedEmployee', label: 'ระบุบุคคล' },
  { id: 'BranchAdmin', label: 'ผู้ดูแลสาขาของผู้ขอ' },
]

const defaultForm = () => ({
  docType: null as string | null,
  stepNo: 1,
  stepName: '',
  approverType: 'ManagerChain' as ApproverType,
  minPositionLevel: null as number | null,
  fixedEmployeeId: null as string | null,
  organizationUnitId: null as string | null,
  isActive: true,
})

const form = ref(defaultForm())

const approverTypeHint = computed(() => {
  switch (form.value.approverType) {
    case 'ManagerChain':
      return 'เหมาะกับขั้นตอนแรก ที่ต้องผ่านหัวหน้าของผู้ขอก่อน'
    case 'FixedEmployee':
      return 'เหมาะกับขั้นตอนที่มีผู้รับผิดชอบคนเดียวตายตัว เช่น ผู้จัดการฝ่ายบัญชี'
    case 'BranchAdmin':
      return 'เหมาะกับขั้นตอนตรวจสอบโดย HR ประจำสาขา'
    default:
      return ''
  }
})

function onApproverTypeChange() {
  // ล้างค่าที่ไม่เกี่ยวกับ Type ที่เลือก กันข้อมูลค้างจาก Type เดิม
  form.value.minPositionLevel = null
  form.value.fixedEmployeeId = null
  form.value.organizationUnitId = null
}

function open(item?: RouteApprove | null, presetDocType?: string) {
  errorMessage.value = ''
  isEditMode.value = !!item

  if (item) {
    form.value = {
      docType: item.docType,
      stepNo: item.stepNo,
      stepName: item.stepName,
      approverType: item.approverType,
      minPositionLevel: item.minPositionLevel,
      fixedEmployeeId: item.fixedEmployeeId,
      organizationUnitId: item.organizationUnitId,
      isActive: item.isActive,
    }
  } else {
    form.value = defaultForm()
    if (presetDocType) form.value.docType = presetDocType
  }

  dialogRef.value?.showModal()
}

function close() {
  dialogRef.value?.close()
}

function getDialogEl() {
  return dialogRef.value ?? null
}

function setError(msg: string) {
  errorMessage.value = msg
  isSubmitting.value = false
}

async function handleSubmit() {
  errorMessage.value = ''
  isSubmitting.value = true

  try {
    emit('save', { ...form.value }, !isEditMode.value)
  } finally {
    isSubmitting.value = false
  }
}
defineExpose({ open, close, getDialogEl, setError })
</script>