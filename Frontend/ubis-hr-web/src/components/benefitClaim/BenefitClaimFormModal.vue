<template>
  <dialog ref="dialogRef" class="modal">
    <div class="modal-box w-11/12 max-w-4xl max-h-[92vh] p-0 overflow-hidden flex flex-col rounded-2xl">
      <!-- Header -->
      <div class="px-6 py-5 border-b border-base-200 shrink-0 bg-base-100">
        <div class="flex items-start justify-between gap-4">
          <div class="flex items-start gap-3">
            <div class="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
              <ReceiptText class="size-5" />
            </div>
            <div>
              <h3 class="font-semibold text-base-content text-base leading-tight">
                {{ isEditMode ? 'แก้ไขใบเบิกสวัสดิการ' : 'สร้างใบเบิกสวัสดิการ' }}
              </h3>
              <div v-if="isEditMode" class="mt-1 flex items-center gap-2">
                <span class="text-xs text-base-content/50">เลขที่</span>
                <span class="text-xs font-semibold text-primary">{{ currentDocNum }}</span>
              </div>
              <p class="text-xs text-base-content/45 mt-1">เลือกประเภทเอกสาร แล้วเลือกสวัสดิการที่ต้องการเบิก</p>
            </div>
          </div>
          <button type="button" class="btn btn-ghost btn-sm btn-square rounded-lg" @click="close">
            <X class="size-4" />
          </button>
        </div>
      </div>

      <form @submit.prevent="handleSubmit" class="flex flex-col flex-1 min-h-0">
        <div class="overflow-y-auto px-6 py-5 space-y-5 flex-1 bg-base-100">
          <!-- Document Information -->
          <section class="rounded-xl border border-base-200 bg-base-200/20 p-4">
            <div class="flex items-center gap-2 mb-4">
              <div class="flex size-7 items-center justify-center rounded-lg bg-base-200">
                <FileText class="size-3.5 text-base-content/60" />
              </div>
              <div>
                <h4 class="text-sm font-semibold">ข้อมูลเอกสาร</h4>
                <p class="text-[11px] text-base-content/40">ข้อมูลพื้นฐานของใบเบิก</p>
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                  วันที่เอกสาร <span class="text-error">*</span>
                </label>
                <input v-model="formData.docDate" type="date"
                  class="input input-bordered input-sm h-9 w-full bg-base-100 focus:border-primary" />
                <p v-if="errors.docDate" class="text-xs text-error mt-1">{{ errors.docDate }}</p>
              </div>
              <div>
                <EmployeeSelect :model-value="selectedEmployeeId" label="ผู้ขอเบิก" :options="allowedEmployeeOptions"
                  :disabled="allowedEmployeeOptions.length <= 1" @change="onEmployeeChange" />
                <p v-if="errors.employeeId" class="text-xs text-error mt-1">{{ errors.employeeId }}</p>
              </div>

              <div class="sm:col-span-2">
                <FormSelect :model-value="selectedFormId" label="ประเภทเอกสาร *" :options="docFormOptions"
                  placeholder="-- เลือกประเภทเอกสาร --" @update:model-value="onFormChange" />
                <p v-if="errors.docFormId" class="text-xs text-error mt-1">{{ errors.docFormId }}</p>
              </div>

              <div v-if="selectedEmployee"
                class="sm:col-span-2 rounded-lg border border-base-200 bg-base-100 px-4 py-3">
                <p class="text-md font-medium">ผู้ขอเบิก : {{ selectedEmployee.fullNameTh }}</p>
                <p v-if="selectedEmployee.positionNameTh" class="text-sm text-base-content/50 mt-0.5">
                  ตำแหน่ง: {{ selectedEmployee.positionNameTh }}
                </p>

                <button type="button"
                  class="mt-2 flex items-center gap-1 text-sm font-medium text-primary hover:underline"
                  @click="showBenefits = !showBenefits">
                  <component :is="showBenefits ? ChevronUp : ChevronDown" class="size-4" />
                  สวัสดิการที่ใช้ได้ในเอกสารนี้
                  <span v-if="!isLoadingBenefits" class="text-base-content/40">({{ availableBenefits.length }})</span>
                </button>

                <div v-if="showBenefits" class="mt-2">
                  <div v-if="isLoadingBenefits" class="skeleton h-8 w-full"></div>
                  <p v-else-if="!selectedFormId" class="text-xs text-base-content/45">
                    กรุณาเลือกประเภทเอกสารก่อน
                  </p>
                  <p v-else-if="!availableBenefits.length" class="text-xs text-warning">
                    พนักงานคนนี้ไม่มีสวัสดิการที่ใช้ได้ในประเภทเอกสารนี้
                  </p>
                  <ul v-else class="space-y-1 max-h-40 overflow-y-auto">
                    <li v-for="b in availableBenefits" :key="b.benefitId"
                      class="flex items-center justify-between gap-3 text-sm">
                      <span class="truncate">{{ b.nameTh }}</span>
                      <span class="font-semibold shrink-0">{{ limitText(b.limitAmount) }}</span>
                    </li>
                  </ul>
                </div>
              </div>

              <div class="sm:col-span-2">
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">หมายเหตุ</label>
                <textarea v-model="formData.remark" rows="2" placeholder="ระบุหมายเหตุเพิ่มเติม (ถ้ามี)"
                  class="textarea textarea-bordered w-full text-sm bg-base-100 resize-none focus:border-primary"></textarea>
              </div>
            </div>
          </section>

          <!-- Trip info (เฉพาะฟอร์มเบี้ยเลี้ยง/ที่พัก) -->
          <section v-if="isPerDiemLayout" class="rounded-xl border border-base-200 bg-base-200/20 p-4">
            <div class="mb-4">
              <h4 class="text-sm font-semibold">ข้อมูลการเดินทาง</h4>
              <p class="text-[11px] text-base-content/40">รายละเอียดการปฏิบัติงานนอกสถานที่</p>
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <FormSelect v-model="formData.tripType" label="ประเภทการเดินทาง" :options="tripTypeOptions"
                  placeholder="-- เลือก --" />
              </div>
              <div>
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">สถานที่ / ประเทศปลายทาง</label>
                <input v-model="formData.destination" type="text"
                  class="input input-bordered input-sm h-9 w-full bg-base-100 focus:border-primary" />
              </div>
              <div class="sm:col-span-2">
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">วัตถุประสงค์การเดินทาง</label>
                <textarea v-model="formData.tripPurpose" rows="2"
                  class="textarea textarea-bordered w-full text-sm bg-base-100 resize-none focus:border-primary"></textarea>
              </div>
              <div>
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">วันและเวลาออกเดินทาง</label>
                <input v-model="formData.departAt" type="datetime-local"
                  class="input input-bordered input-sm h-9 w-full bg-base-100 focus:border-primary" />
              </div>
              <div>
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">วันและเวลากลับ</label>
                <input v-model="formData.returnAt" type="datetime-local"
                  class="input input-bordered input-sm h-9 w-full bg-base-100 focus:border-primary" />
              </div>
              <label class="flex items-center gap-2 text-sm cursor-pointer">
                <input v-model="formData.isOvernight" type="checkbox" class="checkbox checkbox-sm checkbox-primary" />
                จำเป็นต้องค้างแรม
              </label>
            </div>
          </section>

          <!-- Lines -->
          <section>
            <div class="flex items-center justify-between mb-3">
              <div>
                <div class="flex items-center gap-2">
                  <h4 class="text-sm font-semibold">รายการเบิก</h4>
                  <span class="badge badge-sm badge-ghost">{{ formData.lines.length }} รายการ</span>
                </div>
                <p class="text-[11px] text-base-content/40 mt-0.5">เพิ่มรายการสวัสดิการที่ต้องการเบิก</p>
              </div>
              <button type="button" class="btn btn-primary btn-sm gap-1.5 rounded-lg"
                :disabled="!selectedFormId || !availableBenefits.length" @click="addLine">
                <Plus class="size-4" /> เพิ่มรายการ
              </button>
            </div>

            <div v-if="formData.lines.length === 0"
              class="rounded-xl border border-dashed border-base-300 bg-base-200/20 py-10 text-center">
              <p v-if="errors.lines" class="text-xs text-error mb-2">{{ errors.lines }}</p>
              <div
                class="mx-auto flex size-11 items-center justify-center rounded-full bg-base-200 text-base-content/40">
                <ReceiptText class="size-5" />
              </div>
              <p class="text-sm font-medium mt-3">ยังไม่มีรายการเบิก</p>
              <p class="text-xs text-base-content/40 mt-1">
                {{ emptyLinesHint }}
              </p>
            </div>

            <div v-else class="space-y-3">
              <div v-for="(line, idx) in formData.lines" :key="idx"
                class="group rounded-xl border border-base-200 bg-base-100 p-4 transition-all hover:border-primary/20 hover:shadow-sm">
                <div class="flex items-center justify-between mb-4">
                  <div class="flex items-center gap-2.5">
                    <div
                      class="flex size-7 items-center justify-center rounded-lg bg-primary/10 text-primary text-xs font-semibold">
                      {{ idx + 1 }}
                    </div>
                    <div>
                      <p class="text-xs font-semibold">รายการที่ {{ idx + 1 }}</p>
                      <p class="text-[11px] text-base-content/40">รายละเอียดการเบิก</p>
                    </div>
                  </div>
                  <button type="button"
                    class="btn btn-ghost btn-xs btn-square rounded-lg text-base-content/35 hover:bg-error/10 hover:text-error"
                    @click="removeLine(idx)">
                    <Trash2 class="size-3.5" />
                  </button>
                </div>

                <div class="mb-3">
                  <FormSelect :model-value="line.benefitId" label="สวัสดิการ *" :options="benefitSelectOptions"
                    placeholder="-- เลือกสวัสดิการ --" @update:model-value="(id) => onBenefitChange(line, id)" />
                  <p v-if="errors[`lines[${idx}].benefitId`]" class="text-xs text-error mt-1">
                    {{ errors[`lines[${idx}].benefitId`] }}</p>
                </div>

                <div class="mb-3">
                  <label class="block text-xs font-medium text-base-content/60 mb-1.5">รายละเอียด</label>
                  <textarea v-model="line.detail" rows="2" placeholder="ระบุรายละเอียดการเบิก"
                    class="textarea textarea-bordered w-full text-sm resize-none focus:border-primary"></textarea>
                  <p v-if="errors[`lines[${idx}].detail`]" class="text-xs text-error mt-1">
                    {{ errors[`lines[${idx}].detail`] }}
                  </p>
                </div>

                <div v-if="line.benefitId" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <!-- Fuel -->
                  <template v-if="line.calcType === 'Fuel'">
                    <div>
                      <label class="block text-xs font-medium text-base-content/60 mb-1.5">ราคาน้ำมัน (บาท/ลิตร)</label>
                      <input v-model.number="line.fuelPrice" type="number" min="0" step="0.01"
                        class="input input-bordered input-sm w-full focus:border-primary" />
                      <p v-if="errors[`lines[${idx}].fuelPrice`]" class="text-xs text-error mt-1">
                        {{ errors[`lines[${idx}].fuelPrice`] }}</p>
                    </div>
                    <div>
                      <label class="block text-xs font-medium text-base-content/60 mb-1.5">ระยะทาง (กม.)</label>
                      <input v-model.number="line.qty" type="number" min="0" step="0.1"
                        class="input input-bordered input-sm w-full focus:border-primary" />
                      <p v-if="errors[`lines[${idx}].qty`]" class="text-xs text-error mt-1">
                        {{ errors[`lines[${idx}].qty`] }}</p>
                    </div>
                    <div class="sm:col-span-2">
                      <label class="block text-xs font-medium text-base-content/60 mb-1.5">เส้นทาง</label>
                      <input v-model="line.route" type="text" placeholder="ต้นทาง - ปลายทาง"
                        class="input input-bordered input-sm w-full focus:border-primary" />
                    </div>
                  </template>
                  <!-- Daily (FAC-HQ) -->
                  <template v-else-if="line.calcType === 'Daily'">
                    <div class="sm:col-span-2">
                      <label class="block text-xs font-medium text-base-content/60 mb-1.5">จำนวนวัน</label>
                      <input v-model.number="line.qty" type="number" min="1" step="1"
                        class="input input-bordered input-sm w-full focus:border-primary" />
                      <p v-if="errors[`lines[${idx}].qty`]" class="text-xs text-error mt-1">
                        {{ errors[`lines[${idx}].qty`] }}</p>
                    </div>
                  </template>
                  <!-- PerDiem / Accommodation -->
                  <template v-else-if="line.calcType === 'PerDiem' || line.calcType === 'Accommodation'">
                    <div>
                      <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                        {{ line.calcType === 'PerDiem' ? 'วันที่เริ่มต้น' : 'วันที่เข้าพัก' }}
                      </label>
                      <input v-model="line.startDate" type="date"
                        class="input input-bordered input-sm w-full focus:border-primary" />
                      <p v-if="errors[`lines[${idx}].startDate`]" class="text-xs text-error mt-1">
                        {{ errors[`lines[${idx}].startDate`] }}</p>
                    </div>
                    <div>
                      <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                        {{ line.calcType === 'PerDiem' ? 'วันที่สิ้นสุด' : 'วันที่ออก' }}
                      </label>
                      <input v-model="line.endDate" type="date"
                        class="input input-bordered input-sm w-full focus:border-primary" />
                      <p v-if="errors[`lines[${idx}].endDate`]" class="text-xs text-error mt-1">
                        {{ errors[`lines[${idx}].endDate`] }}</p>
                    </div>
                    <p v-if="unitPreview(line)" class="sm:col-span-2 text-xs text-base-content/50">
                      {{ unitPreview(line) }}
                    </p>
                  </template>

                  <!-- Actual -->
                  <div v-else class="sm:col-span-2">
                    <label class="block text-xs font-medium text-base-content/60 mb-1.5">ยอดเบิก</label>
                    <div class="relative">
                      <input v-model.number="line.amount" type="number" min="0" step="0.01"
                        class="input input-bordered input-sm w-full pr-10 focus:border-primary" />
                      <span
                        class="absolute right-3 top-1/2 -translate-y-1/2 text-[11px] text-base-content/35">บาท</span>
                    </div>
                    <p v-if="errors[`lines[${idx}].amount`]" class="text-xs text-error mt-1">
                      {{ errors[`lines[${idx}].amount`] }}</p>
                  </div>
                </div>

                <!-- ยอดที่ระบบคำนวณ -->
                <div v-if="line.benefitId && isComputed(line)"
                  class="mt-3 rounded-lg border border-base-200 bg-base-200/40 px-3 py-2.5">
                  <div class="flex items-center justify-between">
                    <span class="text-xs text-base-content/50">ยอดเบิก (ระบบคำนวณ)</span>
                    <span class="text-sm font-semibold">
                      <template v-if="line.amount > 0">{{ line.amount.toLocaleString('th-TH', {
                        minimumFractionDigits: 2
                      }) }} บาท</template>
                      <template v-else><span
                          class="text-xs font-normal text-base-content/40">กรอกข้อมูลให้ครบเพื่อคำนวณ</span></template>
                    </span>
                  </div>
                  <p v-if="line.calcError" class="mt-2 flex items-center gap-1.5 text-xs text-error">
                    <CircleAlert class="size-3.5 shrink-0" /> {{ line.calcError }}
                  </p>

                  <table v-if="line.breakdown.length" class="table table-xs mt-2">
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

                <!-- Limit Info -->
                <div v-if="line.benefitId && line.limitAmount > 0 && !isComputed(line)"
                  class="mt-3 rounded-lg px-3 py-2.5 space-y-1.5 bg-base-200/50">
                  <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
                    <div class="flex items-center gap-2">
                      <Info class="size-3.5 text-base-content/40" />
                      <span class="text-xs text-base-content/50">วงเงินสิทธิ์</span>
                      <span class="text-xs font-semibold">{{ line.limitAmount.toLocaleString('th-TH') }} บาท</span>
                    </div>

                    <div v-if="line.amount > line.limitAmount" class="flex items-center gap-1.5 text-warning">
                      <TriangleAlert class="size-3.5" />
                      <span class="text-xs font-medium">
                        เกินวงเงินอ้างอิง {{ (line.amount - line.limitAmount).toLocaleString('th-TH') }} บาท
                        (ตรวจสอบก่อนอนุมัติ)
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </section>

          <section class="rounded-xl border border-base-200 bg-base-200/20 p-4">
            <p class="text-sm font-semibold mb-3">ไฟล์แนบ</p>
            <AttachmentList doc-type="BenefitClaim" :doc-number="currentDocNum || null" :target="dialogRef" />
          </section>

          <!-- Total -->
          <div class="rounded-xl border border-primary/20 bg-primary/5 px-5 py-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-xs text-base-content/50">ยอดรวมทั้งหมด</p>
                <p class="text-[11px] text-base-content/35 mt-0.5">จำนวนเงินที่ขอเบิก</p>
              </div>
              <div class="text-right">
                <span class="text-xl font-bold text-primary">{{ totalAmount.toLocaleString('th-TH') }}</span>
                <span class="text-xs text-base-content/50 ml-1">บาท</span>
              </div>
            </div>
            <p v-if="hasPendingCalc" class="text-[11px] text-base-content/45 mt-2">
              ยอดของรายการที่ระบบคำนวณเป็นยอดประมาณการ ระบบจะคำนวณจริงอีกครั้งเมื่อกดบันทึก
            </p>
          </div>

          <!-- Error Banner -->
          <div v-if="errorMessage"
            class="flex items-center gap-2 rounded-lg bg-error/10 text-error px-4 py-2.5 text-xs font-medium">
            <CircleAlert class="size-4 shrink-0" />
            {{ errorMessage }}
          </div>
        </div>

        <!-- Footer -->
        <div class="flex items-center justify-between gap-3 px-6 py-4 border-t border-base-200 bg-base-100 shrink-0">
          <div class="hidden sm:block">
            <span class="text-xs text-base-content/40">ตรวจสอบข้อมูลก่อนกดบันทึก</span>
          </div>
          <div class="flex justify-end gap-2 ml-auto">
            <button type="button" class="btn btn-ghost btn-sm rounded-lg px-5" @click="close">ยกเลิก</button>
            <button type="submit" class="btn btn-primary btn-sm rounded-lg px-6" :disabled="isSubmitting">
              <span v-if="isSubmitting" class="loading loading-spinner loading-xs"></span>
              {{ isSubmitting ? 'กำลังบันทึก...' : 'บันทึก' }}
            </button>
          </div>
        </div>
      </form>
    </div>

    <form method="dialog" class="modal-backdrop">
      <button>close</button>
    </form>
  </dialog>
