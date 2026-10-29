<template>
  <div class="benefit-claim-page">
    <header class="claim-hero">
      <div>
        <div class="claim-title-row">
          <div class="claim-hero-icon">
            <Gift :size="22" :stroke-width="1.7" />
          </div>
          <div>
            <h1>เบิกสวัสดิการ</h1>
            <p>สร้างใบเบิก ติดตามเอกสาร และตรวจสอบสถานะการอนุมัติ</p>
          </div>
        </div>
      </div>
      <button v-if="canWrite" type="button" class="claim-create-button" @click="openCreateModal">
        <Plus :size="18" /> สร้างใบเบิกสวัสดิการ
      </button>
    </header>
    <section class="claim-filter-card">

      <div class="claim-filter-grid">
        <div class="claim-filter-field">
          <label for="claim-search">ค้นหาเอกสาร</label>
          <div class="claim-search">
            <Search :size="18" aria-hidden="true" />
            <input id="claim-search" v-model="filter.search" @input="onSearchInput" type="search"
              placeholder="เลขที่เอกสาร ชื่อพนักงาน หรือรายละเอียด..." />
          </div>
        </div>
        <div class="claim-filter-field"><span class="claim-field-label">สถานะเอกสาร</span>
          <FormSelect v-model="filter.docStatus" :options="statusOptions" placeholder="ทุกสถานะ" />
        </div>
      </div>
    </section>
    <!-- Table Card -->
    <div class="claim-table-card">
      <div class="claim-table-toolbar">
        <div class="claim-table-heading">
          <div class="claim-table-icon">
            <Gift :size="18" />
          </div>
          <div>
            <h2>รายการใบเบิกสวัสดิการ</h2>
            <p>พบทั้งหมด <strong>{{ totalCount.toLocaleString('th-TH') }}</strong> รายการ</p>
          </div>
        </div>
        <div class="claim-page-size"><label for="claim-page-size">แสดง</label>
          <select id="claim-page-size" v-model.number="filter.pageSize">
            <option :value="10">10</option>
            <option :value="20">20</option>
            <option :value="50">50</option>
            <option :value="100">100</option>
          </select>
          <span>รายการ / หน้า</span>
        </div>
      </div>
      <div v-if="isLoading" class="p-5 space-y-2">
        <div v-for="i in 5" :key="i" class="skeleton h-14 w-full rounded-lg"></div>
      </div>

      <div v-else-if="errorMessage" class="flex flex-col items-center gap-3 py-16 text-center">
        <p class="text-sm font-medium text-error">{{ errorMessage }}</p>
        <button class="btn btn-sm btn-primary" @click="fetchList">ลองใหม่</button>
      </div>

      <template v-else>
        <div v-if="items.length === 0" class="flex flex-col items-center gap-2 py-16 text-center">
          <div class="flex size-12 items-center justify-center rounded-full bg-base-200 text-base-content/30">
            <Gift class="size-5" />
          </div>
          <p class="text-sm font-medium text-base-content/60">ไม่พบใบเบิกสวัสดิการ</p>
          <p class="text-xs text-base-content/35">ลองเปลี่ยนเงื่อนไขการค้นหา หรือสร้างใบเบิกใหม่</p>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="table w-full claim-table">
            <thead>
              <tr class="text-[11px] uppercase tracking-wide text-base-content/40 border-b border-base-200">
                <th class="bg-base-100 w-10"></th>
                <th class="bg-base-100">เลขที่เอกสาร</th>
                <th class="bg-base-100">ประเภทเอกสาร</th>
                <th class="bg-base-100">ผู้ขอเบิก</th>
                <th class="bg-base-100">ระดับตำแหน่ง</th>
                <th class="bg-base-100 text-right">ยอดรวม</th>
                <th class="bg-base-100">สถานะ</th>
                <th class="bg-base-100">แก้ไขล่าสุด</th>
                <th class="bg-base-100 text-right">จัดการ</th>
              </tr>
            </thead>
            <tbody>
              <template v-for="item in items" :key="item.id">
                <tr class="claim-document-row group border-b border-base-200/60 transition-colors"
                  :class="{ 'is-expanded': expandedRows.has(item.id) }">
                  <td class="w-10">
                    <button type="button"
                      class="btn btn-ghost btn-xs btn-square rounded-lg text-base-content/40 hover:text-primary hover:bg-primary/10"
                      :title="expandedRows.has(item.id) ? 'ซ่อนรายการ' : 'ดูรายการ'" @click="toggleExpand(item.id)">
                      <ChevronUp v-if="expandedRows.has(item.id)" class="size-4" />
                      <ChevronDown v-else class="size-4" />
                    </button>
                  </td>

                  <td class="py-3">
                    <div class="flex flex-col gap-1">
                      <p class="font-semibold text-sm text-base-content leading-tight">
                        {{ item.docNum }}
                      </p>
                      <span class="text-xs text-base-content/55">
                        วันที่เอกสาร : {{ formatDate(item.docDate) }}
                      </span>
                    </div>
                  </td>

                  <td>
                    <span v-if="item.docFormCode" class="badge badge-sm badge-outline badge-primary font-medium">
                      {{ item.docFormCode }}
                    </span>
                    <span v-else class="text-xs text-base-content/30">-</span>
                    <p v-if="item.docFormNameTh" class="text-[12px] text-base-content/50 mt-0.5 max-w-[220px] truncate"
                      :title="item.docFormNameTh">
                      {{ item.docFormNameTh }}
                    </p>
                    <p v-if="item.destination" class="text-[11px] text-base-content/40 mt-0.5 max-w-[220px] truncate"
                      :title="item.destination">
                      ปลายทาง : {{ item.destination }}
                    </p>
                  </td>

                  <td>
                    <span class="text-sm text-base-content/70">{{ item.employeeNameTh }}</span>
                    <p class="text-[12px] text-base-content/35 mt-0.5">{{ item.positionTh }}</p>
                  </td>

                  <td>
                    <span class="text-sm text-base-content/70">{{ item.positionLevel }} : {{ item.positionLevelNameTh
                    }}</span>
                    <p class="text-[12px] text-base-content/35 mt-0.5">{{ item.positionLevelNameEn }}</p>
                  </td>

                  <td class="text-right">
                    <span class="claim-amount font-semibold text-sm">{{ item.totalAmount.toLocaleString('th-TH', {
                      minimumFractionDigits: 2, maximumFractionDigits: 2
                    }) }}</span>
                    <span class="text-[11px] text-base-content/35 ml-1">บาท</span>
                  </td>

                  <td>
                    <span class="claim-status" :class="`claim-status--${item.docStatus}`">
                      <span class="claim-status-dot"></span>{{ statusLabel(item.docStatus) }}
                    </span>
                  </td>

                  <td>
                    <p class="text-xs text-base-content/60">{{ item.updatedBy }}</p>
                    <p class="text-[11px] text-base-content/35">{{ formatDateTime(item.updatedAt) }}</p>
                  </td>

                  <td>
                    <div class="claim-actions flex items-center justify-end gap-0.5">
                      <button v-if="canWrite && item.docStatus === 'Draft'"
                        class="btn btn-ghost btn-xs btn-square rounded-lg text-warning/70 hover:bg-warning/10 hover:text-warning"
                        title="แก้ไข" @click="openEditModal(item)" :disabled="editLoadingId === item.id">
                        <Edit class="size-4" />
                      </button>
                      <button v-if="canWrite && item.docStatus === 'Draft'"
                        class="btn btn-ghost btn-xs btn-square rounded-lg text-primary hover:bg-primary/10"
                        title="ส่งอนุมัติ" @click="confirmSubmit(item)">
                        <Send class="size-4" />
                      </button>
                      <button
                        class="btn btn-ghost btn-xs btn-square rounded-lg text-base-content/40 hover:bg-info/10 hover:text-info"
                        title="ดูรายละเอียดและสถานะอนุมัติ" :disabled="!item.docNum" @click="openDocumentDetail(item)">
                        <Eye class="size-3.5" />
                      </button>
                      <button
                        class="btn btn-ghost btn-xs btn-square rounded-lg text-base-content/40 hover:bg-primary/10 hover:text-primary"
                        title="พรีวิว / พิมพ์ใบเบิก" @click="openPreview(item)">
                        <Printer class="size-3.5" />
                      </button>
                      <button v-if="canWrite && (item.docStatus === 'WaitApprove' || item.docStatus === 'Rejected')"
                        class="btn btn-ghost btn-xs btn-square rounded-lg text-info hover:bg-info/10"
                        title="ดึงกลับมาเป็นร่าง" @click="confirmRecall(item)">
                        <Undo2 class="size-4" />
                      </button>
                      <button v-if="canWrite && item.docStatus === 'Draft'"
                        class="btn btn-ghost btn-xs btn-square rounded-lg text-error/60 hover:bg-error/10 hover:text-error"
                        title="ลบ" @click="confirmDelete(item)">
                        <Trash2 class="size-4" />
                      </button>
                    </div>
                  </td>
                </tr>

                <tr v-if="expandedRows.has(item.id)" class="claim-expanded-row">
                  <td colspan="9" class="p-0">
                    <div class="px-5 sm:px-12 py-4 space-y-4">
                      <!-- ข้อมูลการเดินทาง (เฉพาะฟอร์มเบี้ยเลี้ยง/ที่พัก) -->
                      <div v-if="detailMap[item.id] && hasTripInfo(detailMap[item.id])"
                        class="rounded-xl border border-base-300 bg-base-100 p-3">
                        <p class="text-base font-semibold mb-2">ข้อมูลการเดินทาง</p>
                        <div class="grid grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-1.5 text-sm">
                          <p v-if="detailMap[item.id].tripType">
                            <span class="text-base-content/40">ประเภท : </span>{{
                              tripTypeLabel(detailMap[item.id].tripType) }}
                          </p>
                          <p v-if="detailMap[item.id].destination">
                            <span class="text-base-content/40">ปลายทาง : </span>{{ detailMap[item.id].destination }}
                          </p>
                          <p v-if="detailMap[item.id].departAt">
                            <span class="text-base-content/40">ออกเดินทาง : </span>{{
                              formatDateTime(detailMap[item.id].departAt!) }}
                          </p>
                          <p v-if="detailMap[item.id].returnAt">
                            <span class="text-base-content/40">กลับ : </span>{{
                              formatDateTime(detailMap[item.id].returnAt!) }}
                          </p>
                          <p v-if="detailMap[item.id].isOvernight != null">
                            <span class="text-base-content/40">ค้างแรม : </span>{{ detailMap[item.id].isOvernight ?
                              'ใช่' : 'ไม่' }}
                          </p>
                          <p v-if="detailMap[item.id].tripPurpose" class="sm:col-span-2">
                            <span class="text-base-content/40">วัตถุประสงค์ : </span>{{ detailMap[item.id].tripPurpose
                            }}
                          </p>
                        </div>
                      </div>

                      <div>
                        <div class="flex items-center justify-between mb-3">
                          <div>
                            <p class="text-base font-semibold">รายละเอียดการเบิก</p>
                            <p class="text-sm text-base-content/40 mt-0.5">รายการสวัสดิการในใบเบิกนี้</p>
                          </div>
                          <span class="text-sm text-base-content/40">{{ detailMap[item.id]?.lines?.length ??
                            item.lineCount ?? 0 }} รายการ</span>
                        </div>

                        <div v-if="detailLoading[item.id] && !detailMap[item.id]" class="space-y-2">
                          <div v-for="i in 2" :key="i" class="skeleton h-12 w-full rounded-lg"></div>
                        </div>

                        <div v-else-if="detailError[item.id] && !detailMap[item.id]"
                          class="rounded-xl border border-dashed border-error/40 p-6 text-center text-sm text-error">
                          โหลดรายละเอียดไม่สำเร็จ
                          <button class="btn btn-xs btn-primary ml-2" @click="loadDetail(item.id)">ลองใหม่</button>
                        </div>

                        <div v-else-if="detailMap[item.id]?.lines?.length"
                          class="rounded-xl border border-base-300 bg-base-100 overflow-hidden">
                          <div v-for="(line, lineIndex) in detailMap[item.id]?.lines ?? []" :key="line.id ?? lineIndex"
                            class="border-b border-base-300/70 last:border-0">
                            <div
                              class="flex flex-col sm:grid sm:grid-cols-[40px_minmax(180px,1fr)_minmax(180px,1.5fr)_130px] gap-3 items-start sm:items-center px-4 py-3">
                              <div
                                class="flex size-7 items-center justify-center rounded-lg bg-primary/10 text-primary text-xs font-semibold">
                                {{ lineIndex + 1 }}
                              </div>
                              <div class="min-w-0">
                                <span
                                  class="badge badge-xs font-normal mb-0.5 badge-primary text-primary-content">สวัสดิการ</span>
                                <p class="text-sm font-medium truncate">{{ line.benefitNameTh || 'ไม่ระบุสวัสดิการ' }}
                                </p>
                              </div>
                              <div class="min-w-0">
                                <p class="text-xs text-base-content/40 mb-0.5">รายละเอียด</p>
                                <p class="text-sm text-base-content/65 truncate" :title="line.detail">{{ line.detail ||
                                  '-' }}</p>
                                <p v-if="lineMeta(line)" class="text-xs text-base-content/45 mt-0.5">
                                  {{ lineMeta(line) }}
                                </p>
                              </div>
                              <div class="sm:text-right">
                                <p class="text-xs text-base-content/40 mb-0.5">ยอดเบิก</p>
                                <p class="text-sm font-semibold">
                                  {{ line.amount?.toLocaleString('th-TH') ?? 0 }}
                                  <span class="text-[11px] font-normal text-base-content/40">บาท</span>
                                </p>
                              </div>
                            </div>

                            <!-- breakdown (อัตราตามช่วง) -->
                            <div v-if="line.breakdown?.length" class="px-4 pb-3 sm:pl-[68px]">
                              <table class="table table-xs w-full sm:max-w-md">
                                <thead>
                                  <tr class="text-[11px] text-base-content/45">
                                    <th>ช่วง</th>
                                    <th class="text-right">จำนวน</th>
                                    <th class="text-right">อัตรา</th>
                                    <th class="text-right">รวม (บาท)</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  <tr v-for="(r, ri) in line.breakdown" :key="ri">
                                    <td>{{ rangeText(r) }}</td>
                                    <td class="text-right">{{ r.qty }}</td>
                                    <td class="text-right">{{ r.rate.toLocaleString('th-TH') }}</td>
                                    <td class="text-right">{{ r.amount.toLocaleString('th-TH') }}</td>
                                  </tr>
                                </tbody>
                              </table>
                            </div>
                          </div>
                        </div>

                        <div v-else
                          class="rounded-xl border border-dashed border-base-300 p-6 text-center text-sm text-base-content/40">
                          ไม่มีรายละเอียดรายการ
                        </div>
                      </div>

                      <div v-if="item.remark"
                        class="flex gap-2 rounded-lg bg-base-100 border border-base-200 px-3 py-2.5">
                        <MessageSquare class="size-3.5 shrink-0 mt-0.5 text-base-content/35" />
                        <div>
                          <p class="text-[11px] text-base-content/40">หมายเหตุ</p>
                          <p class="text-xs text-base-content/65 mt-0.5">{{ item.remark }}</p>
                        </div>
                      </div>

                      <div class="rounded-xl border border-base-300 bg-base-100 p-3">
                        <p class="text-base font-semibold mb-2">ไฟล์แนบ</p>
                        <AttachmentList doc-type="BenefitClaim" :doc-number="item.docNum || null"
                          :readonly="item.docStatus !== 'Draft' || !canWrite" />
                      </div>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>

        <div
          class="flex flex-col sm:flex-row items-center justify-between gap-3 px-4 sm:px-5 py-3 border-t border-base-200">
          <p class="text-xs text-base-content/45">
            แสดง {{ totalCount === 0 ? 0 : (filter.page - 1) * filter.pageSize + 1 }}–{{ Math.min(filter.page *
              filter.pageSize,
              totalCount) }}
            จาก {{ totalCount }} รายการ
          </p>
          <div class="join">
            <button class="join-item btn btn-sm btn-ghost" :disabled="filter.page === 1" @click="goToPage(1)">
              <ChevronsLeft class="size-4" />
            </button>
            <button class="join-item btn btn-sm btn-ghost" :disabled="filter.page === 1"
              @click="goToPage(filter.page - 1)">
              <ChevronLeft class="size-4" />
            </button>
            <button v-for="p in pageWindow" :key="p" class="join-item btn btn-sm"
              :class="p === filter.page ? 'btn-primary' : 'btn-ghost'" @click="goToPage(p)">
              {{ p }}
            </button>
            <button class="join-item btn btn-sm btn-ghost" :disabled="filter.page >= totalPages"
              @click="goToPage(filter.page + 1)">
              <ChevronRight class="size-4" />
            </button>
            <button class="join-item btn btn-sm btn-ghost" :disabled="filter.page >= totalPages"
              @click="goToPage(totalPages)">
              <ChevronsRight class="size-4" />
            </button>
          </div>
        </div>
      </template>
    </div>
  </div>

  <BenefitClaimFormModal ref="formModalRef" :is-create="isCreateMode" :employee-id="myEmployeeId"
    :employee-name="myEmployeeName" :allowed-employees="allowedEmployees" @save="handleSave" />
  <ApprovalDetailModal ref="detailModalRef" />
  <BenefitClaimPreviewModal ref="previewModalRef" />
