<template>
  <dialog ref="dialogRef" class="modal">
    <div class="modal-box flex w-11/12 max-w-2xl flex-col overflow-hidden rounded-2xl p-0">
      <!-- Header -->
      <div class="flex shrink-0 items-start justify-between border-b border-base-200 bg-base-100 px-6 py-4">
        <div class="flex items-center gap-3">
          <div class="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
            <ListTree class="size-5" />
          </div>

          <div>
            <h3 class="text-base font-semibold leading-tight text-base-content">
              {{ editingId ? 'แก้ไขเมนู' : 'เพิ่มเมนู' }}
            </h3>

            <p class="mt-1 text-xs text-base-content/45">
              Category
              <span class="mx-1">→</span>
              Module
              <span class="mx-1">→</span>
              Page
            </p>
          </div>
        </div>

        <button type="button"
          class="btn btn-ghost btn-sm btn-square rounded-xl text-base-content/50 hover:bg-base-200 hover:text-base-content"
          @click="close">
          <X class="size-4" />
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="flex min-h-0 flex-1 flex-col">
        <!-- Content -->
        <div class="flex-1 space-y-5 overflow-y-auto px-6 py-5">
          <!-- ==================== Menu Structure ==================== -->
          <section class="space-y-3">
            <div>
              <h4 class="text-sm font-semibold text-base-content">
                โครงสร้างเมนู
              </h4>
              <p class="mt-0.5 text-xs text-base-content/40">
                กำหนดประเภทและตำแหน่งของเมนูในระบบ
              </p>
            </div>

            <div class="rounded-xl border border-base-200 bg-base-50/50 p-4">
              <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <!-- Type -->
                <div>
                  <label class="mb-1.5 block text-xs font-medium text-base-content/60">
                    ชนิด
                  </label>

                  <select v-model="form.nodeType"
                    class="select select-sm h-9 w-full rounded-lg border-base-300 bg-base-100 text-sm"
                    @change="onTypeChange">
                    <option value="Category">
                      Category (หมวดบนสุด)
                    </option>
                    <option value="Module">
                      Module (กลุ่มย่อย)
                    </option>
                    <option value="Page">
                      Page (หน้าที่เปิดได้)
                    </option>
                  </select>
                </div>

                <!-- Parent -->
                <div v-if="form.nodeType !== 'Category'">
                  <label class="mb-1.5 block text-xs font-medium text-base-content/60">
                    เมนูแม่
                  </label>

                  <select v-model="form.parentId"
                    class="select select-sm h-9 w-full rounded-lg border-base-300 bg-base-100 text-sm">
                    <option :value="null">
                      — ไม่มี (อยู่ระดับบนสุด) —
                    </option>

                    <option v-for="p in parentOptions" :key="p.id" :value="p.id">
                      {{ p.nodeType === 'Module' ? '　└ ' : '' }}
                      {{ p.label }}
                      ({{ p.nodeType }})
                    </option>
                  </select>
                </div>
              </div>
            </div>
          </section>

          <!-- ==================== Basic Information ==================== -->
          <section class="space-y-3">
            <div>
              <h4 class="text-sm font-semibold text-base-content">
                ข้อมูลเมนู
              </h4>
              <p class="mt-0.5 text-xs text-base-content/40">
                ข้อมูลที่จะแสดงและใช้ระบุเมนูในระบบ
              </p>
            </div>

            <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <!-- Code -->
              <div>
                <label class="mb-1.5 block text-xs font-medium text-base-content/60">
                  Code
                  <span class="text-error">*</span>
                </label>

                <input v-model="form.code" type="text"
                  class="input input-sm h-9 w-full rounded-lg border-base-300 bg-base-100 font-mono text-sm"
                  placeholder="benefit-claim" />
              </div>

              <!-- Label -->
              <div>
                <label class="mb-1.5 block text-xs font-medium text-base-content/60">
                  ชื่อที่แสดง
                  <span class="text-error">*</span>
                </label>

                <input v-model="form.label" type="text"
                  class="input input-sm h-9 w-full rounded-lg border-base-300 bg-base-100 text-sm"
                  placeholder="เช่น เบิกสวัสดิการ" />
              </div>
            </div>
          </section>

          <!-- ==================== Page Settings ==================== -->
          <section v-if="form.nodeType === 'Page'" class="space-y-3">
            <div>
              <h4 class="text-sm font-semibold text-base-content">
                การนำทาง
              </h4>
              <p class="mt-0.5 text-xs text-base-content/40">
                กำหนดเส้นทางและ Component สำหรับหน้าเมนูนี้
              </p>
            </div>

            <div class="rounded-xl border border-base-200 bg-base-50/50 p-4 space-y-3">
              <!-- Path -->
              <div>
                <label class="mb-1.5 block text-xs font-medium text-base-content/60">
                  Path
                  <span class="text-error">*</span>
                </label>

                <input v-model="form.path" type="text"
                  class="input input-sm h-9 w-full rounded-lg border-base-300 bg-base-100 font-mono text-sm"
                  placeholder="/settings/menus" />
              </div>

              <!-- Component Path -->
              <div>
                <label class="mb-1.5 block text-xs font-medium text-base-content/60">
                  ComponentPath
                  <span class="text-error">*</span>
                </label>

                <input v-model="form.componentPath" type="text"
                  class="input input-sm h-9 w-full rounded-lg border-base-300 bg-base-100 font-mono text-sm"
                  placeholder="views/settings/MenuAdminView.vue" />

                <p class="mt-1.5 text-[11px] leading-relaxed text-base-content/40">
                  ต้องมีไฟล์นี้อยู่จริงใน
                  <code class="rounded bg-base-200 px-1 py-0.5 font-mono">
                    src/views
                  </code>
                  ไม่เช่นนั้นหน้าจะเปิดไม่ได้
                </p>
              </div>

              <!-- Permission -->
              <div>
                <label class="mb-1.5 block text-xs font-medium text-base-content/60">
                  Permission Code
                  <span class="font-normal text-base-content/35">
                    (resource key)
                  </span>
                </label>

                <input v-model="form.permissionCode" type="text"
                  class="input input-sm h-9 w-full rounded-lg border-base-300 bg-base-100 font-mono text-sm"
                  placeholder="menu" />
              </div>
            </div>
          </section>

          <!-- ==================== Display ==================== -->
          <section class="space-y-3">
            <div>
              <h4 class="text-sm font-semibold text-base-content">
                การแสดงผล
              </h4>
              <p class="mt-0.5 text-xs text-base-content/40">
                กำหนด Icon ลำดับ และสถานะการแสดงเมนู
              </p>
            </div>

            <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <!-- Icon -->
              <div>
                <label class="mb-1.5 block text-xs font-medium text-base-content/60">
                  Icon
                </label>

                <input v-model="form.icon" type="text"
                  class="input input-sm h-9 w-full rounded-lg border-base-300 bg-base-100 font-mono text-sm"
                  placeholder="Gift" />

                <p class="mt-1.5 text-[11px] text-base-content/40">
                  ชื่อต้องมีใน
                  <code class="rounded bg-base-200 px-1 font-mono">
                    utils/menuIcons.ts
                  </code>
                </p>
              </div>

              <!-- Sort -->
              <div>
                <label class="mb-1.5 block text-xs font-medium text-base-content/60">
                  ลำดับ
                </label>

                <input v-model.number="form.sortOrder" type="number"
                  class="input input-sm h-9 w-full rounded-lg border-base-300 bg-base-100 text-sm" />
              </div>
            </div>

            <!-- Active -->
            <label
              class="flex cursor-pointer items-center justify-between rounded-xl border border-base-200 bg-base-50/50 px-4 py-3 transition-colors hover:bg-base-200/40">
              <div class="flex items-center gap-3">
                <div class="flex size-8 items-center justify-center rounded-lg" :class="form.isActive
                    ? 'bg-success/10 text-success'
                    : 'bg-base-200 text-base-content/40'
                  ">
                  <span class="size-2 rounded-full bg-current"></span>
                </div>

                <div>
                  <p class="text-sm font-medium text-base-content">
                    เปิดใช้งานเมนู
                  </p>

                  <p class="mt-0.5 text-xs text-base-content/40">
                    แสดงเมนูนี้ให้ผู้ใช้เห็นตามสิทธิ์ Role
                  </p>
                </div>
              </div>

              <input v-model="form.isActive" type="checkbox" class="toggle toggle-sm toggle-primary" />
            </label>
          </section>

          <!-- Error -->
          <div v-if="errorMessage"
            class="flex items-start gap-2.5 rounded-xl border border-error/10 bg-error/5 px-4 py-3 text-xs font-medium text-error">
            <CircleAlert class="mt-0.5 size-4 shrink-0" />
            <span>{{ errorMessage }}</span>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex shrink-0 items-center justify-between border-t border-base-200 bg-base-100 px-6 py-3.5">
          <p class="hidden text-[11px] text-base-content/35 sm:block">
            <span class="text-error">*</span>
            จำเป็นต้องกรอก
          </p>

          <div class="ml-auto flex gap-2">
            <button type="button" class="btn btn-ghost btn-sm h-9 rounded-lg px-5 text-sm" @click="close">
              ยกเลิก
            </button>

            <button type="submit" class="btn btn-primary btn-sm h-9 rounded-lg px-6 text-sm shadow-sm shadow-primary/20"
              :disabled="isSubmitting">
              <span v-if="isSubmitting" class="loading loading-spinner loading-xs"></span>

              {{ isSubmitting ? 'กำลังบันทึก...' : 'บันทึกเมนู' }}
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
import { ListTree, X, CircleAlert } from 'lucide-vue-next'
import type {
  MenuAdmin,
  MenuNodeType,
  SaveMenuPayload
} from '../../types/MenuAdmin'

