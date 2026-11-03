<template>
  <dialog ref="dialogRef" class="modal benefit-form-modal" aria-labelledby="benefit-form-title">
    <div class="modal-box flex flex-col">
      <!-- Header -->
      <header class="benefit-form-header">
        <div class="benefit-form-heading">
          <div class="benefit-form-icon">
            <ReceiptText :size="22" :stroke-width="1.7" />
          </div>

          <div>
            <h3 id="benefit-form-title">
              {{
                isEditMode
                  ? 'แก้ไขใบเบิกสวัสดิการ'
                  : 'สร้างใบเบิกสวัสดิการ'
              }}
            </h3>

            <div v-if="isEditMode" class="benefit-document-number">
              <span>เลขที่เอกสาร</span>
              <strong>{{ currentDocNum }}</strong>
            </div>

            <p v-else>
              กรอกข้อมูลและเพิ่มรายการสวัสดิการที่ต้องการเบิก
            </p>
          </div>
        </div>

        <button type="button" class="benefit-close-button" aria-label="ปิดหน้าต่าง" @click="close">
          <X :size="18" />
        </button>
      </header>

      <form class="flex flex-col flex-1 min-h-0" @submit.prevent="handleSubmit">
        <div class="benefit-form-body">
          <!-- ข้อมูลเอกสาร -->
          <section>
            <div class="flex items-center gap-2 mb-4">
              <div class="flex size-7 items-center justify-center rounded-lg bg-primary/10">
                <FileText class="size-3.5 text-primary" />
              </div>

              <div>
                <h4 class="text-sm font-semibold">ข้อมูลเอกสาร</h4>
                <p class="text-[11px] text-base-content/40">
                  ข้อมูลพื้นฐานของใบเบิก
                </p>
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label for="benefit-doc-date" class="block text-xs font-medium mb-1.5">
                  วันที่เอกสาร <span class="text-error">*</span>
                </label>

                <input id="benefit-doc-date" v-model="formData.docDate" type="date"
                  class="input input-bordered input-sm w-full" />

                <p v-if="errors.docDate" class="text-xs text-error mt-1">
                  {{ errors.docDate }}
                </p>
              </div>

              <div>
                <EmployeeSelect :model-value="selectedEmployeeId" label="ผู้ขอเบิก" :options="allowedEmployeeOptions"
                  :disabled="allowedEmployeeOptions.length <= 1" @change="onEmployeeChange" />

                <p v-if="errors.employeeId" class="text-xs text-error mt-1">
                  {{ errors.employeeId }}
                </p>
              </div>

              <div class="sm:col-span-2">
                <FormSelect :model-value="selectedFormId" label="ประเภทเอกสาร *" :options="docFormOptions"
                  placeholder="เลือกประเภทเอกสาร" @update:model-value="onFormChange" />

                <p v-if="errors.docFormId" class="text-xs text-error mt-1">
                  {{ errors.docFormId }}
                </p>
              </div>

              <!-- ข้อมูลผู้ขอเบิก -->
              <div v-if="selectedEmployee" class="benefit-employee-card sm:col-span-2">
                <p>
                  ผู้ขอเบิก : {{ selectedEmployee.fullNameTh }}
                </p>

                <p v-if="selectedEmployee.positionNameTh" class="text-xs text-base-content/50 mt-1">
                  ตำแหน่ง : {{ selectedEmployee.positionNameTh }}
                </p>

                <button type="button" class="mt-2 flex items-center gap-1 font-medium hover:underline"
                  :aria-expanded="showBenefits" @click="showBenefits = !showBenefits">
                  <component :is="showBenefits ? ChevronUp : ChevronDown" class="size-4" />

                  สวัสดิการที่ใช้ได้ในเอกสารนี้

                  <span v-if="!isLoadingBenefits" class="text-base-content/40">
                    ({{ availableBenefits.length }})
                  </span>
                </button>

                <div v-if="showBenefits" class="mt-2">
                  <div v-if="isLoadingBenefits" class="skeleton h-8 w-full"></div>

                  <p v-else-if="!selectedFormId" class="text-xs text-base-content/45">
                    กรุณาเลือกประเภทเอกสารก่อน
                  </p>

                  <p v-else-if="!availableBenefits.length" class="text-xs text-warning">
                    พนักงานคนนี้ไม่มีสวัสดิการที่ใช้ได้ในประเภทเอกสารนี้
                  </p>

                  <ul v-else class="max-h-40 overflow-y-auto">
                    <li v-for="benefit in availableBenefits" :key="benefit.benefitId"
                      class="flex items-center justify-between gap-3">
                      <span class="min-w-0">
                        {{ benefit.nameTh }}
                      </span>

                      <span class="font-semibold shrink-0">
                        {{ limitText(benefit.limitAmount) }}
                      </span>
                    </li>
                  </ul>
                </div>
              </div>

              <div class="sm:col-span-2">
                <label for="benefit-remark" class="block text-xs font-medium mb-1.5">
                  หมายเหตุ
                </label>

                <textarea id="benefit-remark" v-model="formData.remark" rows="2"
                  placeholder="ระบุหมายเหตุเพิ่มเติม (ถ้ามี)"
                  class="textarea textarea-bordered w-full resize-none"></textarea>
              </div>
            </div>
          </section>

          <!-- ข้อมูลการเดินทาง -->
          <section v-if="isPerDiemLayout">
            <div class="flex items-center gap-2 mb-4">
              <div class="flex size-7 items-center justify-center rounded-lg bg-primary/10">
                <FileText class="size-3.5 text-primary" />
              </div>

              <div>
                <h4 class="text-sm font-semibold">ข้อมูลการเดินทาง</h4>
                <p class="text-[11px] text-base-content/40">
                  รายละเอียดการปฏิบัติงานนอกสถานที่
                </p>
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <FormSelect v-model="formData.tripType" label="ประเภทการเดินทาง" :options="tripTypeOptions"
                  placeholder="เลือกประเภทการเดินทาง" />
              </div>

              <div>
                <label for="benefit-destination" class="block text-xs font-medium mb-1.5">
                  สถานที่ / ประเทศปลายทาง
                </label>

                <input id="benefit-destination" v-model="formData.destination" type="text"
                  placeholder="ระบุสถานที่หรือประเทศ" class="input input-bordered input-sm w-full" />
              </div>

              <div class="sm:col-span-2">
                <label for="benefit-trip-purpose" class="block text-xs font-medium mb-1.5">
                  วัตถุประสงค์การเดินทาง
                </label>

                <textarea id="benefit-trip-purpose" v-model="formData.tripPurpose" rows="2"
                  placeholder="ระบุวัตถุประสงค์การเดินทาง"
                  class="textarea textarea-bordered w-full resize-none"></textarea>
              </div>

              <div>
                <label for="benefit-depart-at" class="block text-xs font-medium mb-1.5">
                  วันและเวลาออกเดินทาง
                </label>

                <input id="benefit-depart-at" v-model="formData.departAt" type="datetime-local"
                  class="input input-bordered input-sm w-full" />
              </div>

              <div>
                <label for="benefit-return-at" class="block text-xs font-medium mb-1.5">
                  วันและเวลากลับ
                </label>

                <input id="benefit-return-at" v-model="formData.returnAt" type="datetime-local"
                  class="input input-bordered input-sm w-full" />
              </div>

              <label class="flex items-center gap-2 text-sm cursor-pointer sm:col-span-2">
                <input v-model="formData.isOvernight" type="checkbox" class="checkbox checkbox-sm checkbox-primary" />
                จำเป็นต้องค้างแรม
              </label>
            </div>
          </section>

          <!-- รายการเบิก -->
          <section>
            <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
              <div>
                <div class="flex items-center gap-2">
                  <h4 class="text-sm font-semibold">รายการเบิก</h4>

                  <span class="badge badge-sm badge-ghost">
                    {{ formData.lines.length }} รายการ
                  </span>
                </div>

                <p class="text-[11px] text-base-content/40 mt-1">
                  เพิ่มรายการสวัสดิการที่ต้องการเบิก
                </p>
              </div>

              <button type="button" class="btn btn-primary btn-sm gap-1.5" :disabled="!selectedFormId ||
                isLoadingBenefits ||
                !availableBenefits.length
                " @click="addLine">
                <Plus class="size-4" />
                เพิ่มรายการ
              </button>
            </div>

            <p v-if="errors.lines" class="text-xs text-error mb-3" role="alert">
              {{ errors.lines }}
            </p>

            <div v-if="formData.lines.length === 0"
              class="rounded-2xl border border-dashed border-base-300 bg-base-200/20 px-4 py-8 text-center">
              <div class="mx-auto flex size-11 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                <ReceiptText class="size-5" />
              </div>

              <p class="text-sm font-medium mt-3">
                ยังไม่มีรายการเบิก
              </p>

              <p class="text-xs text-base-content/40 mt-1">
                {{ emptyLinesHint }}
              </p>
            </div>

            <div v-else class="space-y-3">
              <div v-for="(line, idx) in formData.lines" :key="idx" class="benefit-line-card">
                <div class="flex items-center justify-between mb-4">
                  <div class="flex items-center gap-2.5">
                    <div
                      class="flex size-8 items-center justify-center rounded-xl bg-primary/10 text-primary text-xs font-semibold">
                      {{ idx + 1 }}
                    </div>

                    <div>
                      <p class="text-xs font-semibold">
                        รายการที่ {{ idx + 1 }}
                      </p>

                      <p class="text-[11px] text-base-content/40 mt-0.5">
                        รายละเอียดการเบิก
                      </p>
                    </div>
                  </div>

                  <button type="button" class="btn btn-ghost btn-xs btn-square text-base-content/40"
                    :aria-label="`ลบรายการที่ ${idx + 1}`" title="ลบรายการ" @click="removeLine(idx)">
                    <Trash2 class="size-3.5" />
                  </button>
                </div>

                <div class="mb-3">
                  <FormSelect :model-value="line.benefitId" label="สวัสดิการ *" :options="benefitSelectOptions"
                    placeholder="เลือกสวัสดิการ" @update:model-value="(id) => onBenefitChange(line, id)" />

                  <p v-if="errors[`lines[${idx}].benefitId`]" class="text-xs text-error mt-1">
                    {{ errors[`lines[${idx}].benefitId`] }}
                  </p>
                </div>

                <div class="mb-3">
                  <label :for="`benefit-detail-${idx}`" class="block text-xs font-medium mb-1.5">
                    รายละเอียด <span class="text-error">*</span>
                  </label>

                  <textarea :id="`benefit-detail-${idx}`" v-model="line.detail" rows="2"
                    placeholder="ระบุรายละเอียดการเบิก"
                    class="textarea textarea-bordered w-full resize-none"></textarea>

                  <p v-if="errors[`lines[${idx}].detail`]" class="text-xs text-error mt-1">
                    {{ errors[`lines[${idx}].detail`] }}
                  </p>
                </div>

                <div v-if="line.benefitId" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <!-- น้ำมัน -->
                  <template v-if="line.calcType === 'Fuel'">
                    <div>
                      <label :for="`benefit-fuel-price-${idx}`" class="block text-xs font-medium mb-1.5">
                        ราคาน้ำมัน (บาท/ลิตร)
                      </label>

                      <input :id="`benefit-fuel-price-${idx}`" v-model.number="line.fuelPrice" v-decimal type="number" min="0"
                        step="0.001" class="input input-bordered input-sm w-full focus:border-primary" />

                      <p v-if="errors[`lines[${idx}].fuelPrice`]" class="text-xs text-error mt-1">
                        {{ errors[`lines[${idx}].fuelPrice`] }}
                      </p>
                    </div>

                    <div>
                      <label :for="`benefit-distance-${idx}`" class="block text-xs font-medium mb-1.5">
                        ระยะทาง (กม.)
                      </label>

                      <input :id="`benefit-distance-${idx}`" v-model.number="line.qty" v-decimal type="number" min="0" step="0.001"
                        placeholder="0" class="input input-bordered input-sm w-full" />

                      <p v-if="errors[`lines[${idx}].qty`]" class="text-xs text-error mt-1">
                        {{ errors[`lines[${idx}].qty`] }}
                      </p>
                    </div>

                    <div class="sm:col-span-2">
                      <label :for="`benefit-route-${idx}`" class="block text-xs font-medium mb-1.5">
                        เส้นทาง
                      </label>

                      <input :id="`benefit-route-${idx}`" v-model="line.route" type="text"
                        placeholder="ต้นทาง - ปลายทาง" class="input input-bordered input-sm w-full" />
                    </div>
                  </template>

                  <!-- รายวัน -->
                  <template v-else-if="line.calcType === 'Daily'">
                    <div class="sm:col-span-2">
                      <label :for="`benefit-days-${idx}`" class="block text-xs font-medium mb-1.5">
                        จำนวนวัน
                      </label>

                      <input :id="`benefit-days-${idx}`" v-model.number="line.qty" type="number" min="1" step="1"
                        class="input input-bordered input-sm w-full" />

                      <p v-if="errors[`lines[${idx}].qty`]" class="text-xs text-error mt-1">
                        {{ errors[`lines[${idx}].qty`] }}
                      </p>
                    </div>
                  </template>

                  <!-- เบี้ยเลี้ยง / ที่พัก -->
                  <template v-else-if="
                    line.calcType === 'PerDiem' ||
                    line.calcType === 'Accommodation'
                  ">
                    <div>
                      <label :for="`benefit-start-date-${idx}`" class="block text-xs font-medium mb-1.5">
                        {{
                          line.calcType === 'PerDiem'
                            ? 'วันที่เริ่มต้น'
                            : 'วันที่เข้าพัก'
                        }}
                      </label>

                      <input :id="`benefit-start-date-${idx}`" v-model="line.startDate" type="date"
                        class="input input-bordered input-sm w-full" />

                      <p v-if="errors[`lines[${idx}].startDate`]" class="text-xs text-error mt-1">
                        {{ errors[`lines[${idx}].startDate`] }}
                      </p>
                    </div>

                    <div>
                      <label :for="`benefit-end-date-${idx}`" class="block text-xs font-medium mb-1.5">
                        {{
                          line.calcType === 'PerDiem'
                            ? 'วันที่สิ้นสุด'
                            : 'วันที่ออก'
                        }}
                      </label>

                      <input :id="`benefit-end-date-${idx}`" v-model="line.endDate" type="date"
                        class="input input-bordered input-sm w-full" />

                      <p v-if="errors[`lines[${idx}].endDate`]" class="text-xs text-error mt-1">
                        {{ errors[`lines[${idx}].endDate`] }}
                      </p>
                    </div>

                    <p v-if="unitPreview(line)" class="sm:col-span-2 text-xs text-base-content/50">
                      {{ unitPreview(line) }}
                    </p>
                  </template>

                  <!-- เบิกตามจริง -->
                  <div v-else class="sm:col-span-2">
                    <label :for="`benefit-amount-${idx}`" class="block text-xs font-medium mb-1.5">
                      ยอดเบิก
                    </label>

                    <div class="relative">
                      <input :id="`benefit-amount-${idx}`" v-model.number="line.amount" v-decimal type="number" min="0" step="0.001"
                        placeholder="0.000" class="input input-bordered input-sm w-full pr-12" />

                      <span class="absolute right-3 top-1/2 -translate-y-1/2 text-[11px] text-base-content/35">
                        บาท
                      </span>
                    </div>

                    <p v-if="errors[`lines[${idx}].amount`]" class="text-xs text-error mt-1">
                      {{ errors[`lines[${idx}].amount`] }}
                    </p>
                  </div>
                </div>

                <!-- ยอดที่ระบบคำนวณ -->
                <div v-if="line.benefitId && isComputed(line)"
                  class="mt-3 rounded-lg border border-base-200 bg-base-200/20 px-3 py-3">
                  <div class="flex flex-wrap items-center justify-between gap-2">
                    <span class="text-xs text-base-content/50">
                      ยอดเบิก (ระบบคำนวณ)
                    </span>

                    <span class="text-sm font-semibold text-primary">
                      <template v-if="line.amount > 0">
                        {{ formatNumber(line.amount) }}
                        บาท
                      </template>

                      <span v-else class="text-xs font-normal text-base-content/40">
                        กรอกข้อมูลให้ครบเพื่อคำนวณ
                      </span>
                    </span>
                  </div>

                  <p v-if="line.calcError" class="mt-2 flex items-start gap-1.5 text-xs text-error" role="alert">
                    <CircleAlert class="size-3.5 shrink-0 mt-0.5" />
                    {{ line.calcError }}
                  </p>

                  <div v-if="line.breakdown.length" class="overflow-x-auto mt-3">
                    <table class="table table-xs w-full">
                      <thead>
                        <tr>
                          <th v-if="hasRange(line.breakdown)">ช่วง</th>
                          <th class="text-right">{{ qtyHeader(line) }}</th>
                          <th class="text-right">อัตรา</th>
                          <th class="text-right">รวม (บาท)</th>
                        </tr>
                      </thead>

                      <tbody>
                        <tr v-for="(row, rowIndex) in line.breakdown" :key="rowIndex">
                          <td v-if="hasRange(line.breakdown)">{{ rangeText(row) }}</td>
                          <td class="text-right">{{ fmt6(row.qty) }}</td>
                          <td class="text-right">{{ fmt6(row.rate) }}</td>
                          <td class="text-right">{{ formatNumber(row.amount) }}</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>

                <!-- วงเงินสิทธิ์ -->
                <div v-if="
                  line.benefitId &&
                  line.limitAmount > 0 &&
                  !isComputed(line)
                " class="mt-3 rounded-lg px-3 py-3 bg-base-200/30">
                  <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
                    <div class="flex flex-wrap items-center gap-2">
                      <Info class="size-3.5 text-base-content/40" />
                      <span class="text-xs text-base-content/50">
                        วงเงินสิทธิ์
                      </span>
                      <span class="text-xs font-semibold">
                        {{ formatNumber(line.limitAmount) }} บาท
                      </span>
                    </div>

                    <div v-if="line.amount > line.limitAmount" class="flex items-start gap-1.5 text-warning">
                      <TriangleAlert class="size-3.5 shrink-0 mt-0.5" />

                      <span class="text-xs font-medium">
                        เกินวงเงินอ้างอิง
                        {{ formatNumber(line.amount - line.limitAmount) }}
                        บาท (ตรวจสอบก่อนอนุมัติ)
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </section>

          <!-- ไฟล์แนบ -->
          <section>
            <div class="flex items-center gap-2 mb-3">
              <div class="flex size-7 items-center justify-center rounded-lg bg-primary/10">
                <FileText class="size-3.5 text-primary" />
              </div>

              <div>
                <h4 class="text-sm font-semibold">ไฟล์แนบ</h4>
                <p class="text-[11px] text-base-content/40">
                  เอกสารประกอบการเบิกสวัสดิการ
                </p>
              </div>
            </div>

            <AttachmentList doc-type="BenefitClaim" :doc-number="currentDocNum || null" :target="dialogRef" />
          </section>

          <!-- ยอดรวม -->
          <div class="benefit-total-card">
            <div class="benefit-total-main">
              <div>
                <p class="benefit-total-label">ยอดรวมที่ขอเบิก</p>
                <p class="benefit-total-caption">
                  รวม {{ formData.lines.length }} รายการ
                </p>
              </div>

              <div class="benefit-total-amount">
                <strong>{{ formatNumber(totalAmount) }}</strong>
                <span>บาท</span>
              </div>
            </div>

            <p v-if="hasPendingCalc" class="benefit-total-note">
              ยอดที่ระบบคำนวณเป็นยอดประมาณการ
              ระบบจะคำนวณจริงอีกครั้งเมื่อบันทึก
            </p>
          </div>

          <!-- ข้อผิดพลาด -->
          <div v-if="errorMessage"
            class="flex items-start gap-2 rounded-2xl border border-error/20 bg-error/10 text-error px-4 py-3 text-xs font-medium"
            role="alert">
            <CircleAlert class="size-4 shrink-0" />
            {{ errorMessage }}
          </div>
        </div>

        <!-- Footer -->
        <footer class="benefit-form-footer">
          <span class="benefit-footer-note">
            ตรวจสอบข้อมูลและรายการเบิกก่อนบันทึก
          </span>

          <div class="benefit-footer-actions">
            <button type="button" class="btn benefit-cancel-button" @click="close">
              ยกเลิก
            </button>

            <button type="submit" class="btn benefit-save-button" :disabled="isSubmitting">
              <span v-if="isSubmitting" class="loading loading-spinner loading-xs"></span>

              {{ isSubmitting ? 'กำลังบันทึก...' : 'บันทึกใบเบิก' }}
            </button>
          </div>
        </footer>
      </form>
    </div>

    <form method="dialog" class="modal-backdrop">
      <button aria-label="ปิดหน้าต่าง">ปิด</button>
    </form>
  </dialog>
