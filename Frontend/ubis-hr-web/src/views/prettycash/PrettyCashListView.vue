<template>
  <div class="p-4 sm:p-6 space-y-5 mx-auto max-w-[1600px]">
    <!-- Page Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
      <div>
        <div class="flex items-center gap-2">
          <div class="flex size-9 items-center justify-center rounded-xl bg-primary/10 text-primary">
            <Receipt class="size-4.5" />
          </div>
          <h1 class="text-xl font-semibold text-base-content">เบิกสวัสดิการ / เงินสดย่อย</h1>
        </div>
        <p class="text-xs text-base-content/45 mt-1 ml-11">จัดการและติดตามรายการเบิกสวัสดิการและเงินสดย่อย</p>
      </div>

      <button class="btn btn-primary btn-sm gap-1.5 rounded-lg" @click="openCreateModal">
        <Plus class="size-4" /> สร้างใบเบิก
      </button>
    </div>

    <!-- Filters -->
    <div class="bg-base-100 rounded-xl border border-base-200 p-3 sm:p-4 shadow-sm">
      <div class="flex flex-col sm:flex-row gap-3">
        <div class="relative flex-1 min-w-[220px]">
          <Search class="absolute left-3.5 top-1/2 -translate-y-1/2 size-4 text-gray-400 pointer-events-none z-10" />
          <input
            v-model="filter.search"
            @input="onSearchInput"
            type="text"
            placeholder="ค้นหาเลขที่เอกสาร ชื่อพนักงาน รายะเอียดเอกสาร..."
            class="input input-bordered w-full pl-10 text-sm focus:outline-none focus:border-primary transition-colors"
          />
        </div>

        <div class="w-full sm:w-56">
          <FormSelect v-model="filter.docStatus" :options="statusOptions" placeholder="สถานะ: ทั้งหมด" />
        </div>
      </div>
    </div>

    <!-- Table Card -->
    <div class="bg-base-100 rounded-xl border border-base-200 shadow-sm overflow-hidden">
      <div v-if="isLoading" class="p-5 space-y-2">
        <div v-for="i in 5" :key="i" class="skeleton h-14 w-full rounded-lg"></div>
      </div>

      <template v-else>
        <div v-if="items.length === 0" class="flex flex-col items-center gap-2 py-16 text-center">
          <div class="flex size-12 items-center justify-center rounded-full bg-base-200 text-base-content/30">
            <Receipt class="size-5" />
          </div>
          <p class="text-sm font-medium text-base-content/60">ไม่พบใบเบิก</p>
          <p class="text-xs text-base-content/35">ลองเปลี่ยนเงื่อนไขการค้นหา หรือสร้างใบเบิกใหม่</p>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="table w-full">
            <thead>
              <tr class="text-[11px] uppercase tracking-wide text-base-content/40 border-b border-base-200">
                <th class="bg-base-100 w-10"></th>
                <th class="bg-base-100">เลขที่เอกสาร</th>
                <th class="bg-base-100">ประเภทการเบิก</th>
                <th class="bg-base-100">วันที่</th>
                <th class="bg-base-100">ผู้ขอเบิก</th>
                <th class="bg-base-100 text-right">ยอดรวม</th>
                <th class="bg-base-100">สถานะ</th>
                <th class="bg-base-100">แก้ไขล่าสุด</th>
                <th class="bg-base-100 text-right">จัดการ</th>
              </tr>
            </thead>
            <tbody>
              <template v-for="item in items" :key="item.id">
                <tr
                  class="group border-b border-base-200/60 transition-colors"
                  :class="expandedRows.has(item.id) ? 'bg-primary/[0.025]' : 'hover:bg-base-200/30'"
                >
                  <td class="w-10">
                    <button
                      type="button"
                      class="btn btn-ghost btn-xs btn-square rounded-lg text-base-content/40 hover:text-primary hover:bg-primary/10"
                      :title="expandedRows.has(item.id) ? 'ซ่อนรายการ' : 'ดูรายการ'"
                      @click="toggleExpand(item.id)"
                    >
                      <ChevronUp v-if="expandedRows.has(item.id)" class="size-4" />
                      <ChevronDown v-else class="size-4" />
                    </button>
                  </td>

                  <td>
                    <p class="font-semibold text-sm">{{ item.docNum || '(ยังไม่ส่งอนุมัติ)' }}</p>
                    <p class="text-[11px] text-base-content/35 mt-0.5">{{ item.lines?.length ?? 0 }} รายการ</p>
                  </td>

                  <td>
                    <span class="badge badge-sm font-normal gap-1" :class="docTypeBadgeClass(item)">
                      <component :is="docTypeIcon(item)" class="size-3" />
                      {{ docTypeLabel(item) }}
                    </span>
                  </td>

                  <td>
                    <span class="text-sm text-base-content/65">{{ formatDate(item.docDate) }}</span>
                  </td>

                  <td>
                    <span class="text-sm text-base-content/65">{{ item.employeeNameTh }}</span>
                  </td>

                  <td class="text-right">
                    <span class="font-semibold text-sm">{{ item.totalAmount.toLocaleString('th-TH') }}</span>
                    <span class="text-[11px] text-base-content/35 ml-1">บาท</span>
                  </td>

                  <td>
                    <span class="badge badge-sm font-medium" :class="statusClass(item.docStatus)">
                      {{ statusLabel(item.docStatus) }}
                    </span>
                  </td>

                  <td>
                    <p class="text-xs text-base-content/60">{{ item.updatedBy }}</p>
                    <p class="text-[11px] text-base-content/35">{{ formatDateTime(item.updatedAt) }}</p>
                  </td>

                  <td>
                    <div class="flex items-center justify-end gap-0.5">
                      <button
                        v-if="item.docStatus === 'Draft'"
                        class="btn btn-ghost btn-xs btn-square rounded-lg text-warning/70 hover:bg-warning/10 hover:text-warning"
                        title="แก้ไข"
                        @click="openEditModal(item)"
                      >
                        <Edit class="size-4" />
                      </button>
                      <button
                        v-if="item.docStatus === 'Draft'"
                        class="btn btn-ghost btn-xs btn-square rounded-lg text-primary hover:bg-primary/10"
                        title="ส่งอนุมัติ"
                        @click="confirmSubmit(item)"
                      >
                        <Send class="size-4" />
                      </button>
                      <button
                        class="btn btn-ghost btn-xs btn-square rounded-lg text-base-content/40 hover:bg-info/10 hover:text-info"
                        title="ดูรายละเอียดและสถานะอนุมัติ"
                        @click="openDocumentDetail(item)"
                      >
                        <Eye class="size-3.5" />
                      </button>
                      <button
                        v-if="item.docStatus === 'WaitApprove' || item.docStatus === 'Rejected'"
                        class="btn btn-ghost btn-xs btn-square rounded-lg text-info hover:bg-info/10"
                        title="ดึงกลับมาเป็นร่าง"
                        @click="confirmRecall(item)"
                      >
                        <Undo2 class="size-4" />
                      </button>

                      <button
                        v-if="item.docStatus === 'Draft'"
                        class="btn btn-ghost btn-xs btn-square rounded-lg text-error/60 hover:bg-error/10 hover:text-error"
                        title="ลบ"
                        @click="confirmDelete(item)"
                      >
                        <Trash2 class="size-4" />
                      </button>
                    </div>
                  </td>
                </tr>

                <tr v-if="expandedRows.has(item.id)" class="border-b border-base-200/60 bg-base-200/20">
                  <td colspan="9" class="p-0">
                    <div class="px-5 sm:px-12 py-4 space-y-4">
                      <div>
                        <div class="flex items-center justify-between mb-3">
                          <div>
                            <p class="text-xs font-semibold">รายละเอียดการเบิก</p>
                            <p class="text-[11px] text-base-content/40 mt-0.5">รายการสวัสดิการและเงินสดย่อยในใบเบิกนี้</p>
                          </div>
                          <span class="text-[11px] text-base-content/40">{{ item.lines?.length ?? 0 }} รายการ</span>
                        </div>

                        <div v-if="item.lines?.length" class="rounded-xl border border-base-200 bg-base-100 overflow-hidden">
                          <div
                            v-for="(line, lineIndex) in item.lines"
                            :key="line.id ?? lineIndex"
                            class="flex flex-col sm:grid sm:grid-cols-[40px_minmax(180px,1fr)_minmax(180px,1.5fr)_130px] gap-3 items-start sm:items-center px-4 py-3 border-b border-base-200/70 last:border-0"
                          >
                            <div class="flex size-7 items-center justify-center rounded-lg bg-primary/10 text-primary text-xs font-semibold">
                              {{ lineIndex + 1 }}
                            </div>

                            <div class="min-w-0">
                              <span
                                class="badge badge-xs font-normal mb-0.5"
                                :class="line.benefitId ? 'badge-primary text-primary-content' : 'badge-ghost'"
                              >
                                {{ line.benefitId ? 'สวัสดิการ' : 'เงินสดย่อย' }}
                              </span>
                              <p class="text-sm font-medium truncate">{{ getBenefitName(line.benefitId) }}</p>
                            </div>

                            <div class="min-w-0">
                              <p class="text-xs text-base-content/40 mb-0.5">รายละเอียด</p>
                              <p class="text-sm text-base-content/65 truncate" :title="line.detail">{{ line.detail || '-' }}</p>
                            </div>

                            <div class="sm:text-right">
                              <p class="text-xs text-base-content/40 mb-0.5">ยอดเบิก</p>
                              <p class="text-sm font-semibold">
                                {{ line.amount?.toLocaleString('th-TH') ?? 0 }}
                                <span class="text-[11px] font-normal text-base-content/40">บาท</span>
                              </p>
                            </div>
                          </div>
                        </div>

                        <div v-else class="rounded-xl border border-dashed border-base-300 p-6 text-center text-xs text-base-content/40">
                          ไม่มีรายละเอียดรายการ
                        </div>
                      </div>

                      <div v-if="item.remark" class="flex gap-2 rounded-lg bg-base-100 border border-base-200 px-3 py-2.5">
                        <MessageSquare class="size-3.5 shrink-0 mt-0.5 text-base-content/35" />
                        <div>
                          <p class="text-[11px] text-base-content/40">หมายเหตุ</p>
                          <p class="text-xs text-base-content/65 mt-0.5">{{ item.remark }}</p>
                        </div>
                      </div>

                      <!-- ไฟล์แนบ -->
                      <div class="rounded-xl border border-base-200 bg-base-100 p-3">
                        <p class="text-xs font-semibold mb-2">ไฟล์แนบ</p>
                        <AttachmentList
                          doc-type="PrettyCash"
                          :doc-number="item.docNum || null"
                          :readonly="item.docStatus !== 'Draft'"
                        />
                      </div>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>

        <div class="flex items-center justify-between px-4 sm:px-5 py-3 border-t border-base-200 bg-base-50/50">
          <span class="text-xs text-base-content/40">{{ totalCount }} รายการทั้งหมด</span>
          <span class="text-[11px] text-base-content/30">กดลูกศรเพื่อดูรายละเอียด</span>
        </div>
      </template>
    </div>
  </div>

  <PrettyCashFormModal
    ref="formModalRef"
    :is-create="isCreateMode"
    :employee-id="myEmployeeId"
    :employee-name="myEmployeeName"
    :benefit-options="benefitOptions"
    :allowed-employees="allowedEmployees"
    @save="handleSave"
  />
  <ApprovalDetailModal ref="detailModalRef" />
