<template>
  <div class="relative" ref="rootRef">
    <label v-if="label" class="block text-xs font-medium text-base-content/60 mb-1.5">
      {{ label }} <span v-if="required" class="text-error">*</span>
    </label>

    <div class="relative">
      <input
        ref="inputRef"
        v-model="query"
        type="text"
        :placeholder="placeholderText"
        :disabled="disabled"
        class="input input-bordered w-full text-sm pr-8"
        autocomplete="off"
        @focus="openDropdown"
        @click="openDropdown"
        @input="onInput"
        @keydown.down.prevent="moveHighlight(1)"
        @keydown.up.prevent="moveHighlight(-1)"
        @keydown.enter.prevent="selectHighlighted"
        @keydown.esc.stop.prevent="closeDropdown"
      />
      <button
        v-if="model && !disabled"
        type="button"
        class="absolute right-2 top-1/2 -translate-y-1/2 text-base-content/40 hover:text-error"
        @click.stop="clearSelection"
      >
        <X class="size-3.5" />
      </button>
      <ChevronDown
        v-else
        class="absolute right-2 top-1/2 -translate-y-1/2 size-3.5 text-base-content/40 pointer-events-none"
      />
    </div>

    <Teleport :to="teleportTo">
      <ul
        v-if="isOpen"
        ref="panelRef"
        :style="dropdownStyle"
        class="fixed z-[100] pointer-events-auto max-h-56 overflow-y-auto rounded-lg border border-base-200 bg-base-100 shadow-lg py-1 text-sm"
      >
        <li v-if="!required">
          <button
            type="button"
            class="w-full text-left px-3 py-2 hover:bg-base-200"
            :class="!model && 'text-primary font-medium'"
            @click="select(null)"
          >
            {{ clearLabel }}
          </button>
        </li>
        <li v-for="(opt, i) in filteredOptions" :key="opt.id" data-opt>
          <button
            type="button"
            class="w-full text-left px-3 py-2 hover:bg-base-200"
            :class="[opt.id === model ? 'text-primary font-medium' : '', i === highlightIndex ? 'bg-base-200' : '']"
            @click="select(opt.id)"
            @mouseenter="highlightIndex = i"
          >
            {{ opt.label }}
          </button>
        </li>
        <li v-if="filteredOptions.length === 0" class="px-3 py-2 text-base-content/40">ไม่พบข้อมูล</li>
      </ul>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { ChevronDown, X } from 'lucide-vue-next'

interface OptionItem {
  id: string
  label: string
}

const props = defineProps<{
  options: OptionItem[]
  label?: string
  required?: boolean
  disabled?: boolean
  placeholder?: string
  clearLabel?: string
  appendTo?: string
}>()

const model = defineModel<string | null>()

const rootRef = ref<HTMLElement>()
const inputRef = ref<HTMLInputElement>()
const panelRef = ref<HTMLElement>()

const query = ref('')
const isOpen = ref(false)
const highlightIndex = ref(-1)
const position = ref({ top: 0, left: 0, width: 0 })

const clearLabel = computed(() => props.clearLabel ?? 'ทั้งหมด')
const placeholderText = computed(
  () => props.placeholder ?? (props.required ? `เลือก${props.label ?? ''}` : clearLabel.value)
)

const selectedLabel = computed(() => props.options.find(o => o.id === model.value)?.label ?? '')

const filteredOptions = computed(() => {
  if (!query.value) return props.options
  const q = query.value.toLowerCase()
  return props.options.filter(o => o.label.toLowerCase().includes(q))
})

watch(selectedLabel, (val) => {
  if (!isOpen.value) query.value = val
}, { immediate: true })

// ---- teleport target & positioning --------------------------------------
const teleportTargetEl = ref<HTMLElement | null>(null)
const teleportTo = computed<any>(() => teleportTargetEl.value ?? 'body')

const dropdownStyle = computed(() => ({
  top: `${position.value.top}px`,
  left: `${position.value.left}px`,
  width: `${position.value.width}px`,
}))

/**
 * ถ้า target ของ teleport มี transform/translate/scale/filter
 * มันจะกลายเป็น containing block ของ position:fixed → ต้องหักตำแหน่งของมันออก
 */