</template>

<script setup lang="ts">
import { ref, reactive, watch, onMounted, onBeforeUnmount, computed } from 'vue'
import { storeToRefs } from 'pinia'
import { useBenefitClaimStore } from '../../stores/benefitClaimStore.ts'
import { usePermission } from '../../composables/usePermission'
import FormSelect from '../../components/ui/FormSelect.vue'
import BenefitClaimFormModal from '../../components/benefitClaim/BenefitClaimFormModal.vue'
import ApprovalDetailModal from '../../components/approval/ApprovalDetailModal.vue'
import BenefitClaimPreviewModal from '../../components/benefitClaim/BenefitClaimPreviewModal.vue'
import AttachmentList from '../../components/attachment/AttachmentList.vue'
import { notify, extractErrorMessage } from '../../utils/notify.ts'
import { useAuthStore } from '../../stores/authStore.ts'
import type { BenefitClaim, BenefitClaimFilter, BenefitClaimLine } from '../../types/BenefitClaim.ts'
import { tripTypeLabel, rangeText } from '../../utils/benefitClaim.ts'
import { Plus, Search, Gift, Edit, Trash2, Send, ChevronDown, ChevronUp, MessageSquare, Undo2, Eye, ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight, Printer } from 'lucide-vue-next'

interface OptionItem {
  id: string
  label: string
}

