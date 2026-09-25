<template>
  <dialog ref="dialogRef" class="modal">
    <div class="modal-box w-11/12 max-w-4xl max-h-[92vh] p-0 overflow-hidden flex flex-col rounded-2xl">
      <!-- Header -->
      <div class="px-6 py-5 border-b border-base-200 shrink-0 bg-base-100">
        <div class="flex items-start justify-between gap-4">
          <div class="flex items-start gap-3">
            <div class="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
              <ReceiptText class="size-5" />
            </div>
            <div>
              <h3 class="font-semibold text-base-content text-base leading-tight">
                {{ isEditMode ? 'แก้ไขใบเบิก' : 'สร้างใบเบิกสวัสดิการ/เงินสดย่อย' }}
              </h3>
              <div v-if="isEditMode" class="mt-1 flex items-center gap-2">
                <span class="text-xs text-base-content/50">เลขที่</span>
                <span class="text-xs font-semibold text-primary">{{ currentDocNum }}</span>
              </div>
              <p class="text-xs text-base-content/45 mt-1">กรอกรายการที่ต้องการเบิก แล้วกดบันทึก</p>
            </div>
          </div>
          <button type="button" class="btn btn-ghost btn-sm btn-square rounded-lg" @click="close">
            <X class="size-4" />
          </button>
        </div>
      </div>

      <form @submit.prevent="handleSubmit" class="flex flex-col flex-1 min-h-0">
        <div class="overflow-y-auto px-6 py-5 space-y-5 flex-1 bg-base-100">
          <!-- Document Information -->
          <section class="rounded-xl border border-base-200 bg-base-200/20 p-4">
            <div class="flex items-center gap-2 mb-4">
              <div class="flex size-7 items-center justify-center rounded-lg bg-base-200">
                <FileText class="size-3.5 text-base-content/60" />
              </div>
              <div>
                <h4 class="text-sm font-semibold">ข้อมูลเอกสาร</h4>
                <p class="text-[11px] text-base-content/40">ข้อมูลพื้นฐานของใบเบิก</p>
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                  วันที่เอกสาร <span class="text-error">*</span>
                </label>
                <input v-model="formData.docDate" type="date"
                    class="input input-bordered input-sm h-9 w-full bg-base-100 focus:border-primary" />
              </div>
             <div>
              <EmployeeSelect
                :model-value="selectedEmployeeId"
                label="ผู้ขอเบิก"
                :options="allowedEmployeeOptions"
                :disabled="allowedEmployeeOptions.length <= 1"
                @change="onEmployeeChange"
              />
            </div>
              <div class="sm:col-span-2">
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">หมายเหตุ</label>
                <textarea v-model="formData.remark" rows="2" placeholder="ระบุหมายเหตุเพิ่มเติม (ถ้ามี)"
                  class="textarea textarea-bordered w-full text-sm bg-base-100 resize-none focus:border-primary"></textarea>
              </div>
            </div>
          </section>

          <!-- Lines -->
          <section>
            <div class="flex items-center justify-between mb-3">
              <div>
                <div class="flex items-center gap-2">
                  <h4 class="text-sm font-semibold">รายการเบิก</h4>
                  <span class="badge badge-sm badge-ghost">{{ formData.lines.length }} รายการ</span>
                </div>
                <p class="text-[11px] text-base-content/40 mt-0.5">เพิ่มรายการสวัสดิการหรือเงินสดย่อยที่ต้องการเบิก</p>
              </div>
              <button type="button" class="btn btn-primary btn-sm gap-1.5 rounded-lg" @click="addLine">
                <Plus class="size-4" /> เพิ่มรายการ
              </button>
            </div>

            <div v-if="formData.lines.length === 0"
              class="rounded-xl border border-dashed border-base-300 bg-base-200/20 py-10 text-center">
              <div
                class="mx-auto flex size-11 items-center justify-center rounded-full bg-base-200 text-base-content/40">
                <ReceiptText class="size-5" />
              </div>
              <p class="text-sm font-medium mt-3">ยังไม่มีรายการเบิก</p>
              <p class="text-xs text-base-content/40 mt-1">กด "เพิ่มรายการ" เพื่อเริ่มกรอกข้อมูล</p>
            </div>

            <div v-else class="space-y-3">
              <div v-for="(line, idx) in formData.lines" :key="idx"
                class="group rounded-xl border border-base-200 bg-base-100 p-4 transition-all hover:border-primary/20 hover:shadow-sm">
                <div class="flex items-center justify-between mb-4">
                  <div class="flex items-center gap-2.5">
                    <div
                      class="flex size-7 items-center justify-center rounded-lg bg-primary/10 text-primary text-xs font-semibold">
                      {{ idx + 1 }}
                    </div>
                    <div>
                      <p class="text-xs font-semibold">รายการที่ {{ idx + 1 }}</p>
                      <p class="text-[11px] text-base-content/40">รายละเอียดการเบิก</p>
                    </div>
                  </div>
                  <button type="button"
                    class="btn btn-ghost btn-xs btn-square rounded-lg text-base-content/35 hover:bg-error/10 hover:text-error"
                    @click="removeLine(idx)">
                    <Trash2 class="size-3.5" />
                  </button>
                </div>

                <div class="mb-3">
                  <FormSelect v-model="line.benefitId" label="สวัสดิการ (ถ้ามี)" :options="filteredBenefitOptions"
                      placeholder="-- เงินสดย่อยทั่วไป --" clear-label="เงินสดย่อยทั่วไป"
                      @update:model-value="(id) => onBenefitChange(line, id)" />
                </div>

                <div class="mb-3">
                  <label class="block text-xs font-medium text-base-content/60 mb-1.5">รายละเอียด</label>
                  <textarea v-model="line.detail" rows="2" placeholder="เช่น ค่าเดินทาง, ค่าอาหาร, ค่าของใช้สำนักงาน"
                    class="textarea textarea-bordered w-full text-sm resize-none focus:border-primary"></textarea>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div>
                    <label class="block text-xs font-medium text-base-content/60 mb-1.5">ยอดเบิก</label>
                    <div class="relative">
                      <input v-model.number="line.amount" type="number" min="0" step="0.01"
                        class="input input-bordered input-sm w-full pr-10 focus:border-primary" />
                      <span
                        class="absolute right-3 top-1/2 -translate-y-1/2 text-[11px] text-base-content/35">บาท</span>
                    </div>
                  </div>
                  <div>
                    <label class="block text-xs font-medium text-base-content/60 mb-1.5">รหัสบัญชี</label>
                    <input v-model="line.accountCode" type="text" placeholder="Account Code"
                      class="input input-bordered input-sm w-full focus:border-primary" />
                  </div>
                </div>

                <!-- Limit Info -->
                <div class="mt-3 rounded-lg px-3 py-2.5 space-y-1.5"
                  :class="isOverGeneralLimit(line) ? 'bg-error/10' : 'bg-base-200/50'">
                  <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
                    <div class="flex items-center gap-2">
                      <Info class="size-3.5 text-base-content/40" />
                      <span class="text-xs text-base-content/50">วงเงินสิทธิ์</span>
                      <span class="text-xs font-semibold">{{ line.limitAmount.toLocaleString('th-TH') }} บาท</span>
                    </div>

                    <div v-if="line.benefitId && line.amount > line.limitAmount"
                      class="flex items-center gap-1.5 text-warning">
                      <TriangleAlert class="size-3.5" />
                      <span class="text-xs font-medium">
                        เกินวงเงินอ้างอิง {{ (line.amount - line.limitAmount).toLocaleString('th-TH') }} บาท
                        (ตรวจสอบก่อนอนุมัติ)
                      </span>
                    </div>

                    <div v-if="isOverGeneralLimit(line)" class="flex items-center gap-1.5 text-error">
                      <CircleAlert class="size-3.5" />
                      <span class="text-xs font-medium">
                        เกินวงเงินเงินสดย่อยทั่วไป {{ (line.amount - line.limitAmount).toLocaleString('th-TH') }} บาท —
                        บันทึกไม่ได้
                      </span>
                    </div>
                  </div>

                  <p v-if="line.condition" class="text-[11px] text-base-content/45 pl-5.5">เงื่อนไข: {{ line.condition
                    }}</p>
                </div>
              </div>
            </div>
          </section>
        <section class="rounded-xl border border-base-200 bg-base-200/20 p-4">
          <p class="text-sm font-semibold mb-3">ไฟล์แนบ</p>
          <AttachmentList doc-type="PrettyCash" :doc-number="currentDocNum || null" :target="dialogRef" />
        </section>
          <!-- Total -->
          <div class="flex items-center justify-between rounded-xl border border-primary/20 bg-primary/5 px-5 py-4">
            <div>
              <p class="text-xs text-base-content/50">ยอดรวมทั้งหมด</p>
              <p class="text-[11px] text-base-content/35 mt-0.5">จำนวนเงินที่ขอเบิก</p>
            </div>
            <div class="text-right">
              <span class="text-xl font-bold text-primary">{{ totalAmount.toLocaleString('th-TH') }}</span>
              <span class="text-xs text-base-content/50 ml-1">บาท</span>
            </div>
          </div>

          <!-- Error Banner -->
          <div v-if="errorMessage"
            class="flex items-center gap-2 rounded-lg bg-error/10 text-error px-4 py-2.5 text-xs font-medium">
            <CircleAlert class="size-4 shrink-0" />
            {{ errorMessage }}
          </div>
        </div>

        <!-- Footer -->
        <div class="flex items-center justify-between gap-3 px-6 py-4 border-t border-base-200 bg-base-100 shrink-0">
          <div class="hidden sm:block">
            <span class="text-xs text-base-content/40">ตรวจสอบข้อมูลก่อนกดบันทึก</span>
          </div>
          <div class="flex justify-end gap-2 ml-auto">
            <button type="button" class="btn btn-ghost btn-sm rounded-lg px-5" @click="close">ยกเลิก</button>
            <button type="submit" class="btn btn-primary btn-sm rounded-lg px-6" :disabled="isSubmitting">
              <span v-if="isSubmitting" class="loading loading-spinner loading-xs"></span>
              {{ isSubmitting ? 'กำลังบันทึก...' : 'บันทึก' }}
            </button>
          </div>
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
import { notify } from '../../utils/notify'
import { X, Plus, Trash2, ReceiptText, FileText, Info, TriangleAlert, CircleAlert } from 'lucide-vue-next'
import FormSelect from '../ui/FormSelect.vue'
import type { PrettyCash } from '../../types/PrettyCash'
import AttachmentList from '../attachment/AttachmentList.vue'
import EmployeeSelect, { type EmployeeOption } from '../../components/ui/EmployeeSelect.vue'
import { usePrettyCashStore } from '../../stores/prettyCashStore.ts'