</template>

<script setup lang="ts">
import { ref, reactive, watch, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { usePrettyCashStore } from '../../stores/prettyCashStore.ts'
import FormSelect from '../../components/ui/FormSelect.vue'
import PrettyCashFormModal from '../../components/prettycash/PrettyCashFormModal.vue'
import ApprovalDetailModal from '../../components/approval/ApprovalDetailModal.vue'
import AttachmentList from '../../components/attachment/AttachmentList.vue'
import { notify, extractErrorMessage } from '../../utils/notify'
import { useAuthStore } from '../../stores/authStore.ts'
import type { PrettyCash, PrettyCashFilter } from '../../types/PrettyCash'
import { Plus, Search, Receipt, Edit, Trash2, Send, ChevronDown, ChevronUp, MessageSquare, Gift, Wallet, Shuffle, Undo2, CircleAlertIcon, Eye } from 'lucide-vue-next'

interface OptionItem {
  id: string
  label: string
}

const authStore = useAuthStore()
const prettyCashStore = usePrettyCashStore()
const { items, totalCount, isLoading, benefitOptions, allowedEmployees, myEmployeeId, myEmployeeName } = storeToRefs(prettyCashStore)
const detailModalRef = ref<InstanceType<typeof ApprovalDetailModal>>()
const expandedRows = ref<Set<string>>(new Set())

const filter = reactive<PrettyCashFilter>({
  search: '',
  docStatus: null,
  employeeId: null,
  page: 1,
  pageSize: 20,
})

const statusOptions: OptionItem[] = [
  { id: 'Draft', label: 'ร่าง' },
  { id: 'WaitApprove', label: 'รออนุมัติ' },
  { id: 'Approved', label: 'อนุมัติแล้ว' },
  { id: 'Rejected', label: 'ตีกลับ' },
  { id: 'Disapproved', label: 'ไม่อนุมัติ' },
  { id: 'Cancelled', label: 'ยกเลิก' },
]

function openDocumentDetail(item: PrettyCash) {
  if (!item.docNum) return
  detailModalRef.value?.open('PrettyCash', item.docNum)
}

function statusLabel(s: string) {
  return statusOptions.find(x => x.id === s)?.label ?? s
}

function statusClass(s: string) {
  if (s === 'Approved') return 'badge-success text-success-content'
  if (s === 'Rejected' || s === 'Disapproved') return 'badge-error text-error-content'
  if (s === 'WaitApprove') return 'badge-warning text-warning-content'
  if (s === 'Cancelled') return 'badge-ghost text-base-content/40'
  return 'badge-ghost'
}

function getDocType(item: PrettyCash): 'benefit' | 'cash' | 'mixed' | 'empty' {
  if (!item.lines || item.lines.length === 0) return 'empty'
  const hasBenefit = item.lines.some(l => l.benefitId)
  const hasCash = item.lines.some(l => !l.benefitId)
  if (hasBenefit && hasCash) return 'mixed'
  return hasBenefit ? 'benefit' : 'cash'
}

function docTypeLabel(item: PrettyCash) {
  const type = getDocType(item)
  if (type === 'empty') return 'ยังไม่มีรายการ'
  if (type === 'benefit') return 'สวัสดิการ'
  if (type === 'cash') return 'เงินสดย่อย'
  return 'สวัสดิการ + เงินสดย่อย'
}

function docTypeBadgeClass(item: PrettyCash) {
  const type = getDocType(item)
  if (type === 'empty') return 'badge-ghost text-base-content/35'
  if (type === 'benefit') return 'badge-primary text-primary-content'
  if (type === 'cash') return 'badge-ghost'
  return 'badge-secondary text-secondary-content'
}

function docTypeIcon(item: PrettyCash) {
  const type = getDocType(item)
  if (type === 'empty') return CircleAlertIcon
  if (type === 'benefit') return Gift
  if (type === 'cash') return Wallet
  return Shuffle
}

function getBenefitName(benefitId: string | null | undefined) {
  if (!benefitId) return 'เงินสดย่อยทั่วไป'
  return benefitOptions.value.find(x => x.id === benefitId)?.label ?? 'ไม่ระบุสวัสดิการ'
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('th-TH', { year: 'numeric', month: 'short', day: 'numeric' })
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

function toggleExpand(id: string) {
  const next = new Set(expandedRows.value)
  next.has(id) ? next.delete(id) : next.add(id)
  expandedRows.value = next
}

async function fetchList() {
  await prettyCashStore.fetchList(filter)
  const validIds = new Set(items.value.map(x => x.id))
  expandedRows.value = new Set([...expandedRows.value].filter(id => validIds.has(id)))
}

let debounceTimer: ReturnType<typeof setTimeout>
function onSearchInput() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    filter.page = 1
    fetchList()
  }, 300)
}

