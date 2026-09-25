<template>
  <dialog ref="dialogRef" class="modal">
    <div class="modal-box w-11/12 max-w-lg p-0 overflow-hidden flex flex-col">
      <div class="flex items-start justify-between gap-3 px-6 py-5 border-b border-base-200 shrink-0">
        <div class="flex items-center gap-3">
          <div class="size-10 rounded-full bg-primary/10 text-primary flex items-center justify-center shrink-0">
            <FolderKanban class="size-5" />
          </div>
          <div>
            <h3 class="font-semibold text-base leading-tight">
              {{ isCreate ? 'เพิ่มแผนสวัสดิการ' : 'แก้ไขแผนสวัสดิการ' }}
            </h3>
            <p class="text-xs text-base-content/45 mt-0.5">กรอกข้อมูลให้ครบถ้วนแล้วกดบันทึก</p>
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
            <label class="block text-xs font-medium text-base-content/60 mb-1.5">
              ชื่อแผน (ไทย) <span class="text-error">*</span>
            </label>
            <input
              v-model="formData.nameTh"
              type="text"
              class="input input-bordered w-full text-sm"
              :class="errors.nameTh && 'input-error'"
            />
            <p v-if="errors.nameTh" class="text-xs text-error mt-1">{{ errors.nameTh }}</p>
          </div>
          <div>
            <label class="block text-xs font-medium text-base-content/60 mb-1.5">
              ชื่อแผน (อังกฤษ) <span class="text-error">*</span>
            </label>
            <input
              v-model="formData.nameEn"
              type="text"
              class="input input-bordered w-full text-sm"
              :class="errors.nameEn && 'input-error'"
            />
            <p v-if="errors.nameEn" class="text-xs text-error mt-1">{{ errors.nameEn }}</p>
          </div>
          <div>
            <label class="block text-xs font-medium text-base-content/60 mb-1.5">รายละเอียด</label>
            <textarea v-model="formData.description" rows="3" class="textarea textarea-bordered w-full text-sm"></textarea>
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
import { X, FolderKanban } from 'lucide-vue-next'
import type { BenefitPlan } from '../../types/Benefit'

const props = defineProps<{ isCreate: boolean }>()
const emit = defineEmits<{ save: [payload: any, isCreate: boolean] }>()

const dialogRef = ref<HTMLDialogElement>()
const isSubmitting = ref(false)
const errors = ref<Record<string, string>>({})

const defaultForm = { nameTh: '', nameEn: '', description: '', isActive: true }
const formData = ref({ ...defaultForm })

const validationSchema = yup.object({
  nameTh: yup.string().required('กรุณาระบุชื่อภาษาไทย'),
  nameEn: yup.string().required('กรุณาระบุชื่อภาษาอังกฤษ'),
})

function open(item?: BenefitPlan | null) {
  errors.value = {}
  formData.value = item
    ? { nameTh: item.nameTh, nameEn: item.nameEn, description: item.description ?? '', isActive: item.isActive }
    : { ...defaultForm }
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