</template>

<script setup lang="ts">
import * as yup from 'yup'
import { ref, computed, watch } from 'vue'
import { notify, extractErrorMessage } from '../../utils/notify.ts'
import { tripTypeOptions, rangeText } from '../../utils/benefitClaim.ts'
import { X, Plus, Trash2, ReceiptText, FileText, Info, TriangleAlert, CircleAlert, ChevronDown, ChevronUp } from 'lucide-vue-next'
import FormSelect from '../ui/FormSelect.vue'
import type { BenefitClaim, AvailableBenefit, BreakdownRow } from '../../types/BenefitClaim.ts'
import AttachmentList from '../attachment/AttachmentList.vue'
import EmployeeSelect from '../../components/ui/EmployeeSelect.vue'
import { useBenefitClaimStore } from '../../stores/benefitClaimStore.ts'

interface EmployeeOption {
  id: string
  empId: string
  fullNameTh: string
  positionNameTh?: string | null
}
interface OptionItem {
  id: string
  label: string
}

const props = defineProps<{
  isCreate: boolean
  employeeId: string
  employeeName: string
  allowedEmployees: EmployeeOption[]
}>()

const benefitClaimStore = useBenefitClaimStore()
const emit = defineEmits<{ save: [data: any, isCreate: boolean] }>()