</template>

<script setup lang="ts">
import * as yup from 'yup'
import { ref, computed, watch } from 'vue'
import { notify, extractErrorMessage } from '../../utils/notify.ts'
import { tripTypeOptions, rangeText, fmt6 } from '../../utils/benefitClaim.ts'
import { formatNumber, roundNum, DECIMAL_PLACES } from '../../utils/number.ts'
import {
  X,
  Plus,
  Trash2,
  ReceiptText,
  FileText,
  Info,
  TriangleAlert,
  CircleAlert,
  ChevronDown,
  ChevronUp,
} from 'lucide-vue-next'
import FormSelect from '../ui/FormSelect.vue'
import EmployeeSelect from '../ui/EmployeeSelect.vue'
import AttachmentList from '../attachment/AttachmentList.vue'
import type {
  BenefitClaim,
  AvailableBenefit,
  BreakdownRow,
} from '../../types/BenefitClaim.ts'
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

const props = defineProps<{
  isCreate: boolean
  employeeId: string
  employeeName: string
  allowedEmployees: EmployeeOption[]
}>()

const emit = defineEmits<{
  save: [data: any, isCreate: boolean]
}>()

const benefitClaimStore = useBenefitClaimStore()

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

const allowedEmployeeOptions = computed<EmployeeOption[]>(() => {
  if (props.allowedEmployees.length === 0) {
    return [
      {
        id: props.employeeId,
        empId: '',
        fullNameTh: props.employeeName,
      },
    ]
  }

  return props.allowedEmployees
})

