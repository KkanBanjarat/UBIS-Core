<template>
  <dialog ref="dialogRef" class="modal">
    <div class="modal-box max-w-5xl p-0 bg-base-200">
      <!-- Toolbar (ไม่พิมพ์) -->
      <div class="flex items-center justify-between gap-2 px-5 py-3 bg-base-100 border-b border-base-200">
        <p class="text-sm font-medium">ตัวอย่างก่อนพิมพ์</p>
        <div class="flex items-center gap-2">
          <button class="btn btn-primary btn-sm gap-1.5" :disabled="isLoadingTrail" @click="print">
            <Printer class="size-4" /> พิมพ์ / บันทึก PDF
          </button>
          <button class="btn btn-ghost btn-sm btn-square" @click="close"><X class="size-4" /></button>
        </div>
      </div>

      <!-- กระดาษ A4 แนวนอน -->
      <div class="p-4 sm:p-6 overflow-auto">
        <div id="print-area-petty"
          class="mx-auto bg-white text-black shadow-md p-8 w-full max-w-[1000px] min-h-[560px] text-[13px] leading-relaxed relative">
          <template v-if="claim">
            <!-- สถานะยังไม่อนุมัติ -->
            <div v-if="claim.docStatus !== 'Approved'"
              class="mb-3 rounded border-2 border-red-500 px-3 py-1.5 text-center font-bold text-red-600">
              <template v-if="claim.docStatus === 'Draft'">เอกสารนี้ยังไม่ได้ส่งอนุมัติ</template>
              <template v-else>เอกสารนี้ยังไม่ได้รับการอนุมัติ ({{ docStatusText(claim.docStatus) }})</template>
            </div>

            <!-- หัวเอกสาร -->
            <div class="flex items-start justify-between">
              <div class="w-32 h-14">
                <img v-if="logoUrl && !logoFailed" :src="logoUrl" @error="logoFailed = true"
                  class="h-14 w-auto max-w-32 object-contain" />
              </div>
              <p class="text-xs text-gray-600">เลขที่ : {{ claim.docNum || '-' }}</p>
            </div>

            <div class="text-center -mt-2 space-y-1">
              <h2 class="text-base font-bold">ใบขอเบิกเงินสดย่อย</h2>
              <p class="text-sm font-medium">
                {{ COMPANIES[0].name }} และ {{ COMPANIES[1].name }}
              </p>
              <div class="flex justify-center gap-16 pt-1 text-sm">
                <label v-for="(c, i) in COMPANIES" :key="i" class="flex items-center gap-2">
                  <span class="pc-box">{{ companyIndex === i ? '✓' : '' }}</span>
                  {{ c.name }}
                </label>
              </div>
            </div>

            <!-- ผู้ขอเบิก -->
            <p class="mt-6 whitespace-nowrap">
              ผู้ขอเบิก <span class="pc-fill min-w-[210px]">{{ claim.employeeNameTh || '' }}</span>
              ฝ่าย <span class="pc-fill min-w-[170px]">{{ claim.affiliation || '' }}</span>
              รหัสฝ่าย <span class="pc-fill min-w-[110px]">{{ claim.affiliationCode || '' }}</span>
              วันที่ <span class="pc-fill min-w-[130px]">{{ fmtDate(claim.docDate) }}</span>
            </p>

            <!-- ตาราง -->
            <table class="mt-6 mx-auto w-[88%] border-collapse border border-black">
              <thead>
                <tr>
                  <th class="border border-black px-2 py-1 w-20">ลำดับที่</th>
                  <th class="border border-black px-2 py-1">รายการ</th>
                  <th class="border border-black px-2 py-1 w-44">จำนวนเงิน</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(l, i) in rows" :key="i" class="h-9 break-inside-avoid">
                  <td class="border border-black px-2 text-center">{{ l ? i + 1 : '' }}</td>
                  <td class="border border-black px-2">{{ l?.detail || '' }}</td>
                  <td class="border border-black px-2 text-right">{{ l ? money(l.amount) : '' }}</td>
                </tr>
              </tbody>
            </table>
            <div class="mx-auto w-[88%] flex items-end justify-end gap-3 mt-1">
              <span>รวมเงิน</span>
              <span class="inline-block w-44 border-b-2 border-black text-right font-bold px-2">
                {{ money(claim.totalAmount) }}
              </span>
            </div>
            <p class="mx-auto w-[88%] text-right text-xs text-gray-600 mt-0.5">({{ thaiBahtText(claim.totalAmount) }})</p>

            <!-- ผู้รับเงิน / ผู้อนุมัติ -->
            <div class="mt-6 space-y-2 break-inside-avoid">
              <p>ผู้รับเงิน <span class="pc-fill min-w-[380px]"></span></p>
              <p>ชื่อ-ตำแหน่ง
                <span class="pc-fill min-w-[360px]">
                  {{ claim.employeeNameTh || '' }}{{ claim.positionTh ? ' - ' + claim.positionTh : '' }}
                </span>
              </p>
              <p>วันที่รับเงิน <span class="pc-fill min-w-[360px]"></span></p>

              <p class="pt-4">ผู้อนุมัติ
                <span class="pc-fill min-w-[380px]">
                  <template v-if="approverApproved">อนุมัติแล้ว<template v-if="lastStep?.approvedDate"> {{ fmtShort(lastStep.approvedDate) }}</template></template>
                </span>
              </p>
              <p>ชื่อ-ตำแหน่ง
                <span class="pc-fill min-w-[360px]">
                  {{ approverName }}{{ approverPosition ? ' - ' + approverPosition : '' }}
                </span>
              </p>
              <p v-if="isLoadingTrail" class="text-xs text-gray-400">กำลังโหลดลำดับการอนุมัติ...</p>
              <p v-else-if="trailFailed" class="text-xs text-red-500">โหลดลำดับการอนุมัติไม่สำเร็จ</p>
            </div>

            <p class="absolute right-4 bottom-2 text-[10px] text-gray-600">FN-F-2301 &nbsp;(Rev.00 : 01/11/69)</p>
          </template>
        </div>
      </div>
    </div>
    <form method="dialog" class="modal-backdrop"><button>close</button></form>
  </dialog>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { Printer, X } from 'lucide-vue-next'
