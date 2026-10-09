<template>
    <dialog ref="dialogRef" class="modal">
        <div class="modal-box max-w-4xl p-0 bg-base-200">
            <!-- Toolbar (ไม่พิมพ์) -->
            <div class="flex items-center justify-between gap-2 px-5 py-3 bg-base-100 border-b border-base-200">
                <p class="text-sm font-medium">ตัวอย่างก่อนพิมพ์</p>
                <div class="flex items-center gap-2">
                    <button class="btn btn-primary btn-sm gap-1.5" :disabled="isLoadingTrail" @click="print">
                        <Printer class="size-4" /> พิมพ์ / บันทึก PDF
                    </button>
                    <button class="btn btn-ghost btn-sm btn-square" @click="close">
                        <X class="size-4" />
                    </button>
                </div>
            </div>

            <!-- กระดาษ A4 -->
            <div class="p-4 sm:p-6 overflow-auto">
                <div id="print-area"
                    class="mx-auto bg-white text-black shadow-md p-8 sm:p-10 w-full max-w-[794px] min-h-[600px] text-[13px] leading-relaxed">
                    <template v-if="claim">
                        <!-- หัวเอกสาร: บริษัทของผู้ขอเบิก -->
                        <div class="text-center mb-5">
                            <img v-if="claim.companyLogoUrl" :src="claim.companyLogoUrl"
                                class="h-12 mx-auto mb-2 object-contain" />
                            <p v-if="claim.companyNameTh" class="text-base font-bold">{{ claim.companyNameTh }}</p>
                            <p v-if="claim.companyNameEn" class="text-xs text-gray-500">{{ claim.companyNameEn }}</p>
                            <h2 class="text-xl font-bold mt-3">ใบเบิกสวัสดิการ</h2>
                            <!-- <p class="text-sm text-gray-500">{{ claim.company }} ({{ claim.branch }})</p> -->
                            <p class="text-gray-500 text-sm"> {{ claim.company }}</p>
                        </div>
                        <!-- <div class="grid grid-cols-2 gap-x-6 gap-y-1 mb-4">
                            <p class="text-gray-500 text-sm"> {{  claim.company }}</p>
                            <p class="text-gray-500 text-right  text-sm">สาขา {{ claim.branch || '-' }}</p>
                         </div> -->
                        <div v-if="claim.docStatus !== 'Approved'"
                            class="mb-4 rounded border-2 border-red-500 px-3 py-2 text-center font-bold text-red-600">
                            <template v-if="claim.docStatus === 'Draft'">เอกสารนี้ยังไม่ได้ส่งอนุมัติ</template>
                            <template v-else>เอกสารนี้ยังไม่ได้รับการอนุมัติ ({{ docStatusText(claim.docStatus)
                            }})</template>
                        </div>
                        <div class="grid grid-cols-2 gap-x-6 gap-y-1 mb-4">
                            <p><span class="text-gray-500">เลขที่เอกสาร :</span> <b>{{ claim.docNum || '-' }}</b> ({{
                                docStatusText(claim.docStatus) }})</p>
                            <p><span class="text-gray-500">วันที่ :</span> {{ fmtDate(claim.docDate) }}</p>
                            <p><span class="text-gray-500">ผู้ขอเบิก :</span> {{ claim.employeeNameTh || '-' }}</p>
                            <p><span class="text-gray-500">ตำแหน่ง :</span> {{ claim.positionTh || '-' }}
                                ({{ claim.positionLevel || '-' }} )</p>
                            <p>
                                <span class="text-gray-500">สังกัด :</span>{{ claim.affiliation || '-' }}
                                <span v-if="claim.affiliationCode"> ({{ claim.affiliationCode }})</span>
                            </p>
                            <p><span class="text-gray-500">สาขา :</span> {{ claim.branch }}</p>
                        </div>

                        <table class="w-full border-collapse border border-gray-400 mb-3">
                            <thead>
                                <tr class="bg-gray-100">
                                    <th class="border border-gray-400 px-2 py-1.5 w-12">ลำดับ</th>
                                    <th class="border border-gray-400 px-2 py-1.5">สวัสดิการ</th>
                                    <th class="border border-gray-400 px-2 py-1.5">รายละเอียด</th>
                                    <!-- <th class="border border-gray-400 px-2 py-1.5 w-16">จำนวน</th> -->
                                    <th class="border border-gray-400 px-2 py-1.5 w-28 text-right">จำนวนเงิน (บาท)</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="(l, i) in claim.lines" :key="l.id ?? i" class="break-inside-avoid">
                                    <td class="border border-gray-400 px-2 py-1.5 text-center">{{ i + 1 }}</td>
                                    <td class="border border-gray-400 px-2 py-1.5">{{ l.benefitNameTh || '-' }}</td>
                                    <td class="border border-gray-400 px-2 py-1.5">{{ l.detail || '-' }}</td>
                                    <!-- <td class="border border-gray-400 px-2 py-1.5 text-center">{{ l.qty ?? 1 }}</td> -->
                                    <td class="border border-gray-400 px-2 py-1.5 text-right">{{ money(l.amount) }}</td>
                                </tr>
                                <tr v-if="!claim.lines?.length">
                                    <td colspan="4" class="border border-gray-400 px-2 py-4 text-center text-gray-400">
                                        ไม่มีรายการ</td>
                                </tr>
                                <tr class="font-bold bg-gray-50 break-inside-avoid">
                                    <td colspan="2" class="border border-gray-400 px-2 py-1.5 text-right">รวมทั้งสิ้น
                                    </td>
                                    <td class="border border-gray-400 px-2 py-1.5 text-center">
                                        {{ thaiBahtText(claim.totalAmount) }}
                                    </td>
                                    <td class="border border-gray-400 px-2 py-1.5 text-right">{{
                                        money(claim.totalAmount) }}</td>
                                </tr>
                            </tbody>
                        </table>
                        <p v-if="claim.remark" class="mb-4"><span class="text-gray-500">หมายเหตุ :</span> {{
                            claim.remark }}</p>
                        <!-- ลายเซ็น: กรอบผู้อนุมัติตามลำดับจริง -->
                        <div v-if="claim.docStatus !== 'Draft'" class="grid grid-cols-2 gap-4 mt-10 text-center">
                            <div v-for="s in steps" :key="s.stepNo" class="border border-gray-400 break-inside-avoid">
                                <!-- หัวกรอบ: ตำแหน่งในเอกสาร -->
                                <div class="border-b border-gray-400 px-2 py-1.5 text-xs font-semibold [print-color-adjust:exact]"
                                :class="s.status === 'Approved' ? ' bg-green-300' : 'bg-gray-200' ">
                                    {{ s.stepName || 'ผู้อนุมัติ ลำดับที่ ' + s.stepNo }}
                                </div>

                                <div class="px-3 pb-3">
                                    <!-- ชื่อ (เหนือเส้นลายเซ็น) -->
                                    <div class="h-14 flex items-end justify-center text-[15px]"
                                        :class="s.status === 'Approved' ? 'text-black font-medium' : 'text-gray-400'">
                                        {{ s.actualApproveNameTh || s.approverNameTh }}
                                    </div>
                                    <div class="border-b border-gray-500"></div>

                                    <!-- ตำแหน่ง -->
                                    <p class="mt-1 text-xs text-gray-600">
                                        {{ (s.actualApproveNameTh ? s.actualApprovePosition : s.approverPosition) || ''
                                        }}
                                    </p>

                                    <!-- กรณีอนุมัติแทน -->
                                    <p v-if="s.actualApproveNameTh && s.approverNameTh !== s.actualApproveNameTh"
                                        class="text-[11px] text-gray-500">
                                        (อนุมัติแทน {{ s.approverNameTh }})
                                    </p>

                                    <!-- สถานะ + วันที่ -->
                                    <p class="mt-1 text-xs"
                                        :class="s.status === 'Approved' ? 'text-black' : 'text-gray-500'">
                                        {{ stepStatusText(s.status) }}
                                        <span v-if="s.approvedDate"> · {{ fmtShort(s.approvedDate) }}</span>
                                    </p>
                                </div>
                            </div>
                        </div>

                        <p v-if="isLoadingTrail" class="text-center text-xs text-gray-400 mt-6">
                            กำลังโหลดลำดับการอนุมัติ...</p>
                        <p v-else-if="trailFailed" class="text-center text-xs text-red-500 mt-6">
                            โหลดลำดับการอนุมัติไม่สำเร็จ</p>
                    </template>
                </div>
            </div>
        </div>
        <form method="dialog" class="modal-backdrop"><button>close</button></form>
    </dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Printer, X } from 'lucide-vue-next'