const selectedEmployee = computed(
  () =>
    allowedEmployeeOptions.value.find(
      (employee) => employee.id === selectedEmployeeId.value,
    ) ?? null,
)

const docFormOptions = computed<OptionItem[]>(() =>
  benefitClaimStore.docForms.map((form) => ({
    id: String(form.id),
    label: `${form.code} - ${form.nameTh}`,
  })),
)

const selectedForm = computed(
  () =>
    benefitClaimStore.docForms.find(
      (form) => String(form.id) === selectedFormId.value,
    ) ?? null,
)

const isPerDiemLayout = computed(
  () => selectedForm.value?.layout === 'PerDiem',
)

const benefitSelectOptions = computed<OptionItem[]>(() =>
  availableBenefits.value.map((benefit) => ({
    id: benefit.benefitId,
    label: benefit.nameTh,
  })),
)

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

const totalAmount = computed(() =>
  roundNum(
    formData.value.lines.reduce(
      (sum, line) => sum + roundNum(line.amount || 0),
      0,
    ),
  ),
)

const hasPendingCalc = computed(() =>
  formData.value.lines.some(
    (line) => line.benefitId && isComputed(line),
  ),
)

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
        benefitId: yup
          .string()
          .nullable()
          .required('กรุณาเลือกสวัสดิการ'),
        detail: yup
          .string()
          .trim()
          .required('กรุณาระบุรายละเอียด'),
      }),
    ),
})

