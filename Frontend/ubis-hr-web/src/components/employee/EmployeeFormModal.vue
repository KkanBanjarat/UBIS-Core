<template>
  <dialog ref="dialogRef" class="modal">
    <div class="modal-box w-11/12 max-w-3xl max-h-[90vh] p-0 overflow-hidden flex flex-col">
      <!-- Header -->
      <div class="flex items-start justify-between gap-3 px-6 py-5 border-b border-base-200 shrink-0">
        <div class="flex items-center gap-3">
          <div class="size-10 rounded-full bg-primary/10 text-primary flex items-center justify-center shrink-0">
            <UserPlus v-if="props.isCreate" class="size-5" />
            <UserCog v-else class="size-5" />
          </div>
          <div>
            <h3 class="font-semibold text-base leading-tight">
              {{ props.isCreate ? 'เพิ่มพนักงานใหม่' : 'แก้ไขข้อมูลพนักงาน' }}
            </h3>
            <p class="text-xs text-base-content/45 mt-0.5">
              {{ props.isCreate ? 'กรอกข้อมูลพนักงานให้ครบถ้วน' : 'แก้ไขข้อมูลแล้วกดบันทึก' }}
            </p>
          </div>
        </div>
        <button type="button" class="btn btn-ghost btn-sm btn-square" @click="close">
          <X class="size-4" />
        </button>
      </div>

      <!-- ✅ Error Alert -->
      <div v-if="Object.keys(errors).length > 0" class="bg-error/10 border-b border-error/30 px-6 py-3">
        <p class="text-sm text-error font-medium">❌ กรุณาระบุข้อมูลให้ครบถ้วน</p>
      </div>

      <form @submit.prevent="handleSubmit" class="flex flex-col flex-1 min-h-0">
        <div class="overflow-y-auto px-6 py-5 space-y-6 flex-1">
          <section class="space-y-3">
            <div class="flex items-center gap-2 text-xs font-semibold text-base-content/45 uppercase tracking-wide">
              <UserRound class="size-3.5" />
              ข้อมูลส่วนตัว
            </div>

            <div class="grid grid-cols-3 gap-3">
              <FormSelect
                v-model="formData.prefixNameTh"
                label="คำนำหน้า"
                :options="prefixSelectOptions"
                placeholder="-- เลือก --"
                clear-label="ไม่ระบุ"
              />
              <div class="col-span-2">
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                  ชื่อ (ไทย) <span class="text-error">*</span>
                </label>
                <input
                  v-model="formData.fNameTh"
                  type="text"
                  placeholder="ชื่อ"
                  :class="['input input-bordered w-full text-sm', hasError('fNameTh') && 'input-error']"
                />
                <p v-if="hasError('fNameTh')" class="text-xs text-error mt-1">{{ errors.fNameTh }}</p>
              </div>
            </div>

            <div>
              <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                นามสกุล (ไทย) <span class="text-error">*</span>
              </label>
              <input
                v-model="formData.lNameTh"
                type="text"
                placeholder="นามสกุล"
                :class="['input input-bordered w-full text-sm', hasError('lNameTh') && 'input-error']"
              />
              <p v-if="hasError('lNameTh')" class="text-xs text-error mt-1">{{ errors.lNameTh }}</p>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                  ชื่อ (อังกฤษ) <span class="text-error">*</span>
                </label>
                <input
                  v-model="formData.fNameEn"
                  type="text"
                  placeholder="First Name"
                  :class="['input input-bordered w-full text-sm', hasError('fNameEn') && 'input-error']"
                />
                <p v-if="hasError('fNameEn')" class="text-xs text-error mt-1">{{ errors.fNameEn }}</p>
              </div>
              <div>
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                  นามสกุล (อังกฤษ) <span class="text-error">*</span>
                </label>
                <input
                  v-model="formData.lNameEn"
                  type="text"
                  placeholder="Last Name"
                  :class="['input input-bordered w-full text-sm', hasError('lNameEn') && 'input-error']"
                />
                <p v-if="hasError('lNameEn')" class="text-xs text-error mt-1">{{ errors.lNameEn }}</p>
              </div>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                  Email <span class="text-error">*</span>
                </label>
                <input
                  v-model="formData.email"
                  type="email"
                  placeholder="name@example.com"
                  :class="['input input-bordered w-full text-sm', hasError('email') && 'input-error']"
                />
                <p v-if="hasError('email')" class="text-xs text-error mt-1">{{ errors.email }}</p>
              </div>
              <div>
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                  รหัสพนักงาน <span class="text-error">*</span>
                </label>
                <input
                  v-model="formData.empId"
                  type="text"
                  placeholder="เช่น 2024001"
                  :class="['input input-bordered w-full text-sm', hasError('empId') && 'input-error']"
                />
                <p v-if="hasError('empId')" class="text-xs text-error mt-1">{{ errors.empId }}</p>
              </div>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-medium text-base-content/60 mb-1.5">
                  วันเริ่มงาน <span class="text-error">*</span>
                </label>
                <input
                  v-model="formData.hireDate"
                  type="date"
                  :class="['input input-bordered w-full text-sm', hasError('hireDate') && 'input-error']"
                />
                <p v-if="hasError('hireDate')" class="text-xs text-error mt-1">{{ errors.hireDate }}</p>
              </div>
              <FormSelect
                v-model="formData.gender"
                label="เพศ"
                :options="genderSelectOptions"
                placeholder="-- เลือก --"
                clear-label="ไม่ระบุ"
              />
            </div>
          </section>

          <section class="space-y-3 pt-5 border-t border-base-200">
            <div class="flex items-center gap-2 text-xs font-semibold text-base-content/45 uppercase tracking-wide">
              <Briefcase class="size-3.5" />
              ตำแหน่งงาน
            </div>
            <div class="grid grid-cols-1 gap-3">
              <div>
                <FormSelect
                  v-model="formData.positionId"
                  label="ตำแหน่ง"
                  required
                  :options="props.positionOptions"
                  :class="hasError('positionId') ? 'border-error' : ''"
                />
                <p v-if="hasError('positionId')" class="text-xs text-error mt-1">{{ errors.positionId }}</p>
              </div>
              <div>
                <FormSelect
                  v-model="formData.positionLevelId"
                  label="ระดับตำแหน่ง"
                  required
                  :options="props.positionLevelOptions"
                />
                <p v-if="hasError('positionLevelId')" class="text-xs text-error mt-1">{{ errors.positionLevelId }}</p>
              </div>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <FormSelect
                  v-model="formData.employeeTypeId"
                  label="ประเภทพนักงาน"
                  required
                  :options="props.employeeTypeOptions"
                />
                <p v-if="hasError('employeeTypeId')" class="text-xs text-error mt-1">{{ errors.employeeTypeId }}</p>
              </div>
              <div>
                <FormSelect
                  v-model="formData.status"
                  label="สถานะ"
                  required
                  :options="statusOptions"
                />
                <p v-if="hasError('status')" class="text-xs text-error mt-1">{{ errors.status }}</p>
              </div>
            </div>
          </section>

          <section class="space-y-3 pt-5 border-t border-base-200">
            <div class="flex items-center justify-between gap-2">
              <div class="flex items-center gap-2 text-xs font-semibold text-base-content/45 uppercase tracking-wide">
                <Gift class="size-3.5" />
                กลุ่มสวัสดิการ
              </div>
              <span
                v-if="formData.benefitPlanIds.length > 0"
                class="badge badge-primary badge-sm font-normal"
              >
                เลือกแล้ว {{ formData.benefitPlanIds.length }} กลุ่ม
              </span>
            </div>
            <p class="text-xs text-base-content/40">พนักงานคนนี้อยู่ในกลุ่มสวัสดิการใดได้บ้าง (เลือกได้มากกว่า 1 กลุ่ม)</p>

            <div v-if="props.benefitPlanOptions.length === 0" class="text-xs text-base-content/40 py-2">
              ยังไม่มีกลุ่มสวัสดิการในระบบ
            </div>

            <div v-else class="rounded-lg border border-base-200 overflow-hidden">
              <!-- Search -->
              <div class="relative border-b border-base-200 bg-base-100">
                <Search class="absolute left-3 top-1/2 -translate-y-1/2 size-3.5 text-base-content/30 pointer-events-none" />
                <input
                  v-model="benefitPlanSearch"
                  type="text"
                  placeholder="ค้นหากลุ่มสวัสดิการ..."
                  class="w-full pl-9 pr-3 py-2 text-sm bg-transparent focus:outline-none"
                />
              </div>

              <!-- Scrollable list -->
              <div class="max-h-56 overflow-y-auto divide-y divide-base-200">
                <label
                  v-for="opt in filteredBenefitPlanOptions"
                  :key="opt.id"
                  class="flex items-center gap-2.5 px-3 py-2.5 cursor-pointer hover:bg-base-200/40 transition-colors"
                >
                  <input
                    type="checkbox"
                    :value="opt.id"
                    v-model="formData.benefitPlanIds"
                    class="checkbox checkbox-sm checkbox-primary shrink-0"
                  />
                  <span class="text-sm text-base-content/80 truncate">{{ opt.label }}</span>
                </label>

                <div v-if="filteredBenefitPlanOptions.length === 0" class="px-3 py-4 text-center text-xs text-base-content/35">
                  ไม่พบกลุ่มสวัสดิการที่ค้นหา
                </div>
              </div>
            </div>

            <!-- Selected chips -->
            <div v-if="selectedBenefitPlanLabels.length > 0" class="flex flex-wrap gap-1.5 pt-1">
              <span
                v-for="item in selectedBenefitPlanLabels"
                :key="item.id"
                class="inline-flex items-center gap-1 badge badge-ghost badge-sm font-normal pr-1"
              >
                {{ item.label }}
                <button
                  type="button"
                  class="hover:text-error"
                  @click="formData.benefitPlanIds = formData.benefitPlanIds.filter(id => id !== item.id)"
                >
                  <X class="size-3" />
                </button>
              </span>
            </div>
          </section>

          <section class="space-y-3 pt-5 border-t border-base-200">
            <div class="flex items-center gap-2 text-xs font-semibold text-base-content/45 uppercase tracking-wide">
              <Building2 class="size-3.5" />
              สังกัด
            </div>

            <div class="grid grid-cols-1 gap-3">
              <div>
                <FormSelect
                  :model-value="formData.companyId"
                  @update:model-value="onCompanyChange"
                  label="บริษัท"
                  required
                  :options="props.companyOptions"
                />
                <p v-if="hasError('companyId')" class="text-xs text-error mt-1">{{ errors.companyId }}</p>
              </div>
              <div>
                <FormSelect
                  v-model="formData.branchId"
                  label="สาขา"
                  required
                  :options="props.branchOptions"
                  :disabled="!formData.companyId"
                  :placeholder="formData.companyId ? 'เลือกสาขา' : 'เลือกบริษัทก่อน'"
                />
                <p v-if="hasError('branchId')" class="text-xs text-error mt-1">{{ errors.branchId }}</p>
              </div>
            </div>

            <div class="grid grid-cols-1 gap-3">
              <FormSelect
                v-model="formData.groupId"
                label="สายงาน (Group)"
                :options="props.groupOptions"
                placeholder="-- เลือก --"
                clear-label="ไม่ระบุ"
              />
              <FormSelect
                v-model="formData.departmentId"
                label="ฝ่าย (Department)"
                :options="props.departmentOptions"
                placeholder="-- เลือก --"
                clear-label="ไม่ระบุ"
              />
            </div>

            <div class="grid grid-cols-1 gap-3">
              <FormSelect
                v-model="formData.divisionId"
                label="แผนก (Division)"
                :options="props.divisionOptions"
                placeholder="-- เลือก --"
                clear-label="ไม่ระบุ"
              />
              <FormSelect
                v-model="formData.sectionId"
                label="ส่วนงาน (Section)"
                :options="props.sectionOptions"
                placeholder="-- เลือก --"
                clear-label="ไม่ระบุ"
              />
            </div>

            <div>
              <EmployeeSearchSelect
                  v-model="formData.reportToId"
                  label="หัวหน้างาน"
                  placeholder="ค้นหารหัสพนักงาน / ชื่อ"
                  :company-id="null"
                  :exclude-id="props.employee?.id ?? null"
                  :initial-label="reportToInitialLabel"
                  :disabled="!formData.companyId"
                />
                <p class="text-xs text-base-content/40 mt-1">เว้นว่างถ้าไม่มีหัวหน้า</p>
            </div>
          </section>
        </div>

        <!-- Footer -->
        <div class="flex justify-end gap-2 px-6 py-4 border-t border-base-200 bg-base-100 shrink-0">
          <button type="button" class="btn btn-ghost" @click="close">ยกเลิก</button>
          <button type="submit" class="btn btn-primary gap-2" :disabled="isSubmitting">
            <span v-if="isSubmitting" class="loading loading-spinner loading-xs"></span>
            {{ isSubmitting ? 'กำลังบันทึก...' : (props.isCreate ? 'เพิ่มพนักงาน' : 'บันทึกการแก้ไข') }}
          </button>
        </div>
      </form>
    </div>
    <form method="dialog" class="modal-backdrop">
      <button @click="close">ปิด</button>
    </form>
  </dialog>
