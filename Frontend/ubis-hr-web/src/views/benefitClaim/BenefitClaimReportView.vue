<template>
  <div class="dashboard-page space-y-6 p-4 md:p-6 lg:p-8">
    <!-- Hero Header -->
    <header class="dashboard-hero relative overflow-hidden rounded-3xl p-6 md:p-8">
      <div class="hero-orb hero-orb-one"></div>
      <div class="hero-orb hero-orb-two"></div>

      <div
        class="relative z-10 flex flex-col justify-between gap-5 md:flex-row md:items-center"
      >
        <div>
          <div
            class="mb-3 inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-3 py-1.5 text-xs font-medium text-white/90 backdrop-blur"
          >
            <span class="h-2 w-2 animate-pulse rounded-full bg-emerald-300"></span>
            BENEFIT MANAGEMENT
          </div>

          <h1 class="text-2xl font-bold tracking-tight text-white md:text-3xl">
            รายงานสรุปใบเบิกสวัสดิการ
          </h1>

          <p class="mt-2 max-w-xl text-sm leading-6 text-white/75">
            ภาพรวมการเบิกสวัสดิการขององค์กร วิเคราะห์ยอดเบิก สถานะเอกสาร
            และข้อมูลแยกตามบริษัทหรือพนักงาน
          </p>
        </div>

        <button
          class="hero-export-btn"
          :disabled="isExporting || store.isSummaryLoading || !s"
          @click="doExport"
        >
          <span v-if="isExporting" class="loading loading-spinner loading-xs"></span>
          <svg
            v-else
            xmlns="http://www.w3.org/2000/svg"
            width="19"
            height="19"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.8"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
            <path d="m7 10 5 5 5-5M12 15V3" />
          </svg>
          Export Excel
        </button>
      </div>

      <div class="hero-decoration">REPORT</div>
    </header>

    <!-- Filter Panel -->
    <section class="filter-panel">
      <div class="mb-4 flex items-center gap-3">
        <div class="filter-icon">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="21"
            height="21"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.8"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <path d="M4 7h16M7 12h10m-7 5h4" />
          </svg>
        </div>
        <div>
          <h2 class="font-bold text-base-content">ตัวกรองรายงาน</h2>
          <p class="text-xs text-base-content/55">กำหนดช่วงเวลาและสถานะที่ต้องการดู</p>
        </div>
      </div>

      <div
        class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-[1fr_1fr_1.2fr_auto_auto] xl:items-end"
      >
        <label class="filter-field">
          <span>ตั้งแต่วันที่</span>
          <input
            v-model="filter.dateFrom"
            type="date"
            class="input input-bordered w-full"
          />
        </label>

        <label class="filter-field">
          <span>ถึงวันที่</span>
          <input
            v-model="filter.dateTo"
            type="date"
            class="input input-bordered w-full"
          />
        </label>

        <label class="filter-field">
          <span>สถานะเอกสาร</span>
          <select v-model="filter.docStatus" class="select select-bordered w-full">
            <option v-for="o in statusOptions" :key="o.value" :value="o.value">
              {{ o.label }}
            </option>
          </select>
        </label>

        <button
          class="btn btn-primary filter-search"
          :disabled="store.isSummaryLoading"
          @click="load"
        >
          <span
            v-if="store.isSummaryLoading"
            class="loading loading-spinner loading-xs"
          ></span>
          <span v-else>⌕</span>
          ค้นหารายงาน
        </button>

        <button
          class="btn btn-ghost filter-reset"
          :disabled="store.isSummaryLoading"
          @click="
            filter.dateFrom = null;
            filter.dateTo = null;
            filter.companyId = null;
            filter.docStatus = '';
            load();
          "
        >
          ล้างตัวกรอง
        </button>
      </div>
    </section>

    <div v-if="exportError" class="alert alert-error rounded-2xl">
      {{ exportError }}
    </div>

    <!-- Loading -->
    <div
      v-if="store.isSummaryLoading && !s"
      class="flex flex-col items-center justify-center gap-3 rounded-3xl border border-base-300 bg-base-100 py-20"
    >
      <span class="loading loading-spinner loading-lg text-primary"></span>
      <p class="text-sm text-base-content/60">กำลังรวบรวมข้อมูลรายงาน...</p>
    </div>

    <!-- Error -->
    <div v-else-if="store.summaryError" class="alert alert-error rounded-2xl">
      <span>{{ store.summaryError }}</span>
      <button class="btn btn-sm" @click="load">ลองใหม่</button>
    </div>

    <template v-else-if="s">
      <div :class="{ 'opacity-60 pointer-events-none': store.isSummaryLoading }"
        class="dashboard-content space-y-6">
        <!-- Report Table -->
        <section class="report-panel overflow-hidden">
          <div class="report-panel-header report-table-heading">
            <div>
              <h2 class="report-title">รายละเอียดการเบิกสวัสดิการ</h2>
              <p class="report-subtitle">ตรวจสอบข้อมูลแยกตามบริษัทและรายบุคคล</p>
            </div>
            <span class="report-tag">DETAIL REPORT</span>
          </div>
          <div class="report-tabs-wrap">
            <div role="tablist" class="report-tabs">
              <button
                role="tab"
                type="button"
                class="report-tab"
                :class="{ 'report-tab-active': tab === 'company' }"
                :aria-selected="tab === 'company'"
                @click="setTab('company')"
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="18"
                  height="18"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  stroke-width="1.8"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                >
                  <rect x="3" y="3" width="7" height="7" rx="1" />
                  <rect x="14" y="3" width="7" height="7" rx="1" />
                  <rect x="3" y="14" width="7" height="7" rx="1" />
                  <rect x="14" y="14" width="7" height="7" rx="1" />
                </svg>
                ตามบริษัท
                <span class="report-tab-count">{{ companies.length }}</span>
              </button>

              <button
                role="tab"
                type="button"
                class="report-tab"
                :class="{ 'report-tab-active': tab === 'employee' }"
                :aria-selected="tab === 'employee'"
                @click="setTab('employee')"
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="18"
                  height="18"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  stroke-width="1.8"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                >
                  <path d="M16 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
                  <circle cx="10" cy="7" r="4" />
                  <path d="M20 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" />
                </svg>
                รายบุคคล
              </button>
            </div>

            <label v-if="tab === 'employee'" class="employee-search">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="18"
                height="18"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="1.8"
                stroke-linecap="round"
              >
                <circle cx="11" cy="11" r="8" />
                <path d="m21 21-4.35-4.35" />
              </svg>
              <input
                v-model="empSearch"
                type="text"
                placeholder="ค้นหาชื่อ หรือรหัสพนักงาน..."
              />
              <kbd>⌕</kbd>
            </label>
          </div>

          <!-- Company Tab -->
          <div v-if="tab === 'company'" class="px-4 pb-5 md:px-6">
            <div v-if="!companies.length" class="empty-state">
              <span class="empty-state-icon">∅</span>
              <strong>ไม่พบข้อมูลรายงาน</strong>
              <p>ลองเปลี่ยนช่วงวันที่หรือเงื่อนไขการค้นหา</p>
            </div>

            <div v-for="c in companies" :key="c.company" class="company-section">
              <div class="company-heading">
                <div class="company-avatar">
                  {{ c.company?.trim().charAt(0) || "C" }}
                </div>
                <div class="min-w-0 flex-1">
                  <p class="company-caption">COMPANY</p>
                  <h3 class="company-name">{{ c.company }}</h3>
                </div>
                <span class="company-document-count">
                  {{ c.docCount.toLocaleString() }} เอกสาร
                </span>
              </div>

              <div class="overflow-x-auto">
                <table class="table report-table">
                  <thead>
                    <tr >
                      <th>สาขา / รายการสวัสดิการ</th>
                      <th class="text-right">จำนวนเงิน (บาท)</th>
                    </tr>
                  </thead>
                  <tbody>
                    <template v-for="br in c.branches" :key="br.branch">
                      <tr class="bg-base-200/50">
                        <td class="font-bold">สาขา: {{ br.branch }}</td>
                        <td class="text-right font-bold">{{ money(br.amount) }}</td>
                      </tr>
                      <tr v-for="b in br.benefits" :key="br.branch + b.benefit">
                        <td class="pl-8 font-medium">{{ b.benefit }}</td>
                        <td class="text-right">{{ money(b.amount) }}</td>
                      </tr>
                    </template>
                  </tbody>
                  <tfoot>
                    <tr>
                      <td>รวม {{ c.company }}</td>
                      <td class="text-right">{{ money(c.amount) }}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>

            <div v-if="companies.length" class="grand-total">
              <div>
                <p class="text-xs font-medium text-white/70">GRAND TOTAL</p>
                <p class="mt-1 text-lg font-bold text-white">รวมทั้งหมด</p>
              </div>
              <div class="text-right">
                <p class="text-sm text-white/80">
                  {{ grand.docCount.toLocaleString() }} เอกสาร ·
                  {{ grand.lineCount.toLocaleString() }} รายการ
                </p>
                <p class="mt-1 text-xl font-bold text-white md:text-2xl">
                  ฿ {{ money(grand.amount) }}
                </p>
              </div>
            </div>
          </div>

          <!-- Employee Tab -->
          <div v-else class="px-4 pb-5 md:px-6">
            <div
              v-if="store.isEmployeeLoading && !store.employeeItems.length"
              class="flex flex-col items-center justify-center gap-3 py-16"
            >
              <span class="loading loading-spinner loading-lg text-primary"></span>
              <p class="text-sm text-base-content/55">กำลังโหลดข้อมูลพนักงาน...</p>
            </div>

            <div v-else-if="store.employeeError" class="alert alert-error rounded-2xl">
              <span>{{ store.employeeError }}</span>
              <button class="btn btn-sm" @click="loadEmployees">ลองใหม่</button>
            </div>

            <div v-else-if="!store.employeeItems.length" class="empty-state">
              <span class="empty-state-icon">⌕</span>
              <strong>ไม่พบข้อมูลพนักงาน</strong>
              <p>ตรวจสอบคำค้นหาหรือเปลี่ยนเงื่อนไขการค้นหา</p>
            </div>

            <template v-else>
              <div class="mb-3 flex flex-wrap items-center justify-between gap-2 text-sm">
                <p class="text-base-content/60">
                  พบพนักงานทั้งหมด
                  <strong class="text-base-content">
                    {{ store.employeeTotal.toLocaleString() }}
                  </strong>
                  คน
                </p>
                <p class="text-xs text-base-content/50">
                  คลิกที่พนักงานเพื่อดูรายละเอียด
                </p>
              </div>

              <div
                class="overflow-x-auto"
                :class="{ 'opacity-60 pointer-events-none': store.isEmployeeLoading }"
              >
                <table class="table report-table employee-table">
                  <thead>
                    <tr>
                      <th class="w-10"></th>
                      <th>รหัสพนักงาน</th>
                      <th>ชื่อ-นามสกุล</th>
                      <th>ตำแหน่ง</th>
                      <th>บริษัท</th>
                      <!-- <th class="text-right">จำนวนเอกสาร</th> -->
                      <th class="text-right">ยอดรวม (บาท)</th>
                    </tr>
                  </thead>
                  <tbody>
                    <template v-for="e in store.employeeItems" :key="e.employeeId">
                      <tr class="employee-row" @click="toggleEmployee(e)">
                        <td>
                          <span
                            class="expand-button"
                            :class="{ 'expand-button-open': open[e.employeeId] }"
                          >
                            <svg
                              xmlns="http://www.w3.org/2000/svg"
                              width="16"
                              height="16"
                              viewBox="0 0 24 24"
                              fill="none"
                              stroke="currentColor"
                              stroke-width="2"
                              stroke-linecap="round"
                              stroke-linejoin="round"
                            >
                              <path d="m9 18 6-6-6-6" />
                            </svg>
                          </span>
                        </td>
                        <td>
                          <span class="employee-code">{{ e.employeeCode }}</span>
                        </td>
                        <td>
                          <div class="flex items-center gap-3">
                            <div class="employee-avatar">
                              {{ e.employeeName?.trim().charAt(0) || "?" }}
                            </div>
                            <span class="font-semibold">{{ e.employeeName }}</span>
                          </div>
                        </td>
                        <td>ตำแหน่ง</td>
                        <td>{{ e.company }}</td>
                        <!-- <td class="text-right">{{ e.docCount.toLocaleString() }}</td> -->
                        <td class="text-right font-semibold">{{ money(e.amount) }}</td>
                      </tr>

                      <tr v-if="open[e.employeeId]" class="detail-row">
                        <td></td>
                        <td colspan="5">
                          <div class="detail-container">
                            <div class="mb-3 flex items-center gap-2">
                              <span class="h-5 w-1 rounded-full bg-primary"></span>
                              <h4 class="font-bold">ประวัติการเบิกสวัสดิการ</h4>
                            </div>

                            <div
                              v-if="rowsState[e.employeeId]?.loading"
                              class="flex items-center gap-2 py-5 text-sm text-base-content/60"
                            >
                              <span
                                class="loading loading-spinner loading-sm text-primary"
                              ></span>
                              กำลังโหลดรายละเอียด...
                            </div>

                            <div
                              v-else-if="rowsState[e.employeeId]?.error"
                              class="text-error text-sm py-3"
                            >
                              {{ rowsState[e.employeeId].error }}
                              <button
                                class="btn btn-xs ml-2"
                                @click.stop="
                                  rowsState[e.employeeId] = {
                                    loading: true,
                                    error: '',
                                    rows: [],
                                  };
                                  toggleEmployee(e);
                                "
                              >
                                ลองใหม่
                              </button>
                            </div>

                            <div
                              v-else-if="!rowsState[e.employeeId]?.rows.length"
                              class="py-5 text-center text-sm text-base-content/50"
                            >
                              ไม่พบรายละเอียดการเบิก
                            </div>

                            <div v-else class="overflow-x-auto">
                              <table class="table table-sm report-table detail-table">
                                <thead>
                                  <tr>
                                    <th>เลขที่เอกสาร</th>
                                    <th>วันที่</th>
                                    <th>สถานะ</th>
                                    <th>สวัสดิการ</th>
                                    <th>รายละเอียด</th>
                                    <!-- <th class="text-right">จำนวน</th> -->
                                    <th class="text-right">จำนวนเงิน (บาท)</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  <tr
                                    v-for="(r, i) in rowsState[e.employeeId]?.rows ?? []"
                                    :key="r.docNum + i"
                                  >
                                    <td class="font-medium">{{ r.docNum }}</td>
                                    <td>{{ fmtDate(r.docDate) }}</td>
                                    <td>
                                      <span
                                        class="status-badge"
                                        :style="{
                                          '--status-color':
                                            statusColor[r.docStatus] || '#64748b',
                                        }"
                                      >
                                        <span class="status-badge-dot"></span>
                                        {{ statusLabel[r.docStatus] ?? r.docStatus }}
                                      </span>
                                    </td>
                                    <td>{{ r.benefitName }}</td>
                                    <td class="min-w-40 whitespace-normal">
                                      {{ r.detail }}
                                    </td>
                                    <!-- <td class="text-right">{{ r.qty }}</td> -->
                                    <td class="text-right font-semibold">
                                      {{ money(r.amount) }}
                                    </td>
                                  </tr>
                                </tbody>
                              </table>
                            </div>
                          </div>
                        </td>
                      </tr>
                    </template>
                  </tbody>
                </table>
              </div>

              <div class="report-pagination">
                <span class="text-sm text-base-content/60">
                  ทั้งหมด {{ store.employeeTotal.toLocaleString() }} คน
                </span>
                <div class="join">
                  <button
                    class="join-item btn btn-sm"
                    :disabled="empPage <= 1 || store.isEmployeeLoading"
                    @click="goPage(empPage - 1)"
                  >
                    ‹
                  </button>
                  <button class="join-item btn btn-sm pagination-current">
                    {{ empPage }} / {{ totalPages }}
                  </button>
                  <button
                    class="join-item btn btn-sm"
                    :disabled="empPage >= totalPages || store.isEmployeeLoading"
                    @click="goPage(empPage + 1)"
                  >
                    ›
                  </button>
                </div>
              </div>
            </template>
          </div>
        </section>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from "vue";
