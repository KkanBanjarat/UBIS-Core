<template>
  <dialog ref="dialogRef" class="modal">
    <div class="modal-box w-11/12 max-w-lg p-0 overflow-hidden flex flex-col rounded-2xl">
      <!-- Header -->
      <div class="px-6 py-5 border-b border-base-200 shrink-0 flex items-start justify-between gap-4">
        <div class="flex items-start gap-3">
          <div class="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
            <UserCog class="size-5" />
          </div>
          <div>
            <h3 class="font-semibold text-base-content text-base leading-tight">เพิ่มผู้ดูแลสาขา</h3>
            <p class="text-xs text-base-content/45 mt-1">{{ branchName }}</p>
          </div>
        </div>
        <button type="button" class="btn btn-ghost btn-sm btn-square rounded-lg" @click="close">
          <X class="size-4" />
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="flex flex-col flex-1 min-h-0">
        <div class="overflow-y-auto px-6 py-5 space-y-4 flex-1">
          <EmployeeSelect
            :model-value="form.employeeId"
            label="พนักงาน"
            required
            :options="employeeOptions"
            @change="(id) => (form.employeeId = id ?? null)"
          />

          <div class="rounded-lg bg-info/5 border border-info/20 px-3 py-2.5">
            <p class="text-xs text-base-content/60">
              ระบบจะผูกกับบัญชีผู้ใช้งานให้อัตโนมัติจากรหัสพนักงาน
              หากพนักงานยังไม่มีบัญชีผู้ใช้งานจะเพิ่มไม่ได้
            </p>
          </div>

          <label class="flex items-start gap-2.5 cursor-pointer rounded-lg border border-base-200 px-3.5 py-3 hover:bg-base-200/30 transition-colors">
            <input v-model="form.isPrimary" type="checkbox" class="checkbox checkbox-sm checkbox-primary mt-0.5" />
            <div>
              <p class="text-sm font-medium">ตั้งเป็นผู้ดูแลหลักของสาขานี้</p>
              <p class="text-xs text-base-content/45 mt-0.5">
                ผู้ดูแลหลักจะเป็นผู้อนุมัติในขั้นตอนที่ใช้ประเภท "ผู้ดูแลสาขา"
                (1 สาขามีได้คนเดียว ระบบจะปลดคนเดิมให้อัตโนมัติ)
              </p>
            </div>
          </label>

          <div
            v-if="errorMessage"
            class="flex items-start gap-2 rounded-lg bg-error/10 text-error px-4 py-2.5 text-xs font-medium"
          >
            <CircleAlert class="size-4 shrink-0 mt-0.5" />
            {{ errorMessage }}
          </div>
        </div>

        <!-- Footer -->
        <div class="flex justify-end gap-2 px-6 py-4 border-t border-base-200 bg-base-100 shrink-0">
          <button type="button" class="btn btn-ghost btn-sm rounded-lg px-5" @click="close">ยกเลิก</button>
          <button
            type="submit"
            class="btn btn-primary btn-sm rounded-lg px-6"
            :disabled="isSubmitting || !form.employeeId"
          >
            <span v-if="isSubmitting" class="loading loading-spinner loading-xs"></span>
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
import EmployeeSelect, { type EmployeeOption } from '../ui/EmployeeSelect.vue'
import { UserCog, X, CircleAlert } from 'lucide-vue-next'

defineProps<{ employeeOptions: EmployeeOption[] }>()
const emit = defineEmits<{ save: [data: { employeeId: string; branchId: string; isPrimary: boolean }] }>()

const dialogRef = ref<HTMLDialogElement>()
const isSubmitting = ref(false)
const errorMessage = ref('')
const branchName = ref('')

const form = ref({
  employeeId: null as string | null,
  branchId: '',
  isPrimary: false,
})

function open(branchId: string, name: string, suggestPrimary = false) {
  errorMessage.value = ''
  branchName.value = name
  form.value = {
    employeeId: null,
    branchId,
    // ถ้าสาขานี้ยังไม่มีผู้ดูแลหลัก ให้ติ๊กไว้ให้เลย ลดโอกาสลืม
    isPrimary: suggestPrimary,
  }
  dialogRef.value?.showModal()
}

function close() {
  dialogRef.value?.close()
}

function setError(msg: string) {
  errorMessage.value = msg
  isSubmitting.value = false
}

async function handleSubmit() {
  if (!form.value.employeeId) return

  errorMessage.value = ''
  isSubmitting.value = true

  try {
    emit('save', {
      employeeId: form.value.employeeId,
      branchId: form.value.branchId,
      isPrimary: form.value.isPrimary,
    })
  } finally {
    isSubmitting.value = false
  }
}

defineExpose({ open, close, setError })
</script>