<template>
  <main class="employee-list-page">
    <header class="el-heading">
      <div class="el-heading__intro">
        <span class="el-heading__icon">
          <Users :size="23" />
        </span>
        <div>
          <p class="el-eyebrow">UBIS CORE · PEOPLE DIRECTORY</p>
          <h1>
            รายชื่อพนักงาน
            <span>{{ totalCount.toLocaleString("th-TH") }} รายการ</span>
          </h1>
        </div>
      </div>
      <button type="button" class="el-button el-button--primary" @click="openCreateModal">
        <Plus :size="18" />เพิ่มพนักงานใหม่
      </button>
    </header>

    <section class="el-workspace" aria-label="รายชื่อและตัวกรองพนักงาน">
      <div class="el-filter-bar">
        <label class="el-search">
          <Search :size="18" /><input v-model="filter.search" type="search" placeholder="ค้นหาชื่อหรือรหัสพนักงาน..."
            aria-label="ค้นหาชื่อหรือรหัสพนักงาน" @input="onSearchInput" />
        </label>
        <div class="el-quick-select">
          <FormSelect v-model="filter.status" :options="statusOptions" placeholder="สถานะ: ทั้งหมด" />
        </div>
        <div class="el-quick-select">
          <FormSelect v-model="filter.employeeTypeId" :options="employeeTypeOptions" placeholder="ประเภท: ทั้งหมด" />
        </div>
        <div class="el-filter-actions">
          <button type="button" class="el-button el-button--outline" :class="{ 'is-active': showFilters }"
            :aria-expanded="showFilters" aria-controls="employee-advanced-filters" @click="showFilters = !showFilters">
            <SlidersHorizontal :size="16" />ตัวกรอง
            <ChevronDown :size="15" :class="{ 'el-rotate': showFilters }" />
          </button><button type="button" class="el-icon-button el-clear" :disabled="!hasActiveFilters"
            title="ล้างตัวกรองทั้งหมด" aria-label="ล้างตัวกรองทั้งหมด" @click="clearFilters">
            <X :size="18" />
          </button>
        </div>
      </div>
      <div v-if="showFilters" id="employee-advanced-filters" class="el-advanced-filters">
        <FormSelect v-model="filter.companyId" label="บริษัท" :options="companyOptions" placeholder="บริษัททั้งหมด" />
        <FormSelect v-model="filter.branchId" label="สาขา" :options="filterBranchOptions" :disabled="!filter.companyId"
          :placeholder="filter.companyId ? 'สาขาทั้งหมด' : 'เลือกบริษัทก่อน'" />
        <FormSelect v-model="filter.groupId" label="สายงาน" :options="groupOptions" placeholder="สายงานทั้งหมด" />
        <FormSelect v-model="filter.departmentId" label="ฝ่าย" :options="departmentOptions" placeholder="ฝ่ายทั้งหมด" />
        <FormSelect v-model="filter.divisionId" label="แผนก" :options="divisionOptions" placeholder="แผนกทั้งหมด" />
        <FormSelect v-model="filter.sectionId" label="ส่วนงาน" :options="sectionOptions" placeholder="ส่วนงานทั้งหมด" />
      </div>
      <div class="el-table-toolbar">
        <div class="el-table-toolbar__title">
          <span class="el-live-dot"></span>
          <h2>ข้อมูลพนักงาน</h2>
          <span v-if="hasActiveFilters" class="el-filter-tag">กำลังใช้ตัวกรอง</span>
        </div>
        <label class="el-page-size"><span>แสดง</span><select v-model.number="filter.pageSize"
            aria-label="จำนวนรายการต่อหน้า">
            <option :value="10">10</option>
            <option :value="20">20</option>
            <option :value="50">50</option>
            <option :value="100">100</option>
          </select><span>รายการ / หน้า</span></label>
      </div>

      <div v-if="isLoading" class="el-skeleton-list" aria-busy="true" role="status">
        <span class="el-sr-only">กำลังโหลดรายชื่อพนักงาน</span>
        <div v-for="i in 6" :key="i" class="el-skeleton" aria-hidden="true"></div>
      </div>
      <div v-else-if="errorMessage" class="el-state" role="alert">
        <CircleAlert :size="32" />
        <h3>โหลดข้อมูลไม่สำเร็จ</h3>
        <p>{{ errorMessage }}</p>
        <button type="button" class="el-button el-button--outline" @click="fetchEmployees">
          ลองใหม่
        </button>
      </div>
      <div v-else-if="employees.length === 0" class="el-state">
        <Users :size="34" />
        <h3>ไม่พบข้อมูลพนักงาน</h3>
        <p>ลองเปลี่ยนคำค้นหาหรือเงื่อนไขตัวกรอง</p>
        <button v-if="hasActiveFilters" type="button" class="el-button el-button--outline" @click="clearFilters">
          ล้างตัวกรอง
        </button>
      </div>
      <template v-else>
        <div class="el-table-scroll" tabindex="0" role="region"
          aria-label="ตารางรายชื่อพนักงาน เลื่อนเพื่อดูคอลัมน์ทั้งหมด">
          <table class="el-table">
            <caption class="el-sr-only">
              รายชื่อพนักงาน บริษัท ตำแหน่ง สังกัด และสถานะ
            </caption>
            <colgroup>
              <col class="el-col-person" />
              <col class="el-col-company" />
              <col class="el-col-position" />
              <col class="el-col-unit" />
              <col class="el-col-status" />
              <col class="el-col-updated" />
              <col class="el-col-actions" />
            </colgroup>
            <thead>
              <tr>
                <th scope="col">พนักงาน</th>
                <th scope="col">บริษัท / สาขา</th>
                <th scope="col">ตำแหน่ง</th>
                <th scope="col">สังกัด</th>
                <th scope="col">สถานะ</th>
                <th scope="col">แก้ไขล่าสุดโดย</th>
                <th scope="col" class="el-actions-heading">จัดการ</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(emp, idx) in employees" :key="emp.id">
                <td>
                  <div class="el-person">
                    <span class="el-avatar" :class="avatarColor(idx)" aria-hidden="true">{{ initials(emp.fNameEn ||
                      emp.fNameTh) }}</span>
                    <div class="el-cell-content">
                      <button type="button" class="el-person__name" :disabled="isLoadingEditData"
                        @click="viewEmployee(emp)">
                        {{ emp.fNameTh }} {{ emp.lNameTh }}
                      </button>
                      <p class="el-cell-secondary">
                        <span class="el-employee-id">{{ emp.empId }}</span><span v-if="emp.employeeTypeNameTh">
                          · {{ emp.employeeTypeNameTh }}</span>
                      </p>
                    </div>
                  </div>
                </td>
                <td>
                  <p class="el-cell-primary">{{ emp.companyCode || "-" }}</p>
                  <p class="el-cell-secondary">{{ emp.branchNameTh || "-" }}</p>
                </td>
                <td>
                  <div class="el-position">
                    <span class="el-level">L{{ emp.positionLevel ?? "-" }}</span>
                    <p class="el-cell-primary">
                      {{ emp.positionNameTh || "-" }}
                    </p>
                  </div>
                  <p class="el-cell-secondary">
                    {{ emp.positionNameEn || "-" }}
                  </p>
                </td>
                <td>
                  <p class="el-cell-primary">
                    {{ GetAffiliationDisplay(emp).NameTh || "-" }}
                  </p>
                  <p class="el-cell-secondary">
                    {{ GetAffiliationDisplay(emp).NameEn || "-" }}
                  </p>
                </td>
                <td>
                  <span class="el-status" :class="statusMeta(emp.status).class"><span></span>{{
                    statusMeta(emp.status).label || "ไม่ระบุ" }}</span>
                </td>
                <td class="el-updated-cell">
                  <p class="el-cell-primary">
                    {{ formatDateTime(emp.updatedAt, true) }}
                  </p>
                  <p class="el-cell-secondary">{{ emp.updatedBy || "-" }}</p>
                </td>
                <td>
                  <div class="el-row-actions">
                    <button type="button" class="el-icon-button" title="ดูรายละเอียด"
                      :aria-label="`ดูรายละเอียด ${emp.fNameTh} ${emp.lNameTh}`" :disabled="isLoadingEditData"
                      @click="viewEmployee(emp)">
                      <Eye :size="17" />
                    </button>

                    <button type="button" class="el-icon-button el-icon-button--edit" title="แก้ไขข้อมูล"
                      :aria-label="`แก้ไข ${emp.fNameTh} ${emp.lNameTh}`" :disabled="isLoadingEditData"
                      @click="openEditModal(emp)">
                      <Edit :size="17" />
                    </button>

                    <button type="button" class="el-icon-button el-icon-button--delete" title="ลบพนักงาน"
                      :aria-label="`ลบ ${emp.fNameTh} ${emp.lNameTh}`" @click="confirmDelete(emp)">
                      <Trash2 :size="17" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <footer class="el-footer">
          <p role="status">
            แสดง
            <strong>{{ (filter.page - 1) * filter.pageSize + 1 }}–{{
              Math.min(filter.page * filter.pageSize, totalCount)
            }}</strong>
            จาก {{ totalCount.toLocaleString("th-TH") }} รายการ
          </p>
          <nav class="el-pagination" aria-label="เปลี่ยนหน้ารายชื่อพนักงาน">
            <button type="button" class="el-page-button" aria-label="หน้าแรก" :disabled="filter.page === 1"
              @click="goToPage(1)">
              <ChevronsLeft :size="17" />
            </button><button type="button" class="el-page-button" aria-label="หน้าก่อนหน้า"
              :disabled="filter.page === 1" @click="goToPage(filter.page - 1)">
              <ChevronLeft :size="17" />
            </button><button v-for="p in pageWindow" :key="p" type="button" class="el-page-button"
              :class="{ 'is-current': p === filter.page }" :aria-current="p === filter.page ? 'page' : undefined"
              :aria-label="`หน้า ${p}`" @click="goToPage(p)">
              {{ p }}</button><button type="button" class="el-page-button" aria-label="หน้าถัดไป"
              :disabled="filter.page >= totalPages" @click="goToPage(filter.page + 1)">
              <ChevronRight :size="17" />
            </button><button type="button" class="el-page-button" aria-label="หน้าสุดท้าย"
              :disabled="filter.page >= totalPages" @click="goToPage(totalPages)">
              <ChevronsRight :size="17" />
            </button>
          </nav>
        </footer>
      </template>
    </section>
  </main>

  <EmployeeFormModal :key="selectedEmployee?.id ?? 'create'" ref="employeeFormModalRef" :is-create="isCreateMode"
    :employee="selectedEmployee" :position-options="positionOptions" :position-level-options="positionLevelOptions"
    :employee-type-options="employeeTypeOptions" :company-options="companyOptions" :branch-options="branchOptions"
    :group-options="groupOptions" :department-options="departmentOptions" :division-options="divisionOptions"
    :section-options="sectionOptions" :benefit-plan-options="benefitPlanOptions" @company-changed="handleCompanyChanged"
    @save="handleEmployeeSave" />

  <EmployeeDetailModal ref="employeeDetailModalRef" :employee="selectedEmployee" @edit="openEditModal" />