</template>

<script setup lang="ts">
import { computed, ref,watch } from 'vue'
import type {Employee} from '../../types/Employee.ts'
import { X, UserPlus, UserCog, UserRound, Briefcase, Building2, Gift, Search } from 'lucide-vue-next'
import FormSelect from '../ui/FormSelect.vue'
import * as yup from 'yup'
import EmployeeSearchSelect from './EmployeeSearchSelect.vue'

const props = defineProps<{
  isCreate: boolean
  employee?: Employee | null
  positionOptions: Array<{ id: string; label: string }>
  positionLevelOptions: Array<{ id: string; label: string }>
  employeeTypeOptions: Array<{ id: string; label: string }>
  companyOptions: Array<{ id: string; label: string }>
  branchOptions: Array<{ id: string; label: string }>
  groupOptions: Array<{ id: string; label: string }>
  departmentOptions: Array<{ id: string; label: string }>
  divisionOptions: Array<{ id: string; label: string }>
  sectionOptions: Array<{ id: string; label: string }>
  benefitPlanOptions: Array<{ id: string; label: string }>
}>()

const emit = defineEmits<{
  save: [data: any, isCreate: boolean]
  'company-changed': [companyId: string | null]
}>()

const dialogRef = ref<HTMLDialogElement>()
const isSubmitting = ref(false)
const errors = ref<Record<string, string>>({}) // ✅ เก็บ errors