const authStore = useAuthStore()
const benefitClaimStore = useBenefitClaimStore()
const { canWrite } = usePermission('benefit-claim')
const { items, totalCount, isLoading, errorMessage, allowedEmployees, myEmployeeId, myEmployeeName } = storeToRefs(benefitClaimStore)
const detailModalRef = ref<InstanceType<typeof ApprovalDetailModal>>()
const previewModalRef = ref<InstanceType<typeof BenefitClaimPreviewModal>>()
const expandedRows = ref<Set<string>>(new Set())

const filter = reactive<BenefitClaimFilter>({
  search: '',
  docStatus: null,
  employeeId: null,
  page: 1,
  pageSize: 10,
})

const statusOptions: OptionItem[] = [
  { id: 'Draft', label: 'ร่าง' },
  { id: 'WaitApprove', label: 'รออนุมัติ' },
  { id: 'Approved', label: 'อนุมัติแล้ว' },
  { id: 'Rejected', label: 'ตีกลับ' },
  { id: 'Disapproved', label: 'ไม่อนุมัติ' },
  { id: 'Cancelled', label: 'ยกเลิก' },
]

function hasTripInfo(c: BenefitClaim) {
  return !!(c.tripType || c.destination || c.tripPurpose || c.departAt || c.returnAt)
}