interface OptionItem {
  id: string
  label: string
}
interface BenefitOption extends OptionItem {
  limitHint?: number
}
interface BenefitLimitInfo {
  limitAmount: number
  description: string | null
}

const props = defineProps<{
  isCreate: boolean
  employeeId: string
  employeeName: string
  benefitOptions: BenefitOption[]
  allowedEmployees: EmployeeOption[]
}>()

const prettyCashStore = usePrettyCashStore()
const emit = defineEmits<{ save: [data: any, isCreate: boolean] }>()

const dialogRef = ref<HTMLDialogElement>()
const isSubmitting = ref(false)
const currentDocNum = ref('')
const errorMessage = ref('')
const selectedEmployeeId = ref('')
const employeeBenefitLimits = ref<Record<string, BenefitLimitInfo>>({})
const isLoadingLimits = ref(false)
const isEditMode = ref(false)

const allowedEmployeeOptions = computed(() => {
  if (props.allowedEmployees.length === 0) {
    return [{ id: props.employeeId, empId: '', fullNameTh: props.employeeName }]
  }
  return props.allowedEmployees
})

const filteredBenefitOptions = computed(() => {
  const entitledIds = new Set(Object.keys(employeeBenefitLimits.value))
  return props.benefitOptions.filter(o => entitledIds.has(o.id))
})