const validationSchema = yup.object({
  empId: yup.string().required('รหัสพนักงาน จำเป็น'),
  fNameTh: yup.string().required('ชื่อ (ไทย) จำเป็น'),
  lNameTh: yup.string().required('นามสกุล (ไทย) จำเป็น'),
  fNameEn: yup.string().required('ชื่อ (อังกฤษ) จำเป็น'),
  lNameEn: yup.string().required('นามสกุล (อังกฤษ) จำเป็น'),
  email: yup.string().email('Email ไม่ถูกต้อง').required('Email จำเป็น'),
  hireDate: yup.string().required('วันเริ่มงาน จำเป็น'),
  positionId: yup.string().required('ตำแหน่ง จำเป็น'),
  positionLevelId: yup.string().required('ระดับตำแหน่ง จำเป็น'),
  employeeTypeId: yup.string().required('ประเภทพนักงาน จำเป็น'),
  companyId: yup.string().required('บริษัท จำเป็น'),
  branchId: yup.string().required('สาขา จำเป็น'),
  status: yup.string().required('สถานะ จำเป็น'),
})

const formData = ref({
  empId: '',
  prefixNameTh: null as string | null,
  prefixNameEn: null as string | null,
  fNameTh: '',
  lNameTh: '',
  fNameEn: '',
  lNameEn: '',
  email: '',
  hireDate: '',
  positionId: null as string | null,
  positionLevelId: null as string | null,
  employeeTypeId: null as string | null,
  status: 'Active' as string | null,
  gender: null as string | null,
  companyId: null as string | null,
  branchId: null as string | null,
  groupId: null as string | null,
  departmentId: null as string | null,
  divisionId: null as string | null,
  sectionId: null as string | null,
  reportToId: null as string | null,
  benefitPlanIds: [] as string[],
})