watch(() => filter.docStatus, () => {
  filter.page = 1
  fetchList()
})

const formModalRef = ref<InstanceType<typeof PrettyCashFormModal>>()
const isCreateMode = ref(true)
const editingId = ref<string | null>(null)

function openCreateModal() {
  isCreateMode.value = true
  editingId.value = null
  formModalRef.value?.open()
}

function openEditModal(item: PrettyCash) {
  isCreateMode.value = false
  editingId.value = item.id
  formModalRef.value?.open(item)
}

async function handleSave(payload: any, isCreate: boolean) {
  try {
    if (isCreate) await prettyCashStore.create(payload)
    else await prettyCashStore.update(editingId.value!, payload)

    formModalRef.value?.close()
    await fetchList()
    await notify.success(isCreate ? 'สร้างใบเบิกสำเร็จ' : 'บันทึกการแก้ไขสำเร็จ')
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'เกิดข้อผิดพลาด', formModalRef.value?.getDialogEl())
  }
}

async function confirmDelete(item: PrettyCash) {
  const ok = await notify.confirm('ยืนยันลบใบเบิกนี้ใช่หรือไม่', 'ยืนยันการลบ')
  if (!ok) return
  try {
    await prettyCashStore.remove(item.id)
    await notify.success('ลบสำเร็จ')
    await fetchList()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ลบไม่สำเร็จ')
  }
}