const dialogRef = ref<HTMLDialogElement>()
const isSubmitting = ref(false)
const currentDocNum = ref('')
const errorMessage = ref('')
const showBenefits = ref(false)
const selectedEmployeeId = ref('')
const selectedFormId = ref('')
const availableBenefits = ref<AvailableBenefit[]>([])
const isLoadingBenefits = ref(false)
const isEditMode = ref(false)
const errors = ref<Record<string, string>>({})

const calcTimers = new Map<object, ReturnType<typeof setTimeout>>()
const calcSeq = new WeakMap<object, number>()
const calcKeys = new WeakMap<object, string>()

const allowedEmployeeOptions = computed(() => {
  if (props.allowedEmployees.length === 0) {
    return [{ id: props.employeeId, empId: '', fullNameTh: props.employeeName }]
  }
  return props.allowedEmployees
})

const selectedEmployee = computed(() =>
  allowedEmployeeOptions.value.find(e => e.id === selectedEmployeeId.value) ?? null,
)

const docFormOptions = computed<OptionItem[]>(() =>
  benefitClaimStore.docForms.map(f => ({ id: String(f.id), label: `${f.code} - ${f.nameTh}` })),
)

const selectedForm = computed(() =>
  benefitClaimStore.docForms.find(f => String(f.id) === selectedFormId.value) ?? null,
)

