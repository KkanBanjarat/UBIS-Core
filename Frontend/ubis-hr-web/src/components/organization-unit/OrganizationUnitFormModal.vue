<template>
  <dialog ref="dialogRef" class="modal">
    <div class="modal-box w-11/12 max-w-lg p-0 overflow-hidden flex flex-col">
      <div class="flex items-start justify-between gap-3 px-6 py-5 border-b border-base-200 shrink-0">
        <div class="flex items-center gap-3">
          <div class="size-10 rounded-full bg-primary/10 text-primary flex items-center justify-center shrink-0">
            <Network class="size-5" />
          </div>
          <div>
            <h3 class="font-semibold text-base leading-tight">
              {{ isCreate ? 'เพิ่มหน่วยงาน' : 'แก้ไขหน่วยงาน' }}
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
            <FormSelect
              v-model="formData.type"
              label="ประเภทหน่วยงาน"
              :required="true"
              :options="typeOptions"
              placeholder="เลือกประเภทหน่วยงาน"
            />
            <p v-if="errors.type" class="text-xs text-error mt-1">{{ errors.type }}</p>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                ชื่อ (ไทย) <span class="text-error">*</span>
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
                ชื่อ (อังกฤษ) <span class="text-error">*</span>
              </label>
              <input
                v-model="formData.nameEn"
                type="text"
                class="input input-bordered w-full text-sm"
                :class="errors.nameEn && 'input-error'"
              />
              <p v-if="errors.nameEn" class="text-xs text-error mt-1">{{ errors.nameEn }}</p>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-medium text-base-content/60 mb-1.5">รหัส (Code)</label>
              <input v-model="formData.code" type="text" class="input input-bordered w-full text-sm" />
            </div>
            <div>
              <label class="block text-xs font-medium text-base-content/60 mb-1.5">ชื่อย่อ (Short Name)</label>
              <input v-model="formData.shortName" type="text" class="input input-bordered w-full text-sm" />
            </div>
          </div>
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
import { X, Network } from 'lucide-vue-next'
import FormSelect from '../ui/FormSelect.vue'
import type { OrganizationUnit } from '../../types/OrganizationUnit'

interface OptionItem { id: string; label: string }

const props = defineProps<{
  isCreate: boolean
  typeOptions: OptionItem[] // ← ส่งเข้ามาจาก Parent (โหลดจาก organizationleveltypes ตัด Company ออกแล้ว)
}>()

const emit = defineEmits<{
  save: [payload: any, isCreate: boolean]
}>()

const dialogRef = ref<HTMLDialogElement>()
const isSubmitting = ref(false)
const errors = ref<Record<string, string>>({})

const defaultForm = {
  code: '',
  nameTh: '',
  nameEn: '',
  shortName: '',
  type: null as string | null,
}
const formData = ref({ ...defaultForm })
function getDialogEl() {
  return dialogRef.value ?? null
}
const validationSchema = yup.object({
  type: yup.string().nullable().required('กรุณาเลือกประเภทหน่วยงาน'),
  nameTh: yup.string().required('กรุณาระบุชื่อภาษาไทย'),
  nameEn: yup.string().required('กรุณาระบุชื่อภาษาอังกฤษ'),
})

function open(item?: OrganizationUnit | null) {
  errors.value = {}
  formData.value = item
    ? {
        code: item.code ?? '',
        nameTh: item.nameTh,
        nameEn: item.nameEn,
        shortName: item.shortName ?? '',
        type: item.type,
      }
    : { ...defaultForm }
  dialogRef.value?.showModal()
}

function close() {
  dialogRef.value?.close()
}

async function handleSubmit() {
  isSubmitting.value = true
  errors.value = {}
  try {
    await validationSchema.validate(formData.value, { abortEarly: false })
    emit('save', {
      code: formData.value.code || null,
      nameTh: formData.value.nameTh,
      nameEn: formData.value.nameEn,
      shortName: formData.value.shortName || null,
      type: formData.value.type,
    }, props.isCreate)
    // ไม่ปิด Modal ที่นี่ — ให้ Parent สั่งปิดเองหลัง Save API สำเร็จเท่านั้น
  } catch (err: any) {
    if (err.inner && err.inner.length > 0) {
      err.inner.forEach((e: any) => {
        if (e.path) errors.value[e.path] = e.message
      })
    }
  } finally {
    isSubmitting.value = false
  }
}

defineExpose({ open, close, getDialogEl })
</script>