async function confirmSubmit(item: PrettyCash) {
  const ok = await notify.confirm('ยืนยันส่งอนุมัติใบเบิกนี้ใช่หรือไม่ (จะออกเลขที่เอกสารทันที)', 'ยืนยันส่งอนุมัติ')
  if (!ok) return
  try {
    await prettyCashStore.submit(item.id)
    await notify.success('ส่งอนุมัติสำเร็จ')
    await fetchList()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ส่งอนุมัติไม่สำเร็จ')
  }
}

async function confirmRecall(item: PrettyCash) {
  const ok = await notify.confirm('ยืนยันดึงเอกสารกลับมาเป็นร่างใช่หรือไม่ (จะแก้ไขข้อมูลต่อได้)', 'ยืนยันดึงกลับ')
  if (!ok) return
  try {
    await prettyCashStore.recall(item.id)
    await notify.success('ดึงเอกสารกลับมาเป็นร่างสำเร็จ')
    await fetchList()
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ดึงกลับไม่สำเร็จ')
  }
}

onMounted(async () => {
  try {
    await prettyCashStore.fetchInitData()

    const empRes = await authStore.fetchCurrentEmployee()
    if (empRes) {
      prettyCashStore.setMyEmployee(empRes.id, `${empRes.fNameTh} ${empRes.lNameTh}`)
    }
  } catch (err) {
    console.error(err)
  }

  await fetchList()
})
</script>