// บรรทัดย่อยใต้รายละเอียด ตามชนิดการคำนวณ (qty = จำนวนวัน/คืนที่ Backend คำนวณไว้)
function lineMeta(l: BenefitClaimLine): string {
  switch (l.calcType) {
    case 'Fuel':
      return `${l.route ? l.route + ' · ' : ''}${l.qty} กม. × ${l.fuelPrice ?? '-'} บาท/ลิตร`
    case 'Daily':
      return `${l.qty} วัน`
    case 'PerDiem':
      if (!l.startDate || !l.endDate) return ''
      return `${formatDate(l.startDate)} - ${formatDate(l.endDate)} (${l.qty} วัน)`
    case 'Accommodation':
      if (!l.startDate || !l.endDate) return ''
      return `${formatDate(l.startDate)} - ${formatDate(l.endDate)} (${l.qty} คืน)`
    default:
      return ''
  }
}

function openDocumentDetail(item: BenefitClaim) {
  if (!item.docNum) return
  detailModalRef.value?.open('BenefitClaim', item.docNum)
}

function statusLabel(s: string) {
  return statusOptions.find(x => x.id === s)?.label ?? s
}



function formatDate(d: string) {
  return new Date(d).toLocaleDateString('en-GB', { year: 'numeric', month: 'numeric', day: 'numeric' })
}