const isPerDiemLayout = computed(() => selectedForm.value?.layout === 'PerDiem')

const benefitSelectOptions = computed<OptionItem[]>(() =>
  availableBenefits.value.map(b => ({ id: b.benefitId, label: b.nameTh })),
)

interface LineForm {
  benefitId: string | null
  detail: string
  calcType: string | null
  region: string | null
  limitAmount: number
  amount: number
  qty: number
  accountCode: string
  startDate: string
  endDate: string
  fuelPrice: number | null
  route: string
  breakdown: BreakdownRow[]
  calcError: string
}

const defaultForm = () => ({
  docDate: new Date().toISOString().split('T')[0],
  remark: '',
  tripType: null as string | null,
  destination: '',
  tripPurpose: '',
  departAt: '',
  returnAt: '',
  isOvernight: false,
  lines: [] as LineForm[],
})

const formData = ref(defaultForm())

const totalAmount = computed(() => formData.value.lines.reduce((sum, l) => sum + (l.amount || 0), 0))
const hasPendingCalc = computed(() => formData.value.lines.some(l => l.benefitId && isComputed(l)))
const emptyLinesHint = computed(() =>
  selectedFormId.value
    ? 'กดปุ่ม เพิ่มรายการ เพื่อเริ่มกรอกข้อมูล'
    : 'เลือกประเภทเอกสารก่อน แล้วกดปุ่ม เพิ่มรายการ',
)
const baseSchema = yup.object({
  docDate: yup.string().required('กรุณาระบุวันที่เอกสาร'),
  employeeId: yup.string().required('กรุณาเลือกผู้ขอเบิก'),
  docFormId: yup.string().required('กรุณาเลือกประเภทเอกสาร'),
  lines: yup
    .array()
    .min(1, 'กรุณาเพิ่มรายการเบิกอย่างน้อย 1 รายการ')
    .of(
      yup.object({
        benefitId: yup.string().nullable().required('กรุณาเลือกสวัสดิการ'),
        detail: yup.string().trim().required('กรุณาระบุรายละเอียด'),
      }),
    ),
})