const reportToInitialLabel = computed(() => {
  const emp = props.employee
  if (!emp || !emp.reportToId) return null
  const th = (emp as any).reportToNameTh
  const en = (emp as any).reportToNameEn
  if (!th && !en) return null
  return `${th ?? ''}${en ? ' (' + en + ')' : ''}`.trim()
})

const prefixOptions = [
  { id: 'Mr', nameTh: 'นาย', nameEn: 'Mr.' },
  { id: 'Mrs', nameTh: 'นาง', nameEn: 'Mrs.' },
  { id: 'Ms', nameTh: 'นางสาว', nameEn: 'Ms.' },
  { id: 'Dr', nameTh: 'ดร.', nameEn: 'Dr.' },
]

const prefixSelectOptions = prefixOptions.map(p => ({
  id: p.id,
  label: `${p.nameTh} (${p.nameEn})`,
}))

const genderSelectOptions = [
  { id: 'Male', label: 'ชาย' },
  { id: 'Female', label: 'หญิง' },
  { id: 'Other', label: 'อื่นๆ' },
]

const statusOptions = [
  { id: 'Active', label: 'ทำงานอยู่' },
  { id: 'Resigned', label: 'ลาออก' },
]

function hasError(field: string): boolean {
  return !!errors.value[field]
}

