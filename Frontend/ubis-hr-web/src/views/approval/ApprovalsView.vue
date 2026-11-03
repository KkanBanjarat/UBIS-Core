<template>
  <main class="approval-page">
    <header class="approval-hero">
      <div class="approval-hero__intro">
        <div class="approval-hero__icon">
          <ClipboardCheck :size="26" />
        </div>
        <div>
          <p class="approval-eyebrow">UBIS CORE · APPROVAL CENTER</p>
          <h1>รายการรออนุมัติ</h1>
          <p class="approval-hero__description">
            ตรวจสอบรายละเอียดและจัดการเอกสารที่ถึงคิวให้คุณอนุมัติ
          </p>
        </div>
      </div>
      <div v-if="initialLoaded && !errorMessage" class="approval-summary" role="status">
        <span class="approval-summary__label"><span></span>{{ hasActiveFilter ? 'ตรงเงื่อนไข' : 'รอการพิจารณา' }}</span>
        <div>
          <strong>{{ items.length }}</strong><span>รายการ</span>
        </div>
      </div>
    </header>

    <section v-if="showFilters" class="approval-filters" aria-label="ค้นหาและกรองเอกสาร">
      <div class="approval-section-heading">
        <div>
          <Search :size="17" />
          <h2>ค้นหาเอกสาร</h2>
        </div>
        <button class="approval-clear" type="button" :disabled="!hasActiveFilter" @click="clearFilter">
          <FilterX :size="16" />ล้างตัวกรอง
        </button>
      </div>
      <div class="approval-filter-grid">
        <label class="approval-field approval-field--search">
          <span>ค้นหา</span>
          <div class="approval-search">
            <Search :size="18" /><input v-model="filter.search" type="search"
              placeholder="เลขที่เอกสาร ชื่อผู้ขอ หรือรายละเอียด" />
          </div>
        </label>
        <div class="approval-field">
          <span id="approval-doc-type-label">ประเภทเอกสาร</span>
          <div role="group" aria-labelledby="approval-doc-type-label">
            <FormSelect v-model="filter.docType" :options="docTypeOptions" placeholder="ประเภทเอกสารทั้งหมด" />
          </div>
        </div>
        <label class="approval-field"><span>วันที่เริ่มต้น</span><input v-model="filter.dateFrom" type="date"
            :max="filter.dateTo || undefined" /></label>
        <label class="approval-field"><span>วันที่สิ้นสุด</span><input v-model="filter.dateTo" type="date"
            :min="filter.dateFrom || undefined" /></label>
      </div>
    </section>

    <section v-if="isLoading && !initialLoaded" class="approval-loading" aria-busy="true"
      aria-label="กำลังโหลดรายการรออนุมัติ">
      <span class="approval-sr-only">กำลังโหลดรายการรออนุมัติ</span>
      <div v-for="i in 3" :key="i" class="approval-skeleton-card" aria-hidden="true">
        <div class="approval-skeleton approval-skeleton--short"></div>
        <div class="approval-skeleton"></div>
        <div class="approval-skeleton approval-skeleton--wide"></div>
      </div>
    </section>
    <section v-else-if="items.length === 0 && !hasActiveFilter" class="approval-state">
      <div class="approval-state__icon approval-state__icon--error">
        <X :size="28" />
      </div>
      <h2>โหลดรายการไม่สำเร็จ</h2>
      <p>{{ errorMessage }}</p>
      <button class="approval-button approval-button--primary" type="button" @click="approvalStore.fetchMyPending()">
        ลองใหม่อีกครั้ง
      </button>
    </section>
    <section v-else-if="items.length === 0" class="approval-state">
      <div class="approval-state__icon">
        <ClipboardCheck :size="32" />
      </div>
      <h2>ไม่มีรายการรออนุมัติ</h2>
      <p>เมื่อมีเอกสารถึงคิวให้คุณพิจารณา รายการจะแสดงที่หน้านี้</p>
    </section>
    <section v-else-if="items.length === 0" class="approval-state">
      <div class="approval-state__icon">
        <Search :size="30" />
      </div>
      <h2>ไม่พบเอกสารที่ค้นหา</h2>
      <p>ลองเปลี่ยนคำค้นหา ประเภทเอกสาร หรือช่วงวันที่</p>
      <button type="button" class="approval-button approval-button--outline" @click="clearFilter">
        ล้างตัวกรอง
      </button>
    </section>
    <section v-else class="approval-results" aria-labelledby="approval-results-title">
      <div class="approval-results__heading">
        <h2 id="approval-results-title">เอกสารที่รอคุณพิจารณา</h2>
        <span role="status">พบ <strong>{{ items.length }}</strong> รายการ</span>
      </div>
      <div class="approval-list" :style="isLoading ? { opacity: 0.6 } : undefined">
        <article v-for="item in items" :key="item.transApproveId" class="approval-card">
          <div class="approval-card__top">
            <div class="approval-document">
              <div class="approval-document__icon">
                <ClipboardCheck :size="23" />
              </div>
              <div class="approval-document__content">
                <div class="approval-document__type">
                  <h3>{{ docTypeLabel(item.docType) }}</h3>
                  <span class="approval-status"><span></span>รออนุมัติ</span>
                </div>
                <button class="approval-document__number" type="button" @click="openDetail(item)">
                  <span>{{ item.docNumber }}</span>
                  <ExternalLink :size="15" /><span class="approval-sr-only">เปิดรายละเอียดเอกสาร</span>
                </button>
              </div>
            </div>
            <button class="approval-detail-link" type="button" @click="openDetail(item)">
              ดูรายละเอียด
              <ExternalLink :size="15" />
            </button>
          </div>
          <dl class="approval-meta">
            <div class="approval-meta__item">
              <span class="approval-meta__icon">
                <User :size="18" />
              </span>
              <div>
                <dt>ผู้ขอเอกสาร</dt>
                <dd>{{ item.employeeNameTh || "ไม่ระบุผู้ขอ" }}</dd>
              </div>
            </div>
            <div class="approval-meta__item">
              <span class="approval-meta__icon">
                <CalendarDays :size="18" />
              </span>
              <div>
                <dt>วันที่เอกสาร</dt>
                <dd>{{ formatDate(item.docDate) }}</dd>
              </div>
            </div>
            <div class="approval-meta__item approval-meta__item--note">
              <span class="approval-meta__icon">
                <MessageSquare :size="18" />
              </span>
              <div>
                <dt>รายละเอียด</dt>
                <dd class="approval-note">
                  {{ item.documentDetail || "ไม่มีรายละเอียดเพิ่มเติม" }}
                </dd>
              </div>
            </div>
          </dl>
          <footer class="approval-card__footer">
            <span class="approval-card__hint"><span></span>รอการพิจารณาจากคุณ</span>
            <div class="approval-actions">
              <button class="approval-button approval-button--danger" type="button"
                @click="openDenial(item, 'disapprove')">
                <X :size="17" />ไม่อนุมัติ
              </button>
              <button class="approval-button approval-button--warning" type="button"
                @click="openDenial(item, 'reject')">
                <Undo2 :size="17" />ตีกลับ
              </button>
              <button class="approval-button approval-button--primary" type="button" @click="confirmApprove(item)">
                <Check :size="18" />อนุมัติ
              </button>
            </div>
          </footer>
        </article>
      </div>
    </section>

    <dialog ref="rejectDialogRef" class="modal approval-dialog" aria-labelledby="approval-denial-title"
      aria-describedby="approval-denial-description">
      <div class="modal-box approval-dialog__box">
        <div class="approval-dialog__header">
          <div class="approval-dialog__icon" :class="denialMode === 'reject' ? 'is-warning' : 'is-danger'">
            <component :is="denialMode === 'reject' ? Undo2 : X" :size="24" />
          </div>
          <div>
            <h2 id="approval-denial-title">
              {{
                denialMode === "reject" ? "ตีกลับเอกสาร" : "ไม่อนุมัติเอกสาร"
              }}
            </h2>
            <p class="approval-dialog__number">{{ rejectTarget?.docNumber }}</p>
          </div>
        </div>
        <p id="approval-denial-description" class="approval-dialog__description">
          {{
            denialMode === "reject"
              ? "ผู้ขอเบิกจะสามารถแก้ไขและส่งเอกสารกลับมาใหม่ได้"
              : "เอกสารจะสิ้นสุดและไม่สามารถแก้ไขได้อีก"
          }}
        </p>
        <label class="approval-field"><span>เหตุผล <span class="approval-required">*</span></span><textarea
            v-model="rejectReason" rows="4" placeholder="กรุณาระบุเหตุผลเพื่อให้ผู้ขอทราบ" required></textarea>
        </label>
        <p class="approval-dialog__help">เหตุผลนี้จะแสดงให้ผู้ขอเอกสารทราบ</p>
        <div class="approval-dialog__actions">
          <button class="approval-button approval-button--outline" type="button" @click="rejectDialogRef?.close()">
            ยกเลิก</button><button class="approval-button" :class="denialMode === 'reject'
              ? 'approval-button--warning-solid'
              : 'approval-button--danger-solid'
              " :disabled="!rejectReason.trim() || isSubmittingDenial" type="button" @click="submitDenial">
            {{
              isSubmittingDenial
                ? "กำลังบันทึก..."
                : denialMode === "reject"
                  ? "ยืนยันตีกลับ"
                  : "ยืนยันไม่อนุมัติ"
            }}
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button aria-label="ปิดหน้าต่าง">ปิด</button>
      </form>
    </dialog>
    <ApprovalDetailModal ref="detailModalRef" @approve="confirmApprove" @reject="openDenial($event, 'reject')"
      @disapprove="openDenial($event, 'disapprove')" />
  </main>