import type { BenefitClaim } from '../../types/BenefitClaim.ts'
import type { ApprovalStep } from '../../types/Approval.ts'
import { useApprovalStore } from '../../stores/approvalStore.ts'
import { thaiBahtText } from '../../utils/thaiBahtText.ts'
import { useAuthStore } from '../../stores/authStore.ts'

// ฟิลด์ที่หลังบ้านจะเพิ่มให้ (ไม่มีก็ไม่ error)
type ClaimView = BenefitClaim & {
    companyNameTh?: string | null
    companyNameEn?: string | null
    companyLogoUrl?: string | null
    company?: string | null
    branch?: string | null
}
type StepView = ApprovalStep

const approvalStore = useApprovalStore()
const dialogRef = ref<HTMLDialogElement>()
const claim = ref<ClaimView | null>(null)
const steps = ref<StepView[]>([])
const isLoadingTrail = ref(false)
const trailFailed = ref(false)

const DOC_STATUS: Record<string, string> = {
    Draft: 'ร่าง', WaitApprove: 'รออนุมัติ', Approved: 'อนุมัติแล้ว',
    Rejected: 'ตีกลับ', Disapproved: 'ไม่อนุมัติ', Cancelled: 'ยกเลิก',
}
const docStatusText = (s: string) => DOC_STATUS[s] ?? s
const authStore = useAuthStore()
const printedBy = ref('')