import type { PettyCash } from '../../types/PettyCash.ts'
import type { ApprovalStep } from '../../types/Approval.ts'
import { useApprovalStore } from '../../stores/approvalStore.ts'
import { useAuthStore } from '../../stores/authStore.ts'
import { thaiBahtText } from '../../utils/thaiBahtText.ts'

// ฟิลด์ที่หลังบ้านต้องส่งมา (ไม่มีก็ไม่ error)
type ClaimView = PettyCash & {
  company?: string | null
  companyCode?: string | null
  affiliation?: string | null
  affiliationCode?: string | null
}
type StepView = ApprovalStep & {
  approverPosition?: string | null
  actualApprovePosition?: string | null
}

// บริษัทที่ใช้ฟอร์มนี้ (keys ใช้จับคู่กับชื่อบริษัทของผู้ขอเบิก)
const COMPANIES = [
  { name: 'บริษัท ยูบิส (เอเชีย) จำกัด (มหาชน)', keys: ['เอเชีย', 'asia'] },
  { name: 'บริษัท ยูบิส พรีมาเทค จำกัด', keys: ['พรีมาเทค', 'prematech'] },
]

const approvalStore = useApprovalStore()
const authStore = useAuthStore()
const dialogRef = ref<HTMLDialogElement>()
const claim = ref<ClaimView | null>(null)
const steps = ref<StepView[]>([])
const isLoadingTrail = ref(false)
const trailFailed = ref(false)
const logoFailed = ref(false)
const printedBy = ref('')

// โลโก้จาก src/assets/logos (ชื่อไฟล์ = รหัสบริษัท)
const logoFiles = import.meta.glob('../../assets/logos/*.{png,jpg,jpeg,svg,webp}', {
  eager: true,
  query: '?url',
  import: 'default',
}) as Record<string, string>

const logoUrl = computed(() => {
  const code = claim.value?.companyCode?.toLowerCase()
  if (!code) return null
  const key = Object.keys(logoFiles).find(k =>
    k.split('/').pop()!.replace(/\.[^.]+$/, '').toLowerCase() === code,
  )
  return key ? logoFiles[key] : null
})

const companyIndex = computed(() => {
  const name = (claim.value?.company ?? '').toLowerCase()
  return COMPANIES.findIndex(c => c.keys.some(k => name.includes(k.toLowerCase())))
})

// แถวตาราง: รายการจริง + แถวว่างให้ครบอย่างน้อย 4 แถว
const rows = computed(() => {
  const lines = claim.value?.lines ?? []
  const pad = Math.max(0, 4 - lines.length)
  return [...lines, ...Array(pad).fill(null)] as (NonNullable<ClaimView['lines']>[number] | null)[]
})