function formatDateTime(d: string) {
  return new Date(d).toLocaleString('th-TH', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const detailMap = ref<Record<string, BenefitClaim>>({})
const detailLoading = ref<Record<string, boolean>>({})
const detailError = ref<Record<string, boolean>>({})

async function loadDetail(id: string) {
  detailLoading.value[id] = true
  detailError.value[id] = false
  try {
    detailMap.value[id] = await benefitClaimStore.getById(id)
  } catch (err) {
    detailError.value[id] = true
    console.error('Failed to load detail:', err)
  } finally {
    detailLoading.value[id] = false
  }
}

function toggleExpand(id: string) {
  const next = new Set(expandedRows.value)
  if (next.has(id)) {
    next.delete(id)
  } else {
    next.add(id)
    loadDetail(id)
  }
  expandedRows.value = next
}

const totalPages = computed(() => Math.ceil(totalCount.value / filter.pageSize) || 1)
const pageWindow = computed(() => {
  const maxButtons = 5
  let start = Math.max(1, filter.page - Math.floor(maxButtons / 2))
  let end = start + maxButtons - 1
  if (end > totalPages.value) {
    end = totalPages.value
    start = Math.max(1, end - maxButtons + 1)
  }
  const pages: number[] = []
  for (let p = start; p <= end; p++) pages.push(p)
  return pages
})

function goToPage(p: number) {
  filter.page = p
  fetchList()
}

async function fetchList() {
  await benefitClaimStore.fetchList(filter)
  const validIds = new Set(items.value.map(x => x.id))
  expandedRows.value = new Set([...expandedRows.value].filter(id => validIds.has(id)))
  expandedRows.value.forEach(id => loadDetail(id))
}

let debounceTimer: ReturnType<typeof setTimeout>
function onSearchInput() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    filter.page = 1
    fetchList()
  }, 300)
}