function limitText(n: number) {
  return n > 0 ? `${n.toLocaleString('th-TH')} บาท` : 'ไม่จำกัด'
}

const COMPUTED_CALC_TYPES = new Set(['Fuel', 'PerDiem', 'Accommodation', 'Daily'])

function isComputed(line: LineForm) {
  return COMPUTED_CALC_TYPES.has(line.calcType ?? '')
}

function daysDiff(start: string, end: string) {
  const ms = new Date(end).getTime() - new Date(start).getTime()
  return Math.round(ms / 86400000)
}

function unitPreview(line: LineForm) {
  if (!line.startDate || !line.endDate) return ''
  const diff = daysDiff(line.startDate, line.endDate)
  if (line.calcType === 'PerDiem') return diff >= 0 ? `จำนวน ${diff + 1} วัน` : ''
  return diff >= 1 ? `จำนวน ${diff} คืน` : ''
}

function blankLine(): LineForm {
  return {
    benefitId: null,
    detail: '',
    calcType: null,
    region: null,
    limitAmount: 0,
    amount: 0,
    qty: 1,
    accountCode: '',
    startDate: '',
    endDate: '',
    fuelPrice: null,
    route: '',
    breakdown: [],
    calcError: '',
  }
}

function addLine() {
  formData.value.lines.push(blankLine())
}