import { useBenefitClaimStore } from "../../stores/benefitClaimStore";
import type { BenefitClaimDetailRow, EmployeeSummary } from "../../types/BenefitClaim";

const store = useBenefitClaimStore();

const PAGE_SIZE = 20;
const year = new Date().getFullYear();
const filter = reactive({
  dateFrom: `${year}-01-01` as string | null,
  dateTo: `${year}-12-31` as string | null,
  companyId: null as string | null,
  docStatus: "" as string,
});

const statusOptions = [
  { value: "", label: "ทุกสถานะ (ยกเว้นแบบร่าง)" },
  { value: "WaitApprove", label: "รออนุมัติ" },
  { value: "Approved", label: "อนุมัติแล้ว" },
  { value: "Rejected", label: "ตีกลับ" },
  { value: "Disapproved", label: "ไม่อนุมัติ" },
];
const statusLabel: Record<string, string> = {
  Draft: "แบบร่าง",
  WaitApprove: "รออนุมัติ",
  Approved: "อนุมัติแล้ว",
  Rejected: "ตีกลับ",
  Disapproved: "ไม่อนุมัติ",
};
const statusColor: Record<string, string> = {
  Draft: "#9ca3af",
  WaitApprove: "#f59e0b",
  Approved: "#22c55e",
  Rejected: "#f97316",
  Disapproved: "#ef4444",
};