function containingBlockOffset() {
  const target = teleportTargetEl.value
  if (!target) return { left: 0, top: 0 }
  const s = getComputedStyle(target) as any
  const isContainingBlock =
    (s.transform && s.transform !== 'none') ||
    (s.translate && s.translate !== 'none') ||
    (s.scale && s.scale !== 'none') ||
    (s.rotate && s.rotate !== 'none') ||
    (s.filter && s.filter !== 'none') ||
    (s.perspective && s.perspective !== 'none')
  if (!isContainingBlock) return { left: 0, top: 0 }
  const r = target.getBoundingClientRect()
  return { left: r.left, top: r.top }
}

function computePosition() {
  const rect = inputRef.value?.getBoundingClientRect()
  if (!rect) return
  const offset = containingBlockOffset()
  position.value = {
    top: rect.bottom + 4 - offset.top,
    left: rect.left - offset.left,
    width: rect.width,
  }
}

function flipIfNeeded() {
  const panel = panelRef.value
  const rect = inputRef.value?.getBoundingClientRect()
  if (!panel || !rect) return
  const panelHeight = panel.offsetHeight
  const spaceBelow = window.innerHeight - rect.bottom
  if (spaceBelow < panelHeight + 8 && rect.top > spaceBelow) {
    position.value = { ...position.value, top: rect.top - panelHeight - 4 - containingBlockOffset().top }
  }
}

function openDropdown() {
  if (isOpen.value || props.disabled) return
  computePosition()
  isOpen.value = true
  query.value = ''
  highlightIndex.value = props.options.findIndex(o => o.id === model.value)
  nextTick(flipIfNeeded)
}

function closeDropdown() {
  isOpen.value = false
  query.value = selectedLabel.value
}

function onInput() {
  if (!isOpen.value) {
    computePosition()
    isOpen.value = true
    nextTick(flipIfNeeded)
  }
  highlightIndex.value = 0
}

function select(id: string | null) {
  model.value = id
  isOpen.value = false
  query.value = id ? (props.options.find(o => o.id === id)?.label ?? '') : ''
}

function clearSelection() {
  model.value = null
  query.value = ''
  isOpen.value = false
}

function moveHighlight(delta: number) {
  if (!isOpen.value) {
    openDropdown()
    return
  }
  const count = filteredOptions.value.length
  if (count === 0) return
  highlightIndex.value = (highlightIndex.value + delta + count) % count
  scrollHighlightIntoView()
}

async function scrollHighlightIntoView() {
  await nextTick()
  const items = panelRef.value?.querySelectorAll<HTMLElement>('li[data-opt]')
  items?.[highlightIndex.value]?.scrollIntoView({ block: 'nearest' })
}

function selectHighlighted() {
  const list = filteredOptions.value
  if (highlightIndex.value >= 0 && highlightIndex.value < list.length) {
    select(list[highlightIndex.value].id)
  } else if (list.length === 1) {
    select(list[0].id)
  }
}

// ---- global listeners ----------------------------------------------------
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

function handleResize() {
  if (isOpen.value) computePosition()
}

function handleDialogCancel(e: Event) {
  if (isOpen.value) {
    e.preventDefault()
    closeDropdown()
  }
}

onMounted(() => {
  const custom = props.appendTo ? document.querySelector<HTMLElement>(props.appendTo) : null
  teleportTargetEl.value = custom ?? rootRef.value?.closest('dialog') ?? null

  if (teleportTargetEl.value instanceof HTMLDialogElement) {
    teleportTargetEl.value.addEventListener('cancel', handleDialogCancel)
  }
  document.addEventListener('click', handleClickOutside)
  window.addEventListener('scroll', handleScroll, true)
  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  if (teleportTargetEl.value instanceof HTMLDialogElement) {
    teleportTargetEl.value.removeEventListener('cancel', handleDialogCancel)
  }
  document.removeEventListener('click', handleClickOutside)
  window.removeEventListener('scroll', handleScroll, true)
  window.removeEventListener('resize', handleResize)
})
</script>