function removeLine(idx: number) {
  const l = formData.value.lines[idx]
  if (l) clearTimeout(calcTimers.get(l))
  formData.value.lines.splice(idx, 1)
}

function applyBenefit(line: LineForm, b: AvailableBenefit | undefined) {
  line.calcType = b?.calcType ?? null
  line.region = b?.region ?? null
  line.limitAmount = b?.limitAmount ?? 0
}

function onBenefitChange(line: LineForm, benefitId: string | null | undefined) {
  // รีเซ็ตค่าที่ขึ้นกับสวัสดิการ แต่คงรายละเอียด/รหัสบัญชีที่ผู้ใช้กรอกไว้
  const { detail, accountCode } = line
  Object.assign(line, blankLine(), { benefitId: benefitId || null, detail, accountCode })
  applyBenefit(line, availableBenefits.value.find(x => x.benefitId === line.benefitId))
}

async function loadAvailableBenefits() {
  availableBenefits.value = []
  if (!selectedEmployeeId.value || !selectedFormId.value) return

  isLoadingBenefits.value = true
  try {
    availableBenefits.value = await benefitClaimStore.getAvailableBenefits(
      selectedEmployeeId.value,
      Number(selectedFormId.value),
    )
    // เคลียร์รายการที่ไม่อยู่ในสิทธิ์แล้ว
    for (const line of formData.value.lines) {
      if (!line.benefitId) continue
      const b = availableBenefits.value.find(x => x.benefitId === line.benefitId)
      if (!b) line.benefitId = null
      applyBenefit(line, b)
    }
  } catch (err) {
    console.error('Failed to load available benefits:', err)
    availableBenefits.value = []
  } finally {
    isLoadingBenefits.value = false
  }
}

async function confirmClearLines(message: string, title: string) {
  if (formData.value.lines.length === 0) return true
  return await notify.confirm(message, title, dialogRef.value)
}