watch(() => [filter.pageSize, filter.docStatus], () => {
  filter.page = 1
  fetchList()
})
onBeforeUnmount(() => clearTimeout(debounceTimer))

const formModalRef = ref<InstanceType<typeof BenefitClaimFormModal>>()
const isCreateMode = ref(true)
const editingId = ref<string | null>(null)

function openCreateModal() {
  isCreateMode.value = true
  editingId.value = null
  formModalRef.value?.open()
}

const editLoadingId = ref<string | null>(null)

async function openEditModal(item: BenefitClaim) {
  editLoadingId.value = item.id
  try {
    const detail = await benefitClaimStore.getById(item.id)
    isCreateMode.value = false
    editingId.value = item.id
    formModalRef.value?.open(detail)
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'โหลดข้อมูลใบเบิกไม่สำเร็จ')
  } finally {
    editLoadingId.value = null
  }
}

async function handleSave(payload: any, isCreate: boolean) {
  try {
    if (isCreate) await benefitClaimStore.create(payload)
    else await benefitClaimStore.update(editingId.value!, payload)

    formModalRef.value?.close()
    if (isCreate) filter.page = 1
    await fetchList()
    await notify.success(isCreate ? 'สร้างใบเบิกสวัสดิการสำเร็จ' : 'บันทึกการแก้ไขสำเร็จ')
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'เกิดข้อผิดพลาด', formModalRef.value?.getDialogEl())
  }
}

