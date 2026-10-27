<template>
  <section class="claim-detail">
    <div v-if="isLoading" class="cd-state" role="status" aria-busy="true">
      <span class="loading loading-spinner"></span>
      <p>กำลังโหลดข้อมูลเอกสาร...</p>
    </div>
    <div v-else-if="loadError" class="cd-state cd-state--error" role="alert">
      <FileQuestion :size="30" />
      <h3>โหลดข้อมูลไม่สำเร็จ</h3>
      <p>กรุณาลองใหม่อีกครั้ง</p>
      <button type="button" class="cd-retry" @click="loadDocument">
        ลองใหม่
      </button>
    </div>
    <article v-else-if="doc" class="cd-document">
      <header class="cd-header">
        <span class="cd-header__icon">
          <FileText :size="23" />
        </span>
        <div>
          <p>เอกสารเบิกสวัสดิการ</p>
          <h2>{{ props.docNumber }}</h2>
        </div>
        <span class="cd-header__count">{{ doc.lines?.length ?? 0 }} รายการ</span>
      </header>
      <dl class="cd-info">
        <div>
          <dt>ผู้ขอเบิก</dt>
          <dd>{{ doc.employeeNameTh || "-" }}</dd>
        </div>
        <div>
          <dt>วันที่ขอเบิก</dt>
          <dd class="cd-date">{{ formatDate(doc.docDate) }}</dd>
        </div>
        <div>
          <dt>ตำแหน่ง</dt>
          <dd>
            {{ doc.positionTh || "-"
            }}<span v-if="doc.positionEn" class="cd-secondary">{{
              doc.positionEn
            }}</span>
          </dd>
        </div>
        <div>
          <dt>ระดับตำแหน่ง</dt>
          <dd>
            <span class="cd-level">L{{ doc.positionLevel ?? "-" }}</span>{{ doc.positionLevelNameTh || "-"
            }}<span v-if="doc.positionLevelNameEn" class="cd-secondary">{{
              doc.positionLevelNameEn
            }}</span>
          </dd>
        </div>
      </dl>
      <aside v-if="doc.remark" class="cd-remark">
        <strong>หมายเหตุ</strong>
        <p>{{ doc.remark }}</p>
      </aside>
      <div class="cd-section-heading">
        <ReceiptText :size="18" />
        <h3>รายละเอียดรายการเบิก</h3>
        <span>จำนวนเงิน (บาท)</span>
      </div>
      <div v-if="doc.lines?.length" class="cd-table-scroll">
        <table class="cd-table">
          <caption class="cd-sr-only">
            รายการเบิกและจำนวนเงิน
          </caption>
          <thead>
            <tr>
              <th scope="col" class="cd-sequence">ลำดับ</th>
              <th scope="col">รายการ / รายละเอียด</th>
              <th scope="col" class="cd-amount">จำนวนเงิน</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(line, idx) in doc.lines" :key="line.id ?? idx">
              <td class="cd-sequence">
                <span>{{ String(idx + 1).padStart(2, "0") }}</span>
              </td>
              <td>
                <p class="cd-line-name">
                  {{ line.benefitNameTh || "รายการเบิกสวัสดิการ" }}
                </p>
                <p v-if="line.detail" class="cd-line-description">
                  {{ line.detail }}
                </p>
              </td>
              <td class="cd-amount">{{ formatAmount(line.amount) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <p v-else class="cd-empty">ไม่มีรายการเบิก</p>
      <footer class="cd-total">
        <span>ยอดรวมทั้งสิ้น</span>
        <div>
          <strong>{{ formatAmount(doc.totalAmount) }}</strong><span>บาท</span>
        </div>
      </footer>
    </article>
    <div v-else class="cd-state">
      <FileQuestion :size="32" />
      <h3>ไม่พบข้อมูลเอกสาร</h3>
      <p>ไม่พบเอกสารเลขที่ {{ props.docNumber }}</p>
    </div>
  </section>
</template>
<style scoped src="../../assets/css/ClaimDocumentDetail.css"></style>
<script setup lang="ts">
import { ref, watch, onBeforeUnmount } from "vue";
import { FileQuestion, FileText, ReceiptText } from "lucide-vue-next";
import { useBenefitClaimStore } from "../../stores/benefitClaimStore";
import type { BenefitClaim } from "../../types/BenefitClaim";

const props = defineProps<{ docNumber: string }>();
const store = useBenefitClaimStore();
const doc = ref<BenefitClaim | null>(null);
const isLoading = ref(true);
const loadError = ref(false);
let requestId = 0;

function formatDate(value: string | null | undefined, showTime = false) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  const text = new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(date);
  if (!showTime) return text;
  return `${text} ${new Intl.DateTimeFormat("en-GB", {
    hour: "2-digit",
    minute: "2-digit",
    hourCycle: "h23",
  }).format(date)}`;
}
function formatAmount(amount: number | null | undefined) {
  const number = Number(amount ?? 0);
  return Number.isFinite(number)
    ? number.toLocaleString("th-TH", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    })
    : "-";
}
async function loadDocument() {
  const id = ++requestId;
  const number = props.docNumber;
  isLoading.value = true;
  loadError.value = false;
  doc.value = null;
  try {
    if (!number) return;
    const result = await store.getByDocNum(number);
    if (id === requestId) doc.value = result;
  } catch (error) {
    if (id === requestId) loadError.value = true;
    console.error("Failed to load document detail:", error);
  } finally {
    if (id === requestId) isLoading.value = false;
  }
}
watch(() => props.docNumber, loadDocument, { immediate: true });
onBeforeUnmount(() => {
  requestId++;
});
</script>