const COMPUTED_CALC_TYPES = new Set([
  'Fuel',
  'PerDiem',
  'Accommodation',
  'Daily',
])

function limitText(amount: number) {
  return amount > 0
    ? `${formatNumber(amount)} บาท`
    : 'ไม่จำกัด'
}

function isComputed(line: LineForm) {
  return COMPUTED_CALC_TYPES.has(line.calcType ?? '')
}

function daysDiff(start: string, end: string) {
  const ms = new Date(end).getTime() - new Date(start).getTime()
  return Math.round(ms / 86400000)
}

function decimalPlaces(n: number | null | undefined): number {
  if (n == null || !Number.isFinite(n)) return 0
  const s = String(n)
  if (s.includes('e-')) return Number(s.split('e-')[1]) + (s.split('e-')[0].split('.')[1]?.length ?? 0)
  return s.split('.')[1]?.length ?? 0
}

// มีช่วง (วันที่ 1-5, 6-10, ...) เฉพาะเบี้ยเลี้ยง; Fuel/ที่พักไม่มี
function hasRange(rows: BreakdownRow[]) {
  return rows.some((r) => r.from != null)
}

function qtyHeader(line: LineForm) {
  if (line.calcType === 'Fuel') return 'ระยะทาง (กม.)'
  if (line.calcType === 'Accommodation') return 'จำนวนคืน'
  return 'จำนวนวัน'
}

