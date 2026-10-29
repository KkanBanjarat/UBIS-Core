<template>
  <div class="print-root">
    <!-- แถบเครื่องมือ (ไม่ถูกพิมพ์) -->
    <div class="no-print mx-auto flex max-w-[210mm] items-center justify-between py-3">
      <button class="btn btn-sm btn-ghost" @click="closeWindow">ปิด</button>
      <button class="btn btn-sm btn-primary gap-1.5" :disabled="!doc" @click="printPage">
        <Printer class="size-4" /> พิมพ์
      </button>
    </div>

    <p v-if="isLoading" class="no-print text-center text-sm py-20">กำลังโหลด...</p>
    <p v-else-if="errorMessage" class="no-print text-center text-sm text-error py-20">{{ errorMessage }}</p>

    <article v-else-if="doc" class="sheet">
      <!-- หัวฟอร์ม -->
      <header class="text-center">
        <p class="company">{{ COMPANY_NAME }}</p>
        <h1 class="title">{{ config.title }}</h1>
      </header>

      <div class="info-grid">
        <div><span class="lbl">เลขที่เอกสาร</span> {{ doc.docNum }}</div>
        <div><span class="lbl">วันที่</span> {{ formatDate(doc.docDate) }}</div>
        <div><span class="lbl">ผู้ขอเบิก</span> {{ doc.employeeNameTh }}</div>
        <div><span class="lbl">ตำแหน่ง</span> {{ doc.positionTh || '-' }}</div>
        <div class="col-span-2">
          <span class="lbl">ระดับตำแหน่ง</span>
          {{ doc.positionLevel || '-' }} {{ doc.positionLevelNameTh ? ': ' + doc.positionLevelNameTh : '' }}
        </div>
      </div>

      <!-- ตารางรายการ -->
      <table class="lines">
        <thead>
          <tr>
            <th style="width: 12mm">ลำดับ</th>
            <th v-if="config.showBenefit" style="width: 50mm">สวัสดิการ</th>
            <th>รายละเอียด</th>
            <th style="width: 32mm">จำนวนเงิน (บาท)</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(line, i) in doc.lines" :key="line.id ?? i">
            <!-- <td class="c">{{ i + 1 }}</td> -->
            <td v-if="config.showBenefit">{{ line.benefitNameTh || '-' }}</td>
            <td>{{ line.detail || '-' }}</td>
            <td class="r">{{ formatAmount(line.amount) }}</td>
          </tr>
          <tr v-for="n in emptyRows" :key="'e' + n" class="empty">
            <td>&nbsp;</td>
            <td v-if="config.showBenefit"></td>
            <td></td>
            <td></td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td :colspan="config.showBenefit ? 3 : 2" class="r total-lbl">รวมทั้งสิ้น</td>
            <td class="r total-val">{{ formatAmount(doc.totalAmount) }}</td>
          </tr>
        </tfoot>
      </table>

      <p v-if="doc.remark" class="remark"><span class="lbl">หมายเหตุ</span> {{ doc.remark }}</p>

      <!-- ลายเซ็น -->
      <section class="sign-grid">
        <div class="sign-box">
          <div class="sign-line"></div>
          <p>({{ doc.employeeNameTh }})</p>
          <p class="role">ผู้ขอเบิก</p>
          <p class="date">วันที่ {{ formatDate(doc.docDate) }}</p>
        </div>
        <div v-for="n in 3" :key="n" class="sign-box">
          <div class="sign-line"></div>
          <p>(................................)</p>
          <p class="role">ผู้อนุมัติ</p>
          <p class="date">วันที่ ......./......./.......</p>
        </div>
      </section>
    </article>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { Printer } from 'lucide-vue-next'
import { printDocumentRegistry, COMPANY_NAME } from '../../config/printDocuments'

const route = useRoute()
const docType = route.params.docType as string
const id = route.params.id as string

const config = printDocumentRegistry[docType] ?? { title: 'ใบเบิก', showBenefit: false, load: async () => null }
const doc = ref<any>(null)
const isLoading = ref(true)
const errorMessage = ref('')

const MIN_ROWS = 8
const emptyRows = computed(() => Math.max(0, MIN_ROWS - (doc.value?.lines?.length ?? 0)))

function formatDate(d: string) {
  if (!d) return '-'
  return new Date(d).toLocaleDateString('en-GB', { year: 'numeric', month: 'numeric', day: 'numeric' })
}

function formatAmount(n: number | null | undefined) {
  return Number(n || 0).toLocaleString('th-TH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

function printPage() { window.print() }
function closeWindow() { window.close() }

onMounted(async () => {
  if (!printDocumentRegistry[docType]) {
    errorMessage.value = 'ไม่รู้จักประเภทเอกสาร'
    isLoading.value = false
    return
  }
  try {
    doc.value = await config.load(id)
    if (!doc.value) errorMessage.value = 'ไม่พบเอกสาร'
  } catch (err) {
    console.error('Failed to load print document:', err)
    errorMessage.value = 'โหลดเอกสารไม่สำเร็จ'
  } finally {
    isLoading.value = false
  }
})
</script>

<style scoped>
.print-root { background: #f3f4f6; min-height: 100vh; padding: 0 12px 24px; }

.sheet {
  width: 210mm; min-height: 297mm; margin: 0 auto; background: #fff; color: #000;
  padding: 12mm; box-shadow: 0 1px 6px rgba(0, 0, 0, 0.15);
  font-size: 13px; line-height: 1.5;
}
.company { font-size: 14px; font-weight: 600; }
.title { font-size: 20px; font-weight: 700; margin: 4px 0 14px; }

.info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 4px 16px; margin-bottom: 12px; }
.lbl { font-weight: 600; margin-right: 6px; }

.lines { width: 100%; border-collapse: collapse; }
.lines th, .lines td { border: 1px solid #000; padding: 4px 6px; }
.lines th { background: #f0f0f0; text-align: center; }
.lines .c { text-align: center; }
.lines .r { text-align: right; }
.lines .empty td { height: 26px; }
.total-lbl { font-weight: 600; }
.total-val { font-weight: 700; }

.remark { margin-top: 10px; }

.sign-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; margin-top: 36px; text-align: center; }
.sign-box { font-size: 12px; }
.sign-line { border-bottom: 1px solid #000; height: 40px; margin: 0 10px 4px; }
.role { font-weight: 600; }
.date { margin-top: 2px; }

@media print {
  @page { size: A4; margin: 0; }
  .print-root { background: #fff; padding: 0; min-height: auto; }
  .sheet { box-shadow: none; margin: 0; }
  .no-print { display: none !important; }
}
</style>