</template>

<script setup lang="ts">
import ApprovalDetailModal from "../../components/approval/ApprovalDetailModal.vue";
import FormSelect from "../../components/ui/FormSelect.vue";
import { approvalDocumentRegistry } from "../../config/approvalDocuments";
import { ref, reactive, computed, onMounted, onBeforeUnmount, watch, nextTick } from 'vue'
import { useRoute, useRouter } from "vue-router";
import { storeToRefs } from "pinia";
import { useApprovalStore } from "../../stores/approvalStore.ts";
import { notify, extractErrorMessage } from "../../utils/notify";
import type { Approval, ApprovalFilter } from '../../types/Approval'
import {
  ClipboardCheck,
  Check,
  X,
  Undo2,
  User,
  CalendarDays,
  ExternalLink,
  MessageSquare,
  Search,
  FilterX,
} from "lucide-vue-next";
const route = useRoute();
const router = useRouter();
const approvalStore = useApprovalStore();
const { items, isLoading, errorMessage } = storeToRefs(approvalStore);
const rejectDialogRef = ref<HTMLDialogElement>();
const rejectReason = ref("");
const isSubmittingDenial = ref(false);
const rejectTarget = ref<Approval | null>(null);
const denialMode = ref<"reject" | "disapprove">("reject");
const detailModalRef = ref<InstanceType<typeof ApprovalDetailModal>>();
function openDetail(item: Approval) {
  detailModalRef.value?.open(item.docType, item.docNumber, item);
}
function docTypeLabel(docType: string) {
  return approvalDocumentRegistry[docType]?.label ?? docType;
}
function formatDate(d: string | null | undefined) {
  if (!d) return "ไม่ระบุวันที่";
  const date = new Date(d);
  if (Number.isNaN(date.getTime())) return "ไม่ระบุวันที่";
  return date.toLocaleDateString("th-TH", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}
// ========================================
// Filter (กรองที่ฐานข้อมูลผ่าน API my-pending)
// ========================================
const filter = reactive({
  search: '',
  docType: null as string | null,
  dateFrom: '',
  dateTo: '',
})
const initialLoaded = ref(false)
let searchTimer: ReturnType<typeof setTimeout> | undefined
let silent = false // กันไม่ให้ watch ยิง API ตอนรีเซ็ตด้วยโค้ด

// ตัวเลือกประเภทเอกสารมาจาก registry ไม่ใช่จากผลลัพธ์ (ไม่งั้นตัวเลือกหดเมื่อกรอง)
const docTypeOptions = computed(() =>
  Object.keys(approvalDocumentRegistry).map(t => ({ id: t, label: docTypeLabel(t) }))
)
const hasActiveFilter = computed(
  () => !!filter.search.trim() || !!filter.docType || !!filter.dateFrom || !!filter.dateTo
)
// ยังโชว์แถบ Filter ตราบที่มีรายการ หรือมี Filter ค้างอยู่ (กันปุ่มล้างหาย)
const showFilters = computed(
  () => !errorMessage.value && (items.value.length > 0 || hasActiveFilter.value)
)

function buildParams(): ApprovalFilter {
  return {
    search: filter.search.trim(),
    docType: filter.docType ?? '',
    dateFrom: filter.dateFrom,
    dateTo: filter.dateTo,
  }
}
function reload() {
  return approvalStore.fetchMyPending(buildParams())
}

// พิมพ์ค้นหา: รอ 400 ms หลังหยุดพิมพ์ / ตัวเลือกอื่น: ยิงทันที
watch(() => filter.search, () => {
  if (silent) return
  clearTimeout(searchTimer)
  searchTimer = setTimeout(reload, 400)
})
watch(() => [filter.docType, filter.dateFrom, filter.dateTo], () => {
  if (silent) return
  clearTimeout(searchTimer)
  reload()
})
onBeforeUnmount(() => clearTimeout(searchTimer))

function clearFilter() {
  filter.search = ''
  filter.docType = null
  filter.dateFrom = ''
  filter.dateTo = ''
}
async function resetFilterSilently() {
  clearTimeout(searchTimer)
  silent = true
  clearFilter()
  await nextTick()
  silent = false
}
async function confirmApprove(item: Approval) {
  const ok = await notify.confirm(
    `ยืนยันอนุมัติเอกสาร "${item.docNumber}" ใช่หรือไม่`,
    "ยืนยันการอนุมัติ",
  );
  if (!ok) return;
  try {
    await approvalStore.approve(item.transApproveId);
    await notify.success("อนุมัติสำเร็จ");
  } catch (err) {
    await notify.error(extractErrorMessage(err), "อนุมัติไม่สำเร็จ");
  }
}
function openDenial(item: Approval, mode: "reject" | "disapprove") {
  rejectTarget.value = item;
  denialMode.value = mode;
  rejectReason.value = "";
  rejectDialogRef.value?.showModal();
}
async function submitDenial() {
  if (
    isSubmittingDenial.value ||
    !rejectTarget.value ||
    !rejectReason.value.trim()
  ) {
    return;
  }
  isSubmittingDenial.value = true;
  try {
    await approvalStore.deny(
      rejectTarget.value.transApproveId,
      rejectReason.value.trim(),
      denialMode.value === "disapprove",
    );
    rejectDialogRef.value?.close();
    await notify.success(
      denialMode.value === "reject"
        ? "ตีกลับเอกสารสำเร็จ"
        : "ไม่อนุมัติเอกสารสำเร็จ",
    );
  } catch (err) {
    await notify.error(
      extractErrorMessage(err),
      denialMode.value === "reject" ? "ตีกลับไม่สำเร็จ" : "ไม่อนุมัติไม่สำเร็จ",
      rejectDialogRef.value,
    );
  } finally {
    isSubmittingDenial.value = false;
  }
}
// เปิดรายการจากลิงก์ในการ์ด Teams: /approvals?docType=..&docNumber=..
async function handleDeepLink() {
  const docType = route.query.docType;
  const docNumber = route.query.docNumber;
  if (typeof docType !== "string" || typeof docNumber !== "string") return;
  // โหลดรายการไม่สำเร็จ ห้ามสรุปว่า "ไม่ได้รออนุมัติแล้ว"
  if (errorMessage.value) return;
  // ล้าง query กันเปิดซ้ำเมื่อ refresh
  await router.replace({ query: {} });
  const item = items.value.find(
    (x) => x.docType === docType && x.docNumber === docNumber,
  );
  if (item) {
    openDetail(item); // เปิดพร้อมปุ่มอนุมัติ
    return;
  }
  await notify.info(
    "รายการนี้ไม่ได้อยู่ในสถานะรออนุมัติของคุณแล้ว (อาจถูกดึงกลับ อนุมัติไปแล้ว หรือไม่ใช่คิวของคุณ)",
  );
  detailModalRef.value?.open(docType, docNumber); // เปิดแบบอ่านอย่างเดียว ไม่มีปุ่ม
}
onMounted(async () => {
  await reload() // ส่ง filter เสมอ เพื่อล้างค่าเก่าที่ค้างใน store
  initialLoaded.value = true
  await handleDeepLink()
})

// กรณีอยู่หน้านี้อยู่แล้วแล้วกดการ์ดใบอื่นจาก Teams
watch(() => route.query.docNumber, async (v) => {
  if (!v) return
  await resetFilterSilently() // ลิงก์ต้องหารายการเจอ จึงต้องไม่มี filter ค้าง
  await reload()
  await handleDeepLink()
})
</script>

<style scoped src="../../assets/css/ApprovalPage.css"></style>