// ผู้อนุมัติ = ขั้นสุดท้ายของสายอนุมัติ
const lastStep = computed(() => (steps.value.length ? steps.value[steps.value.length - 1] : null))
const approverApproved = computed(() => lastStep.value?.status === 'Approved')
const approverName = computed(() =>
  lastStep.value ? lastStep.value.actualApproveNameTh || lastStep.value.approverNameTh : '',
)
const approverPosition = computed(() => {
  const s = lastStep.value
  if (!s) return ''
  return (s.actualApproveNameTh ? s.actualApprovePosition : s.approverPosition) || ''
})

const DOC_STATUS: Record<string, string> = {
  Draft: 'ร่าง', WaitApprove: 'รออนุมัติ', Approved: 'อนุมัติแล้ว',
  Rejected: 'ตีกลับ', Disapproved: 'ไม่อนุมัติ', Cancelled: 'ยกเลิก',
}
const docStatusText = (s: string) => DOC_STATUS[s] ?? s
const money = (n?: number) => (n ?? 0).toLocaleString('th-TH', { minimumFractionDigits: 2 })
const fmtDate = (d: string) =>
  new Date(d).toLocaleDateString('th-TH', { year: 'numeric', month: 'long', day: 'numeric' })
const fmtShort = (d: string) =>
  new Date(d).toLocaleDateString('th-TH', { year: 'numeric', month: 'short', day: 'numeric' })

async function open(data: PettyCash) {
  claim.value = data as ClaimView
  steps.value = []
  trailFailed.value = false
  logoFailed.value = false
  dialogRef.value?.showModal()

  if (!data.docNum || data.docStatus === 'Draft') return
  isLoadingTrail.value = true
  try {
    const trail = await approvalStore.getTrail('PettyCash', data.docNum)
    steps.value = [...trail.steps].sort((a, b) => a.stepNo - b.stepNo)
  } catch (err) {
    trailFailed.value = true
    console.error('Failed to load approval trail:', err)
  } finally {
    isLoadingTrail.value = false
  }
}
function close() {
  dialogRef.value?.close()
}

async function print() {
  const src = document.getElementById('print-area-petty')
  if (!src) return

  if (!printedBy.value) {
    try {
      const emp = await authStore.fetchCurrentEmployee(false,false)
      if (emp) printedBy.value = `${emp.fNameTh} ${emp.lNameTh}`
    } catch (err) {
      console.error('Failed to load current employee:', err)
    }
  }

  const root = document.createElement('div')
  root.id = 'print-root-petty'
  const clone = src.cloneNode(true) as HTMLElement
  clone.removeAttribute('id')
  root.appendChild(clone)

  const footer = document.createElement('div')
  footer.className = 'print-footer-petty'
  const when = new Date().toLocaleString('th-TH', {
    year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit',
  })
  footer.textContent = `พิมพ์เมื่อ ${when}${printedBy.value ? ' โดย ' + printedBy.value : ''}`
  root.appendChild(footer)

  document.body.appendChild(root)
  document.body.classList.add('printing-petty')

  const cleanup = () => {
    document.body.classList.remove('printing-petty')
    root.remove()
    window.removeEventListener('afterprint', cleanup)
  }
  window.addEventListener('afterprint', cleanup)
  window.print()
}

defineExpose({ open, close })
</script>

<style>
.pc-fill {
  display: inline-block;
  border-bottom: 1px dotted #555;
  padding: 0 6px;
  text-align: center;
  vertical-align: bottom;
  min-height: 1.4em;
}
.pc-box {
  display: inline-block;
  width: 16px;
  height: 16px;
  border: 1px solid #000;
  text-align: center;
  line-height: 14px;
  font-size: 12px;
}

#print-root-petty { display: none; }

@page petty { size: A4 landscape; margin: 0; }

@media print {
  body.printing-petty > *:not(#print-root-petty) { display: none !important; }
  body.printing-petty #print-root-petty { display: block !important; }
  body.printing-petty #print-root-petty > div:first-child {
    page: petty;
    width: 100% !important;
    max-width: none !important;
    min-height: 0 !important;
    margin: 0 !important;
    padding: 10mm 14mm 16mm !important;
    box-shadow: none !important;
  }
  body.printing-petty .print-footer-petty {
    position: fixed;
    left: 14mm;
    bottom: 6mm;
    font-size: 10px;
    color: #6b7280;
  }
}
</style>