interface LineForm {
  benefitId: string | null
  detail: string
  limitAmount: number
  condition: string | null
  amount: number
  qty: number
  accountCode: string
}

const GENERAL_PETTY_CASH_LIMIT = 5000

const defaultForm = () => ({
  docDate: new Date().toISOString().split('T')[0],
  remark: '',
  lines: [] as LineForm[],
})

const formData = ref(defaultForm())

const totalAmount = computed(() => formData.value.lines.reduce((sum, l) => sum + (l.amount || 0), 0))

function isOverGeneralLimit(line: LineForm) {
  return !line.benefitId && line.amount > line.limitAmount
}

function addLine() {
  formData.value.lines.push({
    benefitId: null,
    detail: '',
    limitAmount: GENERAL_PETTY_CASH_LIMIT,
    condition: null,
    amount: 0,
    qty: 1,
    accountCode: '',
  })
}

function removeLine(idx: number) {
  formData.value.lines.splice(idx, 1)
}

function onBenefitChange(line: LineForm, benefitId: string | null | undefined) {
  line.benefitId = benefitId ?? null
  const info = line.benefitId ? employeeBenefitLimits.value[line.benefitId] : null
  line.limitAmount = info?.limitAmount ?? GENERAL_PETTY_CASH_LIMIT
  line.condition = line.benefitId ? (info?.description ?? null) : null
}