</template>

<script setup lang="ts">
import {
  ref,
  computed,
  watch,
  onMounted,
  reactive,
  nextTick,
  onBeforeUnmount,
} from "vue";
import { storeToRefs } from "pinia";
import {
  useEmployeeStore,
  type OptionItem,
} from "../../stores/employeeStore.ts";
import type { Employee, EmployeeFilter } from "../../types/Employee.ts";
import FormSelect from "../../components/ui/FormSelect.vue";
import EmployeeFormModal from "../../components/employee/EmployeeFormModal.vue";
import EmployeeDetailModal from "../../components/employee/EmployeeDetailModal.vue";
import { notify, extractErrorMessage } from "../../utils/notify";
import {
  Search,
  ChevronDown,
  SlidersHorizontal,
  X,
  Users,
  CircleAlert,
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
  Plus,
  Eye,
  Trash2,
  Edit,
} from "lucide-vue-next";
const emit = defineEmits<{
  view: [id: string];
  delete: [id: string];
}>();
const employeeStore = useEmployeeStore();
const {
  employees,
  totalCount,
  isLoading,
  errorMessage,
  positionOptions,
  positionLevelOptions,
  employeeTypeOptions,
  companyOptions,
  groupOptions,
  departmentOptions,
  divisionOptions,
  sectionOptions,
  benefitPlanOptions,
} = storeToRefs(employeeStore);
const defaultFilter: EmployeeFilter = {
  search: "",
  status: null,
  employeeTypeId: null,
  companyId: null,
  branchId: null,
  groupId: null,
  departmentId: null,
  divisionId: null,
  sectionId: null,
  page: 1,
  pageSize: 10,
};
// ========================================
// UI State
// ========================================
const showFilters = ref(false);
const isLoadingEditData = ref(false);
// ========================================
// Filter & Pagination State
// ========================================
const filter = reactive<EmployeeFilter>({ ...defaultFilter });
// ========================================
// Modal State
// ========================================
const employeeFormModalRef = ref<InstanceType<typeof EmployeeFormModal>>();
const employeeDetailModalRef = ref<InstanceType<typeof EmployeeDetailModal>>();
const isCreateMode = ref(true);
const selectedEmployee = ref<Employee | null>(null);
// ========================================
// Branch Options (แยก 2 ชุด: ของ Filter กับของ Form)
// ========================================
const branchOptions = ref<OptionItem[]>([]);
const filterBranchOptions = ref<OptionItem[]>([]);
// Static Options
// ========================================
const statusOptions: OptionItem[] = [
  { id: "Active", label: "ทำงานอยู่" },
  { id: "Resigned", label: "ลาออก" },
  { id: "FailedProbation", label: "ไม่ผ่านทดลองงาน" },
];
// ========================================
// Computed
// ========================================
const totalPages = computed(
  () => Math.ceil(totalCount.value / filter.pageSize) || 1,
);
const hasActiveFilters = computed(
  () =>
    !!filter.search ||
    !!filter.status ||
    !!filter.employeeTypeId ||
    !!filter.companyId ||
    !!filter.branchId ||
    !!filter.groupId ||
    !!filter.departmentId ||
    !!filter.divisionId ||
    !!filter.sectionId,
);
const pageWindow = computed(() => {
  const maxButtons = 5;
  let start = Math.max(1, filter.page - Math.floor(maxButtons / 2));
  let end = start + maxButtons - 1;
  if (end > totalPages.value) {
    end = totalPages.value;
    start = Math.max(1, end - maxButtons + 1);
  }
  const pages: number[] = [];
  for (let p = start; p <= end; p++) pages.push(p);
  return pages;
});
// ========================================
// Avatar & Status Helpers
// ========================================
const avatarPalette = [
  "bg-emerald-100 text-emerald-700",
  "bg-teal-100 text-teal-700",
  "bg-green-100 text-green-700",
  "bg-lime-100 text-lime-800",
  "bg-cyan-100 text-cyan-700",
  "bg-emerald-200 text-emerald-800",
];
function avatarColor(idx: number) {
  return avatarPalette[idx % avatarPalette.length];
}
function statusMeta(rawStatus: string) {
  if (rawStatus === "ปกติ" || rawStatus === "Active") {
    return { label: "ทำงานอยู่", class: "badge-success text-success-content" };
  }
  if (rawStatus === "ลาออก" || rawStatus === "Resigned") {
    return { label: "ลาออก", class: "badge-error text-error-content" };
  }
  if (rawStatus === "ไม่ผ่านทดลองงาน" || rawStatus === "FailedProbation") {
    return {
      label: "ไม่ผ่านทดลองงาน",
      class: "badge-error text-error-content",
    };
  }
  return { label: rawStatus, class: "badge-ghost" };
}
function GetAffiliationDisplay(emp: Employee) {
  if (emp.sectionId)
    return { NameTh: emp.sectionNameTh, NameEn: emp.sectionNameEn };
  else if (emp.divisionId)
    return { NameTh: emp.divisionNameTh, NameEn: emp.divisionNameEn };
  else if (emp.departmentId)
    return { NameTh: emp.departmentNameTh, NameEn: emp.departmentNameEn };
  else return { NameTh: emp.groupNameTh, NameEn: emp.groupNameEn };
}
function initials(name: string) {
  return name?.charAt(0)?.toUpperCase() ?? "?";
}
// ========================================
// Fetch
// ========================================
function fetchEmployees() {
  return employeeStore.fetchList(filter);
}
// ========================================
// Modal Actions
// ========================================
async function openCreateModal() {
  selectedEmployee.value = null;
  branchOptions.value = [];
  isCreateMode.value = true;
  await nextTick();
  employeeFormModalRef.value?.open();
}
async function viewEmployee(emp: Employee) {
  isLoadingEditData.value = true;
  try {
    selectedEmployee.value = await employeeStore.getById(emp.id);
    employeeDetailModalRef.value?.open();
  } catch (err) {
    await notify.error("โหลดข้อมูลพนักงานไม่สำเร็จ");
    console.error("Failed to load employee detail:", err);
  } finally {
    isLoadingEditData.value = false;
  }
}
async function openEditModal(employee: Employee) {
  isCreateMode.value = false;
  isLoadingEditData.value = true;
  try {
    // หน้าแก้ไขไม่ได้ใช้แผนผังองค์กร จึงไม่ต้องให้ Backend คำนวณมาให้
    const data = await employeeStore.getById(employee.id, false);
    selectedEmployee.value = data;
    if (data.companyId) {
      await handleCompanyChanged(data.companyId);
    } else {
      branchOptions.value = [];
    }
    await nextTick();
    employeeFormModalRef.value?.open();
  } catch (err) {
    await notify.error("โหลดข้อมูลพนักงานไม่สำเร็จ");
    console.error("Failed to load employee detail:", err);
  } finally {
    isLoadingEditData.value = false;
  }
}
// ========================================
// Event: Company Changed (from Modal)
// ========================================
async function handleCompanyChanged(companyId: string | null) {
  if (!companyId) {
    branchOptions.value = [];
    return;
  }
  try {
    branchOptions.value = await employeeStore.getBranchesByCompany(companyId);
  } catch (err) {
    await notify.error("เกิดข้อผิดพลาดในการดึงข้อมูลสาขา");
    console.error("Failed to load branches:", err);
    branchOptions.value = [];
  }
}
async function loadFilterBranches(companyId: string | null | undefined) {
  if (!companyId) {
    filterBranchOptions.value = [];
    return;
  }
  try {
    filterBranchOptions.value =
      await employeeStore.getBranchesByCompany(companyId);
  } catch (err) {
    console.error("Failed to load filter branches:", err);
    filterBranchOptions.value = [];
  }
}
// ========================================
// Event: Employee Save (from Modal)
// ========================================
async function handleEmployeeSave(payload: any, isCreate: boolean) {
  try {
    if (isCreate) {
      await employeeStore.create(payload);
    } else {
      await employeeStore.update(selectedEmployee.value!.id, payload);
    }
    employeeFormModalRef.value?.close();
    filter.page = 1;
    await fetchEmployees();
    await notify.success(
      isCreate ? "เพิ่มพนักงานสำเร็จ" : "บันทึกการแก้ไขสำเร็จ",
    );
  } catch (err: any) {
    console.error("Save error:", err);
    await notify.error(
      extractErrorMessage(err),
      "เกิดข้อผิดพลาด",
      employeeFormModalRef.value?.dialogEl,
    );
  }
}
async function confirmDelete(emp: Employee) {
  const ok = await notify.confirm(
    `ยืนยันลบพนักงาน "${emp.fNameTh} ${emp.lNameTh}" ใช่หรือไม่`,
    "ยืนยันการลบ",
  );
  if (!ok) return;
  try {
    await employeeStore.remove(emp.id);
    await notify.success("ลบพนักงานสำเร็จ");
    if (employees.value.length === 1 && filter.page > 1) {
      filter.page -= 1;
    }
    await fetchEmployees();
    emit("delete", emp.id);
  } catch (err: any) {
    await notify.error(extractErrorMessage(err), "ลบไม่สำเร็จ");
    console.error("Delete error:", err);
  }
}
// ========================================
// Filter & Search Handlers
// ========================================
function clearFilters() {
  clearTimeout(debounceTimer);
  Object.assign(filter, defaultFilter);
  filterBranchOptions.value = [];
  fetchEmployees();
}
function resetPageAndFetch() {
  filter.page = 1;
  fetchEmployees();
}
let debounceTimer: ReturnType<typeof setTimeout>;
function onSearchInput() {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(resetPageAndFetch, 300);
}
function goToPage(p: number) {
  filter.page = p;
  fetchEmployees();
}
function formatDateTime(
  value: string | null | undefined,
  showTime: boolean = false,
) {
  if (!value) return "-";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";

  const dateText = new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(date);

  if (!showTime) return dateText;

  const timeText = new Intl.DateTimeFormat("en-GB", {
    hour: "2-digit",
    minute: "2-digit",
    hourCycle: "h23",
  }).format(date);

  return `${dateText} ${timeText}`;
}
// ========================================
// Watchers
// ========================================
watch(
  () => [
    filter.status,
    filter.employeeTypeId,
    filter.groupId,
    filter.departmentId,
    filter.divisionId,
    filter.sectionId,
    filter.pageSize,
  ],
  resetPageAndFetch,
);
watch(
  () => filter.companyId,
  (newCompanyId) => {
    filter.branchId = null;
    loadFilterBranches(newCompanyId);
    resetPageAndFetch();
  },
);
watch(() => filter.branchId, resetPageAndFetch);
// ========================================
// Lifecycle
// ========================================
onMounted(() => {
  // 2 อย่างนี้ไม่ต้องรอกัน — ตารางขึ้นได้ทันทีโดยไม่ต้องรอ Dropdown โหลดเสร็จ
  employeeStore.fetchOptions(); // มี Cache ในตัว เข้ารอบ 2 จะไม่ยิงซ้ำ
  fetchEmployees();
});
onBeforeUnmount(() => clearTimeout(debounceTimer));
</script>

<style scoped src="../../assets/css/EmployeeList.css"></style>
