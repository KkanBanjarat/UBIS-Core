<template>
    <div class="relative" ref="rootRef">
        <label v-if="label" class="block text-xs font-medium text-base-content/60 mb-1.5">{{ label }}</label>

    <button
      type="button"
      class="input input-bordered w-full text-sm flex items-center justify-between gap-2"
      :class="disabled && 'opacity-60 cursor-not-allowed bg-base-200'"
      :disabled="disabled"
      @click="toggleOpen"
    >
      <span class="truncate text-left" :class="!displayLabel && 'text-base-content/40'">
        {{ displayLabel || placeholder || 'เลือกพนักงาน' }}
      </span>
      <div class="flex items-center gap-1 shrink-0">
        <X v-if="modelValue && !disabled" class="size-3.5 text-base-content/40 hover:text-error" @click.stop="clearSelection" />
        <ChevronDown class="size-3.5 text-base-content/40 transition-transform" :class="isOpen && 'rotate-180'" />
      </div>
    </button>

    <Transition enter-active-class="transition-all duration-150 ease-out"
        enter-from-class="opacity-0 -translate-y-1" enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition-all duration-100 ease-in" leave-from-class="opacity-100"
        leave-to-class="opacity-0">
        <div v-if="isOpen"
            class="absolute z-50 mt-1 w-full bg-base-100 rounded-lg shadow-lg border border-base-200 flex flex-col overflow-hidden">
            <div class="p-2 border-b border-base-200 shrink-0">
                <div class="relative">
                    <Search class="absolute left-2.5 top-1/2 -translate-y-1/2 size-3.5 text-base-content/40 pointer-events-none" />
                    <input ref="searchInputRef" v-model="searchTerm" type="text"
                        placeholder="ค้นหารหัส, ชื่อไทย, ชื่ออังกฤษ..."
                        class="input input-bordered input-sm w-full pl-8 text-sm" @input="onSearchInput" />
                </div>
            </div>

            <div ref="listRef" class="overflow-y-auto max-h-64" @scroll="onScroll">
                <div v-for="item in items" :key="item.id"
                    class="px-3 py-2 text-sm hover:bg-base-200/60 cursor-pointer flex flex-col"
                    :class="item.id === modelValue && 'bg-primary/10'" @click="selectItem(item)">
                    <span class="font-medium">{{ item.fNameTh }} {{ item.lNameTh }}</span>
                    <span class="text-xs text-base-content/45">รหัส {{ item.empId }} · {{ item.positionName }} </span>
                </div>

                <div v-if="isLoading" class="flex justify-center py-3">
                    <span class="loading loading-spinner loading-xs"></span>
                </div>
                <div v-if="!isLoading && items.length === 0"
                    class="px-3 py-6 text-center text-xs text-base-content/40">ไม่พบพนักงาน</div>
                <div v-if="!isLoading && !hasMore && items.length > 0"
                    class="px-3 py-2 text-center text-[11px] text-base-content/30">แสดงครบทั้งหมดแล้ว</div>
            </div>
        </div>
    </Transition>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import type { PropType } from 'vue'
import { Search, X, ChevronDown } from 'lucide-vue-next'
import hrApi from '../../services/hrApi'

interface EmployeeSearchItem {
    id: string
    empId: string
    fNameTh: string
    lNameTh: string
    fNameEn: string
    lNameEn: string
    positionName: string
}

const props = defineProps({
  modelValue: { type: String as PropType<string | null>, default: null },
  label: { type: String, default: undefined },
  placeholder: { type: String, default: undefined },
  companyId: { type: String as PropType<string | null>, default: null },
  excludeId: { type: String as PropType<string | null>, default: null },
  initialLabel: { type: String as PropType<string | null>, default: null },
  pageSize: { type: Number, default: 50 },
  disabled: { type: Boolean, default: false },
})

const emit = defineEmits<{ 'update:modelValue': [value: string | null] }>()

const rootRef = ref<HTMLElement>()
const searchInputRef = ref<HTMLInputElement>()
const listRef = ref<HTMLElement>()

const isOpen = ref(false)
const isLoading = ref(false)
const searchTerm = ref('')
const items = ref<EmployeeSearchItem[]>([])
const page = ref(1)
const totalCount = ref(0)

const size = computed(() => props.pageSize ?? 50)
const hasMore = computed(() => items.value.length < totalCount.value)
const selectedLabel = ref<string | null>(props.initialLabel ?? null)

const displayLabel = computed(() => {
    if (!props.modelValue) return ''
    const found = items.value.find(i => i.id === props.modelValue)
    if (found) return `${found.fNameTh} ${found.lNameTh} (${found.empId})`
    return selectedLabel.value ?? ''
})

watch(() => props.initialLabel, v => { if (v) selectedLabel.value = v })

let debounceTimer: ReturnType<typeof setTimeout>
function onSearchInput() {
    clearTimeout(debounceTimer)
    debounceTimer = setTimeout(() => {
        page.value = 1
        items.value = []
        fetchItems()
    }, 300)
}

async function fetchItems() {
    isLoading.value = true
    try {
        const res = await hrApi.post('/Employees/search', {
            search: searchTerm.value || undefined,
            companyId: props.companyId || undefined,
            excludeId: props.excludeId || undefined,
            status: 'Active',
            page: page.value,
            pageSize: size.value,
        })
        const newItems: EmployeeSearchItem[] = res.data.items.map((x: any) => ({
            id: x.id,
            empId: x.empId,
            fNameTh: x.firstNameTh,
            lNameTh: x.lastNameTh,
            fNameEn: x.firstNameEn,
            lNameEn: x.lastNameEn,
            positionName : `${x.positionNameTh}`
        }))
        items.value = page.value === 1 ? newItems : [...items.value, ...newItems]
        totalCount.value = res.data.totalCount
    } catch (err) {
        console.error('Failed to search employees:', err)
    } finally {
        isLoading.value = false
    }
}

function onScroll() {
    const el = listRef.value
    if (!el || isLoading.value || !hasMore.value) return
    if (el.scrollTop + el.clientHeight >= el.scrollHeight - 40) {
        page.value += 1
        fetchItems()
    }
}

function toggleOpen() {
    isOpen.value = !isOpen.value
    if (isOpen.value) {
        searchTerm.value = ''
        page.value = 1
        items.value = []
        fetchItems()
        requestAnimationFrame(() => searchInputRef.value?.focus())
    }
}

function selectItem(item: EmployeeSearchItem) {
    selectedLabel.value = `${item.fNameTh} ${item.lNameTh} (${item.empId})`
    emit('update:modelValue', item.id)
    isOpen.value = false
}

function clearSelection() {
    selectedLabel.value = null
    emit('update:modelValue', null)
}

function onClickOutside(e: MouseEvent) {
    if (rootRef.value && !rootRef.value.contains(e.target as Node)) isOpen.value = false
}

onMounted(() => document.addEventListener('mousedown', onClickOutside))
onBeforeUnmount(() => document.removeEventListener('mousedown', onClickOutside))
</script>