async function loadBenefitLimitsFor(employeeId: string) {
  isLoadingLimits.value = true
  try {
    employeeBenefitLimits.value = await prettyCashStore.getBenefitLimitsFor(employeeId)

    for (const line of formData.value.lines) {
      if (line.benefitId) {
        const info = employeeBenefitLimits.value[line.benefitId]
        if (!info) {
          line.benefitId = null
          line.limitAmount = GENERAL_PETTY_CASH_LIMIT
          line.condition = null
        } else {
          line.limitAmount = info.limitAmount
          line.condition = info.description
        }
      }
    }
  } catch (err) {
    console.error('Failed to load benefit limits:', err)
    employeeBenefitLimits.value = {}
  } finally {
    isLoadingLimits.value = false
  }
}

async function onEmployeeChange(employeeId: string | null | undefined) {
  const newId = employeeId ?? props.employeeId
  if (newId === selectedEmployeeId.value) return

  if (formData.value.lines.length > 0) {
    const ok = await notify.confirm(
      'เปลี่ยนผู้ขอเบิกจะล้างรายการเบิกที่กรอกไว้ทั้งหมด เนื่องจากสวัสดิการแต่ละคนไม่เท่ากัน ยืนยันเปลี่ยนหรือไม่',
      'ยืนยันเปลี่ยนผู้ขอเบิก',
      dialogRef.value
    )
    if (!ok) return
  }

  selectedEmployeeId.value = newId
  formData.value.lines = []
  await loadBenefitLimitsFor(newId)
}

async function open(item?: PrettyCash | null) {
  errorMessage.value = ''
  currentDocNum.value = item?.docNum || ''
  isEditMode.value = !!item
  selectedEmployeeId.value = item?.employeeId || props.employeeId
  await loadBenefitLimitsFor(selectedEmployeeId.value)

  if (isEditMode.value && item) {
    formData.value = {
      docDate: item.docDate.split('T')[0],
      remark: item.remark ?? '',
      lines: item.lines.map(l => ({
        benefitId: l.benefitId,
        detail: l.detail,
        limitAmount: l.benefitId
          ? (employeeBenefitLimits.value[l.benefitId]?.limitAmount ?? l.limitAmount)
          : GENERAL_PETTY_CASH_LIMIT,
        condition: l.benefitId ? (employeeBenefitLimits.value[l.benefitId]?.description ?? null) : null,
        amount: l.amount,
        qty: l.qty,
        accountCode: l.accountCode ?? '',
      })),
    }
  } else {
    formData.value = defaultForm()
  }

  dialogRef.value?.showModal()
}

function close() {
  dialogRef.value?.close()
}

function getDialogEl() {
  return dialogRef.value ?? null
}

async function handleSubmit() {
  errorMessage.value = ''

  const overGeneralLimit = formData.value.lines.find(isOverGeneralLimit)
  if (overGeneralLimit) {
    errorMessage.value = `มีรายการเงินสดย่อยทั่วไปที่เกินวงเงิน ${GENERAL_PETTY_CASH_LIMIT.toLocaleString('th-TH')} บาท กรุณาแก้ไขก่อนบันทึก`
    return
  }

  isSubmitting.value = true
  try {
    const payload = {
      docDate: formData.value.docDate,
      employeeId: selectedEmployeeId.value,
      remark: formData.value.remark || null,
      lines: formData.value.lines.map(l => ({
        benefitId: l.benefitId,
        detail: l.detail,
        limitAmount: l.limitAmount || 0,
        amount: l.amount,
        qty: l.qty,
        accountCode: l.accountCode || null,
      })),
    }
    emit('save', payload, !isEditMode.value)
  } finally {
    isSubmitting.value = false
  }
}

defineExpose({ open, close, getDialogEl })
</script>