function buildPayload() {
  return {
    dateFrom: filter.dateFrom || null,
    dateTo: filter.dateTo || null,
    companyId: filter.companyId,
    docStatus: filter.docStatus || null,
  };
}

// เงื่อนไขที่ใช้ค้นหาล่าสุด (ทุก request ต่อจากนี้ใช้ชุดนี้ ไม่ใช้ค่าที่กำลังพิมพ์ในฟอร์ม)
const applied = ref(buildPayload());

const tab = ref<"company" | "employee">("company");
const isExporting = ref(false);
const exportError = ref("");

// ---- รายบุคคล ----
const empSearch = ref("");
const empPage = ref(1);
const employeesLoaded = ref(false);
const open = reactive<Record<string, boolean>>({});
const rowsState = reactive<
  Record<string, { loading: boolean; error: string; rows: BenefitClaimDetailRow[] }>
>({});

const totalPages = computed(() =>
  Math.max(1, Math.ceil(store.employeeTotal / PAGE_SIZE))
);

function clearExpanded() {
  Object.keys(open).forEach((k) => delete open[k]);
  Object.keys(rowsState).forEach((k) => delete rowsState[k]);
}

async function loadEmployees() {
  employeesLoaded.value = true;
  clearExpanded();
  await store.fetchEmployees({
    ...applied.value,
    search: empSearch.value.trim() || null,
    page: empPage.value,
    pageSize: PAGE_SIZE,
  });
}

