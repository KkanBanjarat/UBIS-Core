<template>
  <!-- Loading -->
  <section v-if="isLoading" class="space-y-4">
    <div class="rounded-2xl border border-base-300 bg-base-100 p-5 shadow-sm">
      <div class="skeleton mb-5 h-6 w-40"></div>

      <div class="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <div v-for="i in 4" :key="i" class="space-y-2">
          <div class="skeleton h-3 w-20"></div>
          <div class="skeleton h-5 w-32"></div>
        </div>
      </div>
    </div>
  </section>


  <!-- Content -->
  <section v-else-if="doc" class="space-y-4">

    <!-- ==================== DOCUMENT ==================== -->
    <div
      class="overflow-hidden rounded-2xl border border-base-300
             bg-base-100 shadow-sm"
    >

      <!-- Document Header -->
      <div
        class="relative overflow-hidden border-b border-base-300
               bg-base-200/40 px-5 py-4"
      >
        <div class="pointer-events-none absolute -right-12 -top-12 size-32 rounded-full bg-primary/5" ></div>

        <div class="relative flex items-center gap-3">
          <div class="flex size-10 shrink-0 items-center justify-center
                   rounded-xl bg-primary text-primary-content shadow-sm">
            <FileText class="size-4.5" />
          </div>

          <div class="min-w-0">
            <p class="text-[11px] font-medium text-base-content/45">
              ข้อมูลเอกสาร
            </p>
            <h2 class="mt-0.5 truncate text-base font-bold text-base-content">
              {{ props.docNumber }}
            </h2>
          </div>
        </div>
      </div>


      <!-- Document Information -->
      <div class="px-5 py-4">
        <div class="grid gap-x-6 gap-y-4 sm:grid-cols-2 lg:grid-cols-2" >
          <!-- Employee -->
          <div class="min-w-0">
            <p class="text-[11px] text-base-content/40">ผู้ขอเบิก</p>
            <p class="mt-0.5 truncate text-sm font-semibold">{{ doc.employeeNameTh || '-' }}</p>
          </div>
          <!-- Date -->
          <div>
            <p class="text-[11px] text-base-content/40">วันที่ขอเบิก</p>
            <p class="mt-0.5 text-sm font-semibold">{{ formatDate(doc.docDate) }}</p>
          </div>
        </div>
        <!-- Remark -->
        <div v-if="doc.remark"
          class="mt-4 rounded-xl border border-warning/20
                 bg-warning/5 px-3.5 py-3">
          <p class="mb-0.5 text-[11px] font-semibold text-warning">หมายเหตุ</p>
          <p class="text-sm text-base-content/70">
            {{ doc.remark }}
          </p>
        </div>
      </div>
      <!-- Divider / Expense Header -->
      <div class="flex items-center justify-between
               border-y border-base-300 bg-base-200/40
               px-5 py-3">
        <div class="flex items-center gap-2.5">
          <div class="flex size-8 shrink-0 items-center justify-center
                   rounded-lg bg-base-100 text-primary
                   shadow-sm ring-1 ring-base-300">
            <ReceiptText class="size-4" />
          </div>
          <div>
            <h3 class="text-sm font-semibold text-base-content">
              รายการเบิก
            </h3>
            <p class="text-[11px] text-base-content/40">
              รายละเอียดค่าใช้จ่าย
            </p>
          </div>
        </div>
        <span class="badge badge-ghost badge-sm">
          {{ doc.lines?.length ?? 0 }} รายการ
        </span>
      </div>
      <!-- Expense Items -->
      <div v-if="doc.lines?.length">
        <div v-for="(line, idx) in doc.lines"
          :key="line.id ?? idx"
          class="group flex items-center gap-3.5
                 border-b border-base-200 px-5 py-3.5
                 transition-colors last:border-0
                 hover:bg-base-200/25">
          <!-- Number -->
          <div class="flex size-8 shrink-0 items-center justify-center
                   rounded-lg bg-base-200
                   text-[11px] font-semibold text-base-content/50
                   transition-colors
                   group-hover:bg-primary/10
                   group-hover:text-primary">
            {{ String(idx + 1).padStart(2, '0') }}
          </div>
          <!-- Detail -->
          <div class="min-w-0 flex-1">
            <p class="truncate text-sm font-semibold">
              {{ line.benefitNameTh || 'เงินสดย่อยทั่วไป' }}
            </p>
            <p v-if="line.detail"
              class="mt-0.5 truncate text-xs text-base-content/45">
              {{ line.detail }}
            </p>
          </div>
          <!-- Amount -->
          <div class="shrink-0 text-right">
            <p class="text-sm font-bold">
              {{ formatAmount(line.amount) }}
            </p>
            <p class="text-[10px] text-base-content/40">บาท</p>
          </div>
        </div>
      </div>
      <!-- Empty -->
      <div v-else
        class="px-5 py-10 text-center text-sm text-base-content/40">
        ไม่มีรายการเบิก
      </div>
      <!-- Total -->
      <div class="flex items-center justify-between
               border-t border-base-300 bg-base-200/40
               px-5 py-4">
        <span class="text-sm font-semibold">ยอดรวมทั้งสิ้น</span>
        <div class="text-right">
          <span class="text-xl font-bold text-primary">
            {{ formatAmount(doc.totalAmount) }}
          </span>
          <span class="ml-1 text-xs text-base-content/50">บาท</span>
        </div>
      </div>
    </div>
  </section>
  <!-- Not Found -->
  <section v-else
    class="rounded-2xl border border-base-300
           bg-base-100 p-12 text-center shadow-sm">
    <div class="mx-auto flex size-14 items-center justify-center
             rounded-2xl border border-base-300 bg-base-200">
      <FileQuestion class="size-6 text-base-content/40" />
    </div>

    <p class="mt-4 text-sm font-semibold">
      ไม่พบข้อมูลเอกสาร
    </p>

    <p class="mt-1 text-xs text-base-content/45">
      ไม่พบเอกสารเลขที่ {{ props.docNumber }}
    </p>
  </section>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { User as UserIcon, FileQuestion, FileText, ReceiptText } from 'lucide-vue-next'
import { usePrettyCashStore } from '../../stores/prettyCashStore'
import type { PrettyCash } from '../../types/PrettyCash'

const props = defineProps<{ docNumber: string }>()

const prettyCashStore = usePrettyCashStore()
const doc = ref<PrettyCash | null>(null)
const isLoading = ref(true)

function formatDate(d: string) {
  if (!d) return '-'
  return new Date(d).toLocaleDateString('th-TH', { year: 'numeric', month: 'short', day: 'numeric' })
}

function formatAmount(amount: number | null | undefined) {
  return Number(amount || 0).toLocaleString('th-TH', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })
}

onMounted(async () => {
  try {
    doc.value = await prettyCashStore.getByDocNum(props.docNumber)
  } catch (err) {
    console.error('Failed to load pretty cash detail:', err)
  } finally {
    isLoading.value = false
  }
})
</script>