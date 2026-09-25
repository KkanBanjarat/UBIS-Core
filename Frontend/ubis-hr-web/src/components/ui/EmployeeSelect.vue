<template>
  <div class="relative" ref="rootRef">
    <label v-if="label" class="block text-xs font-medium text-base-content/60 mb-1.5">
      {{ label }} <span v-if="required" class="text-error">*</span>
    </label>

    <!-- ช่องที่โชว์ตอนยังไม่เปิด Dropdown -->
    <button
      type="button"
      class="input input-bordered input-sm h-9 w-full text-sm flex items-center gap-2 text-left focus:border-primary"
      :class="disabled ? 'bg-base-200/50 cursor-not-allowed' : 'bg-base-100 cursor-pointer'"
      :disabled="disabled"
      @click="toggleDropdown"
    >
      <template v-if="selected">
        <div class="flex size-5 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary text-[10px] font-semibold">
          {{ initials(selected.fullNameTh) }}
        </div>
        <div class="min-w-0 flex-1">
          <p class="truncate leading-tight">{{ selected.fullNameTh }}</p>
        </div>
        <span class="text-xs text-base-content/40 shrink-0">{{ selected.empId }}</span>
      </template>
      <span v-else class="text-base-content/40">{{ placeholder ?? 'เลือกพนักงาน' }}</span>

      <ChevronDown class="size-3.5 text-base-content/40 shrink-0 ml-auto" :class="isOpen && 'rotate-180'" />
    </button>

    <Teleport :to="teleportTo">
      <div
        v-if="isOpen"
        ref="panelRef"
        :style="dropdownStyle"
        class="fixed z-[100] rounded-lg border border-base-200 bg-base-100 shadow-lg py-1 flex flex-col"
      >
        <div class="px-2 pb-1.5 pt-0.5 border-b border-base-200">
          <input
            ref="searchInputRef"
            v-model="query"
            type="text"
            placeholder="ค้นหาชื่อหรือรหัสพนักงาน..."
            class="input input-bordered input-sm w-full text-sm"
            @keydown.esc="closeDropdown"
          />
        </div>

        <ul class="overflow-y-auto max-h-64 py-1">
          <li v-for="opt in filteredOptions" :key="opt.id">
            <button
              type="button"
              class="w-full flex items-center gap-2.5 px-3 py-2 text-left hover:bg-base-200"
              :class="opt.id === modelValue && 'bg-primary/5'"
              @click="select(opt)"
            >
              <div class="flex size-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary text-xs font-semibold">
                {{ initials(opt.fullNameTh) }}
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-sm font-medium truncate" :class="opt.id === modelValue && 'text-primary'">
                  {{ opt.fullNameTh }}
                </p>
                <p class="text-xs text-base-content/40 truncate">
                  {{ opt.empId }}<span v-if="opt.positionNameTh"> · {{ opt.positionNameTh }}</span>
                </p>
              </div>
            </button>
          </li>
          <li v-if="filteredOptions.length === 0" class="px-3 py-4 text-center text-xs text-base-content/40">
            ไม่พบพนักงาน
          </li>
        </ul>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { ChevronDown } from 'lucide-vue-next'

export interface EmployeeOption {
  id: string
  empId: string
  fullNameTh: string
  positionNameTh?: string | null
}

const props = defineProps<{
  options: EmployeeOption[]
  label?: string
  required?: boolean
  disabled?: boolean
  placeholder?: string
}>()

const modelValue = defineModel<string | null>()
const emit = defineEmits<{ change: [id: string | null] }>()

const rootRef = ref<HTMLElement>()
const panelRef = ref<HTMLElement>()
const searchInputRef = ref<HTMLInputElement>()

const query = ref('')
const isOpen = ref(false)
const position = ref({ top: 0, left: 0, width: 0 })

const selected = computed(() => props.options.find(o => o.id === modelValue.value) ?? null)

const filteredOptions = computed(() => {
  if (!query.value) return props.options
  const q = query.value.toLowerCase()
  return props.options.filter(
    o => o.fullNameTh.toLowerCase().includes(q) || o.empId.toLowerCase().includes(q)
  )
})

function initials(name: string) {
  return name?.charAt(0)?.toUpperCase() ?? '?'
}

const teleportTargetEl = ref<HTMLElement | null>(null)
const teleportTo = computed<any>(() => teleportTargetEl.value ?? 'body')

const dropdownStyle = computed(() => ({
  top: `${position.value.top}px`,
  left: `${position.value.left}px`,
  width: `${position.value.width}px`,
}))

function computePosition() {
  const rect = rootRef.value?.querySelector('button')?.getBoundingClientRect()
  if (!rect) return
  position.value = { top: rect.bottom + 4, left: rect.left, width: rect.width }
}

function toggleDropdown() {
  if (props.disabled) return
  isOpen.value ? closeDropdown() : openDropdown()
}

function openDropdown() {
  computePosition()
  isOpen.value = true
  query.value = ''
  nextTick(() => searchInputRef.value?.focus())
}

function closeDropdown() {
  isOpen.value = false
}

function select(opt: EmployeeOption) {
  modelValue.value = opt.id
  emit('change', opt.id)
  closeDropdown()
}

function handleClickOutside(e: MouseEvent) {
  const target = e.target as Node
  if (rootRef.value?.contains(target)) return
  if (panelRef.value?.contains(target)) return
  if (isOpen.value) closeDropdown()
}

function handleScroll(e: Event) {
  if (!isOpen.value) return
  const target = e.target as Node
  if (panelRef.value && (panelRef.value === target || panelRef.value.contains(target))) return
  computePosition()
}

onMounted(() => {
  teleportTargetEl.value = rootRef.value?.closest('dialog') ?? null
  document.addEventListener('click', handleClickOutside)
  window.addEventListener('scroll', handleScroll, true)
  window.addEventListener('resize', computePosition)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
  window.removeEventListener('scroll', handleScroll, true)
  window.removeEventListener('resize', computePosition)
})
</script>