function unitPreview(line: LineForm) {
  if (!line.startDate || !line.endDate) return ''

  const diff = daysDiff(line.startDate, line.endDate)

  if (line.calcType === 'PerDiem') {
    return diff >= 0 ? `จำนวน ${diff + 1} วัน` : ''
  }

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

function removeLine(index: number) {
  const line = formData.value.lines[index]

  if (line) {
    clearTimeout(calcTimers.get(line))
  }

  formData.value.lines.splice(index, 1)
}

function applyBenefit(
  line: LineForm,
  benefit: AvailableBenefit | undefined,
) {
  line.calcType = benefit?.calcType ?? null
  line.region = benefit?.region ?? null
  line.limitAmount = benefit?.limitAmount ?? 0
}

function onBenefitChange(
  line: LineForm,
  benefitId: string | null | undefined,
) {
  // คงรายละเอียดและรหัสบัญชีที่ผู้ใช้กรอกไว้
  const { detail, accountCode } = line

  Object.assign(line, blankLine(), {
    benefitId: benefitId || null,
    detail,
    accountCode,
  })

  applyBenefit(
    line,
    availableBenefits.value.find(
      (benefit) => benefit.benefitId === line.benefitId,
    ),
  )
}

async function loadAvailableBenefits() {
  availableBenefits.value = []

  if (!selectedEmployeeId.value || !selectedFormId.value) return

  isLoadingBenefits.value = true

  try {
    availableBenefits.value =
      await benefitClaimStore.getAvailableBenefits(
        selectedEmployeeId.value,
        Number(selectedFormId.value),
      )

    for (const line of formData.value.lines) {
      if (!line.benefitId) continue

      const benefit = availableBenefits.value.find(
        (item) => item.benefitId === line.benefitId,
      )

      if (!benefit) {
        line.benefitId = null
      }

      applyBenefit(line, benefit)
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

async function onEmployeeChange(
  employeeId: string | null | undefined,
) {
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
  selectedFormId.value = item?.docFormId
    ? String(item.docFormId)
    : ''

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
      lines: item.lines.map((line) => ({
        benefitId: line.benefitId,
        detail: line.detail,
        calcType: line.calcType ?? null,
        region: line.region ?? null,
        limitAmount: line.limitAmount,
        amount: line.amount,
        qty: line.qty,
        accountCode: line.accountCode ?? '',
        startDate: line.startDate
          ? line.startDate.split('T')[0]
          : '',
        endDate: line.endDate ? line.endDate.split('T')[0] : '',
        fuelPrice: line.fuelPrice ?? null,
        route: line.route ?? '',
        breakdown: line.breakdown ?? [],
        calcError: '',
      })),
    }

    // ใช้ยอดที่บันทึกจาก Backend โดยไม่คำนวณซ้ำตอนเปิด
    formData.value.lines.forEach((line) => {
      calcKeys.set(line, calcKey(line))
    })
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
  let valid = true

  formData.value.lines.forEach((line, index) => {
    const fail = (field: string, message: string) => {
      const path = `lines[${index}].${field}`

      if (!errors.value[path]) {
        errors.value[path] = message
      }

      valid = false
    }

    if (line.calcError) {
      valid = false
    }

    if (line.calcType === 'Fuel') {
      if (!((line.fuelPrice ?? 0) > 0)) fail('fuelPrice', 'กรุณาระบุราคาน้ำมัน')
      else if (decimalPlaces(line.fuelPrice) > DECIMAL_PLACES) fail('fuelPrice', `ทศนิยมได้ไม่เกิน ${DECIMAL_PLACES} ตำแหน่ง`)
      if (!(line.qty > 0)) fail('qty', 'กรุณาระบุระยะทาง')
      else if (decimalPlaces(line.qty) > DECIMAL_PLACES) fail('qty', `ทศนิยมได้ไม่เกิน ${DECIMAL_PLACES} ตำแหน่ง`)
    } else if (
      line.calcType === 'PerDiem' ||
      line.calcType === 'Accommodation'
    ) {
      if (!line.startDate) {
        fail('startDate', 'กรุณาระบุวันที่เริ่มต้น')
      }

      if (!line.endDate) {
        fail('endDate', 'กรุณาระบุวันที่สิ้นสุด')
      }

      if (line.startDate && line.endDate) {
        const diff = daysDiff(line.startDate, line.endDate)

        if (line.calcType === 'PerDiem' && diff < 0) {
          fail(
            'endDate',
            'วันที่สิ้นสุดต้องไม่ก่อนวันที่เริ่มต้น',
          )
        }

        if (line.calcType === 'Accommodation' && diff < 1) {
          fail(
            'endDate',
            'วันที่ออกต้องหลังวันที่เข้าพักอย่างน้อย 1 วัน',
          )
        }
      }
    } else if (line.calcType === 'Daily') {
      if (!(line.qty > 0) || !Number.isInteger(line.qty)) {
        fail('qty', 'กรุณาระบุจำนวนวัน (จำนวนเต็ม)')
      }
    } else if (!(line.amount > 0)) {
      fail('amount', 'ยอดเบิกต้องมากกว่า 0')
    } else if (decimalPlaces(line.amount) > DECIMAL_PLACES) {
      fail('amount', `ทศนิยมได้ไม่เกิน ${DECIMAL_PLACES} ตำแหน่ง`)
    }
  })

  return valid
}

// คำนวณยอดประมาณการจาก Backend
function lineReady(line: LineForm) {
  if (!line.benefitId) return false

  if (line.calcType === 'Fuel') {
    return line.qty > 0 && decimalPlaces(line.qty) <= 6
      && (line.fuelPrice ?? 0) > 0 && decimalPlaces(line.fuelPrice) <= 6
  }
  if (line.calcType === 'Daily') {
    return line.qty > 0 && Number.isInteger(line.qty)
  }
  return !!(line.startDate && line.endDate)
}

function scheduleCalc(line: LineForm) {
  clearTimeout(calcTimers.get(line))
  line.calcError = ''

  if (!isComputed(line) || !lineReady(line)) {
    line.amount = 0
    line.breakdown = []
    return
  }

  calcTimers.set(
    line,
    setTimeout(async () => {
      const seq = (calcSeq.get(line) ?? 0) + 1
      calcSeq.set(line, seq)

      try {
        const result = await benefitClaimStore.calculateLine({
          employeeId: selectedEmployeeId.value,
          docFormId: Number(selectedFormId.value),
          docDate: formData.value.docDate,
          line: {
            benefitId: line.benefitId!,
            qty: line.qty,
            startDate: line.startDate || null,
            endDate: line.endDate || null,
            fuelPrice: line.fuelPrice || null,
          },
        })

        if (calcSeq.get(line) !== seq) return

        line.amount = result.amount
        line.breakdown = result.breakdown ?? []
        line.calcError = ''
      } catch (err: unknown) {
        if (calcSeq.get(line) !== seq) return

        line.amount = 0
        line.breakdown = []
        line.calcError =
          extractErrorMessage(err) ||
          'คำนวณยอดไม่สำเร็จ กรุณาตรวจสอบข้อมูล'
      }
    }, 400),
  )
}

function calcKey(line: LineForm) {
  return [
    line.benefitId,
    line.calcType,
    line.qty,
    line.startDate,
    line.endDate,
    line.fuelPrice,
    formData.value.docDate,
  ].join('|')
}

// คำนวณใหม่เฉพาะรายการที่ข้อมูลเปลี่ยน
watch(
  () => formData.value.lines.map(calcKey).join(';'),
  () => {
    for (const line of formData.value.lines) {
      const key = calcKey(line)

      if (calcKeys.get(line) === key) continue

      calcKeys.set(line, key)
      scheduleCalc(line)
    }
  },
)

async function handleSubmit() {
  if (isSubmitting.value) return

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
  } catch (err: unknown) {
    valid = false

    if (err instanceof yup.ValidationError) {
      const validationErrors = err.inner.length ? err.inner : [err]

      validationErrors.forEach((error) => {
        if (error.path && !errors.value[error.path]) {
          errors.value[error.path] = error.message
        }
      })
    } else {
      errorMessage.value = extractErrorMessage(err)
    }
  }

  if (!validateLinesByType()) {
    valid = false
  }

  if (!valid) return

  isSubmitting.value = true

  try {
    const form = formData.value

    const trip = isPerDiemLayout.value
      ? {
        tripType: form.tripType,
        destination: form.destination || null,
        tripPurpose: form.tripPurpose || null,
        departAt: form.departAt || null,
        returnAt: form.returnAt || null,
        isOvernight: form.isOvernight,
      }
      : {
        tripType: null,
        destination: null,
        tripPurpose: null,
        departAt: null,
        returnAt: null,
        isOvernight: null,
      }

    const payload = {
      docDate: form.docDate,
      docFormId: Number(selectedFormId.value),
      employeeId: selectedEmployeeId.value,
      remark: form.remark || null,
      ...trip,
      lines: form.lines.map((line) => ({
        benefitId: line.benefitId,
        detail: line.detail,
        amount: isComputed(line) ? 0 : line.amount,
        qty: line.qty,
        accountCode: line.accountCode || null,
        startDate: line.startDate || null,
        endDate: line.endDate || null,
        route: line.route || null,
        fuelPrice: line.calcType === 'Fuel' ? line.fuelPrice : null,
      })),
    }

    emit('save', payload, !isEditMode.value)
  } finally {
    isSubmitting.value = false
  }
}

defineExpose({
  open,
  close,
  getDialogEl,
})
</script>

<style scoped src="../../assets/css/BenefitClaimFormModal.css"></style>