const props = defineProps<{
  menus: MenuAdmin[]
}>()

const emit = defineEmits<{
  save: [
    data: {
      id: string | null
      payload: SaveMenuPayload
    }
  ]
}>()

const dialogRef = ref<HTMLDialogElement>()
const isSubmitting = ref(false)
const errorMessage = ref('')
const editingId = ref<string | null>(null)

const emptyForm = () => ({
  nodeType: 'Page' as MenuNodeType,
  parentId: null as string | null,
  code: '',
  label: '',
  path: '',
  componentPath: '',
  icon: '',
  permissionCode: '',
  sortOrder: 0,
  isActive: true,
})

const form = ref(emptyForm())

// เมนูแม่ที่เลือกได้:
// Module → Category เท่านั้น
// Page → Category + Module
// ตัดตัวเองออก
const parentOptions = computed(() =>
  props.menus
    .filter((m) => m.id !== editingId.value)
    .filter((m) =>
      form.value.nodeType === 'Module'
        ? m.nodeType === 'Category'
        : m.nodeType === 'Category' ||
        m.nodeType === 'Module',
    )
    .sort(
      (a, b) => a.sortOrder - b.sortOrder,
    ),
)

function onTypeChange() {
  if (form.value.nodeType === 'Category') {
    form.value.parentId = null
  } else if (
    !parentOptions.value.some(
      (p) => p.id === form.value.parentId,
    )
  ) {
    form.value.parentId = null
  }
}