function open() {
  errors.value = {}
  if (!props.isCreate && props.employee) {
    const emp = props.employee
    const matchedPrefix = prefixOptions.find(
      p => p.nameTh === emp.prefixNameTh || p.nameEn === emp.prefixNameEn
    )
    formData.value = {
      empId: emp.empId,
      prefixNameTh: matchedPrefix?.id || null,
      prefixNameEn: matchedPrefix?.id || null, 
      fNameTh: emp.fNameTh,
      lNameTh: emp.lNameTh,
      fNameEn: emp.fNameEn,
      lNameEn: emp.lNameEn,
      email: emp.email,
      hireDate: emp.hireDate?.split('T')[0] || '',
      positionId: emp.positionId,
      positionLevelId: emp.positionLevelId,
      employeeTypeId: emp.employeeTypeId,
      status: emp.status,
      gender: emp.gender || null,
      companyId: emp.companyId || null,
      branchId: emp.branchId,
      groupId: emp.groupId || null,
      departmentId: emp.departmentId || null,
      divisionId: emp.divisionId || null,
      sectionId: emp.sectionId || null,
      reportToId: emp.reportToId || null,
      benefitPlanIds: emp.benefitPlans?.map(p => p.id) || [],
    }
  }  else {
    formData.value = {
      empId: '',
      prefixNameTh: null,
      prefixNameEn: '',
      fNameTh: '',
      lNameTh: '',
      fNameEn: '',
      lNameEn: '',
      email: '',
      hireDate: '',
      positionId: null,
      positionLevelId: null,
      employeeTypeId: null,
      status: 'Active',
      gender: null,
      companyId: null,
      branchId: null,
      groupId: null,
      departmentId: null,
      divisionId: null,
      sectionId: null,
      reportToId: null,
      benefitPlanIds: [],
    }
  }

  dialogRef.value?.showModal()
}

function close() {
  dialogRef.value?.close()
}

function onCompanyChange(companyId: string | null | undefined) {
  formData.value.companyId = companyId ?? null
  formData.value.branchId = null
  emit('company-changed', formData.value.companyId)
}

async function handleSubmit() {
  isSubmitting.value = true
  errors.value = {} // ✅ Reset errors

  try {
    await validationSchema.validate(formData.value, { abortEarly: false })

    let prefixNameEn = ''
    let prefixNameTh = ''
    if (formData.value.prefixNameTh) {
      const prefix = prefixOptions.find(p => p.id === formData.value.prefixNameTh)
      if (prefix) {
        prefixNameEn = prefix.nameEn
        prefixNameTh = prefix.nameTh
      }
    }
    const payload = {
      empId: formData.value.empId,
       prefixNameTh: prefixNameTh || null, 
      prefixNameEn: prefixNameEn || null,
      fNameTh: formData.value.fNameTh,
      lNameTh: formData.value.lNameTh,
      fNameEn: formData.value.fNameEn,
      lNameEn: formData.value.lNameEn,
      email: formData.value.email,
      hireDate: formData.value.hireDate,
      positionId: formData.value.positionId,
      positionLevelId: formData.value.positionLevelId,
      employeeTypeId: formData.value.employeeTypeId,
      status: formData.value.status,
      gender: formData.value.gender || null,
      companyId: formData.value.companyId,
      branchId: formData.value.branchId,
      groupId: formData.value.groupId || null,
      departmentId: formData.value.departmentId || null,
      divisionId: formData.value.divisionId || null,
      sectionId: formData.value.sectionId || null,
      reportToId: formData.value.reportToId || null,
      benefitPlanIds: formData.value.benefitPlanIds,
    }

    emit('save', payload, props.isCreate)
    
  } catch (err: any) {
    // ✅ เก็บ errors state
    if (err.inner && err.inner.length > 0) {
      err.inner.forEach((e: any) => {
        errors.value[e.path] = e.message
      })
    }
  } finally {
    isSubmitting.value = false
  }
}


const benefitPlanSearch = ref('')

const filteredBenefitPlanOptions = computed(() => {
  const q = benefitPlanSearch.value.trim().toLowerCase()
  if (!q) return props.benefitPlanOptions
  return props.benefitPlanOptions.filter(opt => opt.label.toLowerCase().includes(q))
})

const selectedBenefitPlanLabels = computed(() =>
  props.benefitPlanOptions.filter(opt => formData.value.benefitPlanIds.includes(opt.id))
)
defineExpose({ open, close })



</script>