async function toggleEmployee(e: EmployeeSummary) {
  const id = e.employeeId;
  open[id] = !open[id];
  if (!open[id] || rowsState[id]?.rows.length) return;

  rowsState[id] = { loading: true, error: "", rows: [] };
  try {
    rowsState[id].rows = await store.fetchEmployeeRows(applied.value, id);
  } catch (err) {
    rowsState[id].error = "โหลดรายการไม่สำเร็จ กรุณาลองใหม่";
    console.error(err);
  } finally {
    rowsState[id].loading = false;
  }
}

function goPage(p: number) {
  if (p < 1 || p > totalPages.value) return;
  empPage.value = p;
  loadEmployees();
}

let searchTimer: ReturnType<typeof setTimeout> | undefined;
watch(empSearch, () => {
  clearTimeout(searchTimer);
  searchTimer = setTimeout(() => {
    empPage.value = 1;
    loadEmployees();
  }, 400);
});

function setTab(t: "company" | "employee") {
  tab.value = t;
  if (t === "employee" && !employeesLoaded.value) loadEmployees();
}

// ---- โหลดข้อมูลหลัก ----
async function load() {
  applied.value = buildPayload();
  employeesLoaded.value = false;
  empPage.value = 1;
  clearExpanded();
  const jobs: Promise<unknown>[] = [store.fetchSummary(applied.value)];
  if (tab.value === "employee") jobs.push(loadEmployees());
  await Promise.all(jobs);
}

async function doExport() {
  isExporting.value = true;
  exportError.value = "";
  try {
    await store.exportSummary(applied.value);
  } catch (err) {
    exportError.value = "Export ไม่สำเร็จ กรุณาลองใหม่";
    console.error(err);
  } finally {
    isExporting.value = false;
  }
}

onMounted(load);

// ---- ข้อมูลแสดงผล ----
const s = computed(() => store.summary);
const money = (n: number) =>
  n.toLocaleString("th-TH", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
const fmtDate = (d: string) => new Date(d).toLocaleDateString("en-GB");

const companies = computed(() => s.value?.byCompany ?? []);
const grand = computed(() => ({
  docCount: companies.value.reduce((a, c) => a + c.docCount, 0),
  lineCount: companies.value.reduce((a, c) => a + c.lineCount, 0),
  amount: companies.value.reduce((a, c) => a + c.amount, 0),
}));
</script>
<style scoped src="../../assets/css/BenefitClaimReportView.css"></style>
