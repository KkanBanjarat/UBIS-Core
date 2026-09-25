<template>
  <dialog ref="dialogRef" class="modal">
    <div class="modal-box w-11/12 max-w-lg p-0 overflow-hidden flex flex-col">
      <div class="flex items-start justify-between gap-3 px-6 py-5 border-b border-base-200 shrink-0">
        <div class="flex items-center gap-3">
          <div class="size-10 rounded-full bg-primary/10 text-primary flex items-center justify-center shrink-0">
            <ListChecks class="size-5" />
          </div>
          <div>
            <h3 class="font-semibold text-base leading-tight">
              {{ isCreate ? 'เพิ่มสวัสดิการในแผน' : 'แก้ไขสวัสดิการในแผน' }}
            </h3>
            <p class="text-xs text-base-content/45 mt-0.5">แผน: {{ planLabel }}</p>
          </div>
        </div>
        <button type="button" class="btn btn-ghost btn-sm btn-square" @click="close">
          <X class="size-4" />
        </button>
      </div>

      <div v-if="Object.keys(errors).length > 0" class="bg-error/10 border-b border-error/30 px-6 py-3">
        <p class="text-sm text-error font-medium">❌ กรุณาระบุข้อมูลให้ครบถ้วน</p>
      </div>

      <form @submit.prevent="handleSubmit" class="flex flex-col flex-1 min-h-0">
        <div class="overflow-y-auto px-6 py-5 space-y-4 flex-1">
          <div>
            <FormSelect
              v-model="formData.benefitId"
              label="สวัสดิการ"
              :required="true"
              :options="benefitOptions"
              placeholder="เลือกสวัสดิการ"
            />
            <p v-if="errors.benefitId" class="text-xs text-error mt-1">{{ errors.benefitId }}</p>
          </div>
          <div>
            <label class="block text-xs font-medium text-base-content/60 mb-1.5">
              วงเงินที่เบิกได้ (บาท) <span class="text-error">*</span>
            </label>
            <input
              v-model.number="formData.limitAmount"
              type="number"
              min="0"
              step="0.01"
              class="input input-bordered w-full text-sm"
              :class="errors.limitAmount && 'input-error'"
            />
            <p v-if="errors.limitAmount" class="text-xs text-error mt-1">{{ errors.limitAmount }}</p>
          </div>
          <div>
            <label class="block text-xs font-medium text-base-content/60 mb-1.5">รายละเอียด/เงื่อนไข</label>
            <textarea
              v-model="formData.description"
              rows="2"
              placeholder="เช่น สำหรับพนักงานระดับ L1-L5"
              class="textarea textarea-bordered w-full text-sm"
            ></textarea>
          </div>
          <label class="flex items-center gap-2 cursor-pointer pt-1">
            <input v-model="formData.isActive" type="checkbox" class="checkbox checkbox-sm" />
            <span class="text-sm">เปิดใช้งาน</span>
          </label>
        </div>

        <div class="flex items-center justify-end gap-3 px-6 py-4 border-t border-base-200 shrink-0">
          <button type="button" class="btn btn-ghost border border-base-300" @click="close">ยกเลิก</button>
          <button type="submit" class="btn btn-primary" :disabled="isSubmitting">
            {{ isSubmitting ? 'กำลังบันทึก...' : 'บันทึก' }}
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
import { ref } from 'vue'
import * as yup from 'yup'
import { X, ListChecks } from 'lucide-vue-next'
import FormSelect from '../ui/FormSelect.vue'
import type { BenefitPlanItem } from '../../types/Benefit'

interface OptionItem { id: string; label: string }

const props = defineProps<{
  isCreate: boolean
  benefitOptions: OptionItem[]
}>()
const emit = defineEmits<{ save: [payload: any, isCreate: boolean] }>()

const dialogRef = ref<HTMLDialogElement>()
const isSubmitting = ref(false)
const errors = ref<Record<string, string>>({})
const planLabel = ref('')

const defaultForm = {
  benefitPlanId: '',
  benefitId: null as string | null,
  limitAmount: 0,
  description: '',
  isActive: true,
}
const formData = ref({ ...defaultForm })

const validationSchema = yup.object({
  benefitId: yup.string().nullable().required('กรุณาเลือกสวัสดิการ'),
  limitAmount: yup.number().min(0, 'ต้องไม่ติดลบ').required('กรุณาระบุวงเงิน'),
})

// planId = แผนที่ Fix ไว้จาก Context (แผนที่กำลังเลือกอยู่ในหน้า Master-Detail)
// planLabelText = ข้อความแสดงชื่อแผน (แสดงอย่างเดียว แก้ไขไม่ได้)
function open(planId: string, planLabelText: string, item?: BenefitPlanItem | null) {
  errors.value = {}
  planLabel.value = planLabelText
  formData.value = item
    ? {
        benefitPlanId: planId,
        benefitId: item.benefitId,
        limitAmount: item.limitAmount,
        description: item.description ?? '',
        isActive: item.isActive,
      }
    : { ...defaultForm, benefitPlanId: planId }
  dialogRef.value?.showModal()
}
function close() {
  dialogRef.value?.close()
}
function getDialogEl() {
  return dialogRef.value ?? null
}

async function handleSubmit() {
  isSubmitting.value = true
  errors.value = {}
  try {
    await validationSchema.validate(formData.value, { abortEarly: false })
    emit('save', { ...formData.value, description: formData.value.description || null }, props.isCreate)
  } catch (err: any) {
    if (err.inner) err.inner.forEach((e: any) => { if (e.path) errors.value[e.path] = e.message })
  } finally {
    isSubmitting.value = false
  }
}

defineExpose({ open, close, getDialogEl })
</script>