function stepStatusText(s: string) {
    switch (s) {
        case 'Approved': return 'อนุมัติแล้ว'
        case 'WaitApprove': return 'รออนุมัติ'
        case 'Rejected': return 'ตีกลับ'
        case 'Disapproved': return 'ไม่อนุมัติ'
        case 'Recalled': return 'ดึงกลับ'
        default: return 'รอลำดับก่อนหน้า'
    }
}

const money = (n?: number) => (n ?? 0).toLocaleString('th-TH', { minimumFractionDigits: 2 })
const fmtDate = (d: string) =>
    new Date(d).toLocaleDateString('th-TH', { year: 'numeric', month: 'long', day: 'numeric' })
const fmtShort = (d: string) =>
    new Date(d).toLocaleDateString('th-TH', { year: 'numeric', month: 'short', day: 'numeric' })

async function open(data: BenefitClaim) {
    claim.value = data as ClaimView
    steps.value = []
    trailFailed.value = false
    dialogRef.value?.showModal()

    if (!data.docNum) return
    isLoadingTrail.value = true
    try {
        if (!data.docNum || data.docStatus === 'Draft') return
        const trail = await approvalStore.getTrail('BenefitClaim', data.docNum)
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

// คัดลอกกระดาษไปไว้ใต้ body แล้วซ่อนส่วนอื่นทั้งหมดตอนพิมพ์
async function print() {
    const src = document.getElementById('print-area')
    if (!src) return

    if (!printedBy.value) {
        try {
            const emp = await authStore.fetchCurrentEmployee(false)
            if (emp) printedBy.value = `${emp.fNameTh} ${emp.lNameTh}`
        } catch (err) {
            console.error('Failed to load current employee:', err)
        }
    }

    const root = document.createElement('div')
    root.id = 'print-root'
    const clone = src.cloneNode(true) as HTMLElement
    clone.removeAttribute('id')
    root.appendChild(clone)

    const footer = document.createElement('div')
    footer.className = 'print-footer'
    const when = new Date().toLocaleString('th-TH', {
        year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit',
    })
    footer.textContent = `พิมพ์เมื่อ ${when}${printedBy.value ? ' โดย ' + printedBy.value : ''}`
    root.appendChild(footer)

    document.body.appendChild(root)
    document.body.classList.add('printing')

    const cleanup = () => {
        document.body.classList.remove('printing')
        root.remove()
        window.removeEventListener('afterprint', cleanup)
    }
    window.addEventListener('afterprint', cleanup)
    window.print()
}

defineExpose({ open, close })
</script>

<style>
#print-root {
    display: none;
}

@media print {
    @page {
        size: A4;
        margin: 0;
    }

    body.printing>*:not(#print-root) {
        display: none !important;
    }

    body.printing #print-root {
        display: block !important;
    }

    body.printing #print-root>div:first-child {
        width: 100% !important;
        max-width: none !important;
        min-height: 0 !important;
        margin: 0 !important;
        padding: 12mm 12mm 22mm !important;
        box-shadow: none !important;
    }

    body.printing .print-footer {
        position: fixed;
        left: 12mm;
        right: 12mm;
        bottom: 8mm;
        padding-top: 2mm;
        border-top: 1px solid #d1d5db;
        font-size: 11px;
        color: #6b7280;
        text-align: right;
    }
}
</style>