async function onEmployeeChange(employeeId: string | null | undefined) {
  const newId = employeeId ?? props.employeeId
  if (newId === selectedEmployeeId.value) return

  const ok = await confirmClearLines(
    'เปลี่ยนผู้ขอเบิกจะล้างรายการเบิกที่กรอกไว้ทั้งหมด เนื่องจากสวัสดิการแต่ละคนไม่เท่ากัน ยืนยันเปลี่ยนหรือไม่',
    'ยืนยันเปลี่ยนผู้ขอเบิก',
  )
  if (!ok) return

  selectedEmployeeId.value = newId
  formData.value.lines = []
  await loadAvailableBenefits()
}

async function onFormChange(formId: string | null | undefined) {
  const newId = formId ?? ''
  if (newId === selectedFormId.value) return

  const ok = await confirmClearLines(
    'เปลี่ยนประเภทเอกสารจะล้างรายการเบิกที่กรอกไว้ทั้งหมด ยืนยันเปลี่ยนหรือไม่',
    'ยืนยันเปลี่ยนประเภทเอกสาร',
  )
  if (!ok) return

  selectedFormId.value = newId
  formData.value.lines = []
  await loadAvailableBenefits()
}

async function open(item?: BenefitClaim | null) {
  errorMessage.value = ''
  errors.value = {}
  showBenefits.value = false
  currentDocNum.value = item?.docNum || ''
  isEditMode.value = !!item
  selectedEmployeeId.value = item?.employeeId || props.employeeId
  selectedFormId.value = item?.docFormId ? String(item.docFormId) : ''

  if (benefitClaimStore.docForms.length === 0) {
    try {
      await benefitClaimStore.fetchInitData()
    } catch (err) {
      console.error('Failed to load forms:', err)
    }
  }

  if (isEditMode.value && item) {
    formData.value = {
      docDate: item.docDate.split('T')[0],
      remark: item.remark ?? '',
      tripType: item.tripType ?? null,
      destination: item.destination ?? '',
      tripPurpose: item.tripPurpose ?? '',
      departAt: item.departAt ? item.departAt.slice(0, 16) : '',
      returnAt: item.returnAt ? item.returnAt.slice(0, 16) : '',
      isOvernight: !!item.isOvernight,
      lines: item.lines.map(l => ({
        benefitId: l.benefitId,
        detail: l.detail,
        calcType: l.calcType ?? null,
        region: l.region ?? null,
        limitAmount: l.limitAmount,
        amount: l.amount,
        qty: l.qty,
        accountCode: l.accountCode ?? '',
        startDate: l.startDate ? l.startDate.split('T')[0] : '',
        endDate: l.endDate ? l.endDate.split('T')[0] : '',
        fuelPrice: l.fuelPrice ?? null,
        route: l.route ?? '',
        breakdown: l.breakdown ?? [],
        calcError: '',
      })),
    }
    // ยอดที่บันทึกไว้มาจาก Backend แล้ว ไม่ต้องคำนวณซ้ำตอนเปิดฟอร์ม
    formData.value.lines.forEach(l => calcKeys.set(l, calcKey(l)))
  } else {
    formData.value = defaultForm()
  }

  await loadAvailableBenefits()
  dialogRef.value?.showModal()
}

function close() {
  dialogRef.value?.close()
}

function getDialogEl() {
  return dialogRef.value ?? null
}

function validateLinesByType(): boolean {
  let ok = true
  formData.value.lines.forEach((l, i) => {
    const fail = (field: string, msg: string) => {
      if (!errors.value[`lines[${i}].${field}`]) errors.value[`lines[${i}].${field}`] = msg
      ok = false
    }
    if (l.calcError) { ok = false }
    if (l.calcType === 'Fuel') {
      if (!((l.fuelPrice ?? 0) > 0)) fail('fuelPrice', 'กรุณาระบุราคาน้ำมัน')
      if (!(l.qty > 0)) fail('qty', 'กรุณาระบุระยะทาง')
    }
    else if (l.calcType === 'PerDiem' || l.calcType === 'Accommodation') {
      if (!l.startDate) fail('startDate', 'กรุณาระบุวันที่เริ่มต้น')
      if (!l.endDate) fail('endDate', 'กรุณาระบุวันที่สิ้นสุด')
      if (l.startDate && l.endDate) {
        const diff = daysDiff(l.startDate, l.endDate)
        if (l.calcType === 'PerDiem' && diff < 0) fail('endDate', 'วันที่สิ้นสุดต้องไม่ก่อนวันที่เริ่มต้น')
        if (l.calcType === 'Accommodation' && diff < 1) fail('endDate', 'วันที่ออกต้องหลังวันที่เข้าพักอย่างน้อย 1 วัน')
      }
    } else if (l.calcType === 'Daily') {
      if (!(l.qty > 0) || !Number.isInteger(l.qty)) fail('qty', 'กรุณาระบุจำนวนวัน (จำนวนเต็ม)')
    } else if (!(l.amount > 0)) {
      fail('amount', 'ยอดเบิกต้องมากกว่า 0')
    }
  })
  return ok
}