async function confirmDelete(item: BenefitClaim) {
  const ok = await notify.confirm('ยืนยันลบใบเบิกนี้ใช่หรือไม่', 'ยืนยันการลบ')
  if (!ok) return
  try {
    await benefitClaimStore.remove(item.id)
    await notify.success('ลบสำเร็จ')
    if (items.value.length === 1 && filter.page > 1) filter.page -= 1
    await fetchList()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ลบไม่สำเร็จ')
  }
}

async function confirmSubmit(item: BenefitClaim) {
  const ok = await notify.confirm('ยืนยันส่งอนุมัติใบเบิกนี้ใช่หรือไม่', 'ยืนยันส่งอนุมัติ')
  if (!ok) return
  try {
    await benefitClaimStore.submit(item.id)
    await notify.success('ส่งอนุมัติสำเร็จ')
    await fetchList()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ส่งอนุมัติไม่สำเร็จ')
  }
}

async function confirmRecall(item: BenefitClaim) {
  const ok = await notify.confirm('ยืนยันดึงเอกสารกลับมาเป็นร่างใช่หรือไม่ (จะแก้ไขข้อมูลต่อได้)', 'ยืนยันดึงกลับ')
  if (!ok) return
  try {
    await benefitClaimStore.recall(item.id)
    await notify.success('ดึงเอกสารกลับมาเป็นร่างสำเร็จ')
    await fetchList()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ดึงกลับไม่สำเร็จ')
  }
}

async function openPreview(item: BenefitClaim) {
  try {
    const detail = await benefitClaimStore.getById(item.id)
    previewModalRef.value?.open(detail)
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'โหลดข้อมูลไม่สำเร็จ')
  }
}

onMounted(() => {
  fetchList()

  benefitClaimStore.fetchInitData().catch((err) => {
    console.error('Failed to load init data:', err)
  })

  authStore
    .fetchCurrentEmployee(false, false)
    .then((emp) => {
      if (emp) {
        benefitClaimStore.setMyEmployee(emp.id, `${emp.fNameTh} ${emp.lNameTh}`)
      }
    })
    .catch((err) => {
      console.error('Failed to load current employee:', err)
    })
})
</script>
<style scoped src="../../assets/css/BenefitClaimList.css"></style>