function open(
  item?: MenuAdmin,
  defaultParentId: string | null = null,
) {
  errorMessage.value = ''
  isSubmitting.value = false

  if (item) {
    editingId.value = item.id

    form.value = {
      nodeType: item.nodeType,
      parentId: item.parentId,
      code: item.code,
      label: item.label,
      path: item.path ?? '',
      componentPath: item.componentPath ?? '',
      icon: item.icon ?? '',
      permissionCode: item.permissionCode ?? '',
      sortOrder: item.sortOrder,
      isActive: item.isActive,
    }
  } else {
    editingId.value = null

    form.value = {
      ...emptyForm(),
      parentId: defaultParentId,
    }
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

function handleSubmit() {
  const f = form.value

  errorMessage.value = ''

  if (!f.code.trim()) {
    return (errorMessage.value = 'กรุณาระบุ Code')
  }

  if (!f.label.trim()) {
    return (errorMessage.value = 'กรุณาระบุชื่อที่แสดง')
  }

  if (f.nodeType === 'Page') {
    if (!f.path.trim()) {
      return (errorMessage.value = 'Page ต้องระบุ Path')
    }

    if (!f.componentPath.trim()) {
      return (errorMessage.value = 'Page ต้องระบุ ComponentPath')
    }
  }

  isSubmitting.value = true

  const isPage = f.nodeType === 'Page'

  emit('save', {
    id: editingId.value,

    payload: {
      parentId:
        f.nodeType === 'Category'
          ? null
          : f.parentId,

      nodeType: f.nodeType,

      code: f.code.trim(),

      label: f.label.trim(),

      path: isPage
        ? f.path.trim()
        : null,

      componentPath: isPage
        ? f.componentPath.trim()
        : null,

      icon: f.icon.trim() || null,

      permissionCode: isPage
        ? f.permissionCode.trim() || null
        : null,

      sortOrder:
        Number(f.sortOrder) || 0,

      isActive: f.isActive,
    },
  })
}

defineExpose({
  open,
  close,
  setError,
})
</script>