// ---------- Live preview (คำนวณยอดสดจาก Backend) ----------
function lineReady(l: LineForm) {
  if (!l.benefitId) return false
  if (l.calcType === 'Fuel') return l.qty > 0 && (l.fuelPrice ?? 0) > 0
  if (l.calcType === 'Daily') return l.qty > 0 && Number.isInteger(l.qty)
  return !!(l.startDate && l.endDate) // PerDiem / Accommodation
}

function scheduleCalc(l: LineForm) {
  clearTimeout(calcTimers.get(l))
  l.calcError = ''
  if (!isComputed(l) || !lineReady(l)) {
    l.amount = 0
    l.breakdown = []
    return
  }
  calcTimers.set(l, setTimeout(async () => {
    const seq = (calcSeq.get(l) ?? 0) + 1
    calcSeq.set(l, seq)
    try {
      const r = await benefitClaimStore.calculateLine({
        employeeId: selectedEmployeeId.value,
        docFormId: Number(selectedFormId.value),
        docDate: formData.value.docDate,
        line: {
          benefitId: l.benefitId!,
          qty: l.qty,
          startDate: l.startDate || null,
          endDate: l.endDate || null,
          fuelPrice: l.fuelPrice || null,
        },
      })
      if (calcSeq.get(l) !== seq) return
      l.amount = r.amount
      l.breakdown = r.breakdown ?? []
      l.calcError = ''
    } catch (err: any) {
      if (calcSeq.get(l) !== seq) return
      l.amount = 0          // ล้างยอดเก่า
      l.breakdown = []      // ล้างตารางเก่า
      l.calcError = extractErrorMessage(err) || 'คำนวณยอดไม่สำเร็จ กรุณาตรวจสอบข้อมูล'
    }
  }, 400))
}

function calcKey(l: LineForm) {
  return [l.benefitId, l.calcType, l.qty, l.startDate, l.endDate, l.fuelPrice, formData.value.docDate].join('|')
}

// คำนวณใหม่เฉพาะรายการที่ค่าเปลี่ยน (เปลี่ยน docDate = ทุกรายการ)
watch(
  () => formData.value.lines.map(calcKey).join(';'),
  () => {
    for (const l of formData.value.lines) {
      const key = calcKey(l)
      if (calcKeys.get(l) === key) continue
      calcKeys.set(l, key)
      scheduleCalc(l)
    }
  },
)

async function handleSubmit() {
  errorMessage.value = ''
  errors.value = {}
  let valid = true
  try {
    await baseSchema.validate(
      {
        docDate: formData.value.docDate,
        employeeId: selectedEmployeeId.value,
        docFormId: selectedFormId.value,
        lines: formData.value.lines,
      },
      { abortEarly: false },
    )
  } catch (err: any) {
    valid = false
    if (err.inner) {
      err.inner.forEach((e: any) => {
        if (e.path && !errors.value[e.path]) errors.value[e.path] = e.message
      })
    }
  }
  if (!validateLinesByType()) valid = false
  if (!valid) return

  isSubmitting.value = true
  try {
    const f = formData.value
    const trip = isPerDiemLayout.value
      ? {
          tripType: f.tripType,
          destination: f.destination || null,
          tripPurpose: f.tripPurpose || null,
          departAt: f.departAt || null,
          returnAt: f.returnAt || null,
          isOvernight: f.isOvernight,
        }
      : { tripType: null, destination: null, tripPurpose: null, departAt: null, returnAt: null, isOvernight: null }
    const payload = {
      docDate: f.docDate,
      docFormId: Number(selectedFormId.value),
      employeeId: selectedEmployeeId.value,
      remark: f.remark || null,
      ...trip,
      lines: formData.value.lines.map(l => ({
        benefitId: l.benefitId,
        detail: l.detail,
        amount: isComputed(l) ? 0 : l.amount,
        qty: l.qty,
        accountCode: l.accountCode || null,
        startDate: l.startDate || null,
        endDate: l.endDate || null,
        route: l.route || null,
        fuelPrice: l.calcType === 'Fuel' ? l.fuelPrice : null,
      })),
    }
    emit('save', payload, !isEditMode.value)
  } finally {
    isSubmitting.value = false
  }
}

defineExpose({ open, close, getDialogEl })
</script>