<template>
  <div class="p-4 sm:p-6 space-y-5 mx-auto">
    <!-- Header -->
    <div>
      <h1 class="text-xl font-semibold text-base-content">รายชื่อพนักงาน</h1>
      <p class="text-sm text-base-content/50 mt-0.5">
        {{ totalCount }} รายการทั้งหมด
      </p>
    </div>

    <!-- Filter card -->
    <div
      class="bg-base-100 rounded-2xl shadow-sm border border-base-200 p-5 space-y-4"
    >
      <div class="flex flex-col lg:flex-row gap-3 lg:items-center">
        <!-- Search -->
        <div class="relative flex-1 min-w-[240px]">
          <Search
            class="absolute left-3.5 top-1/2 -translate-y-1/2 size-4 text-gray-400 pointer-events-none z-10"
          />
          <input
            v-model="filter.search"
            @input="onSearchInput"
            type="text"
            placeholder="ค้นหาชื่อพนักงาน, รหัสพนักงาน..."
            class="input input-bordered w-full pl-10 text-sm focus:outline-none focus:border-primary transition-colors"
          />
        </div>

        <!-- Quick filters + buttons -->
        <div class="flex flex-wrap gap-2 items-center">
          <div class="w-full sm:w-52">
            <FormSelect
              v-model="filter.status"
              :options="statusOptions"
              placeholder="สถานะ: ทั้งหมด"
            />
          </div>
          <div class="w-full sm:w-60">
            <FormSelect
              v-model="filter.employeeTypeId"
              :options="employeeTypeOptions"
              placeholder="ประเภท: ทั้งหมด"
            />
          </div>
          <div class="flex gap-2">
            <button
              @click="showFilters = !showFilters"
              class="btn btn-sm gap-1.5"
              :class="
                showFilters ? 'btn-primary' : 'btn-ghost border border-base-300'
              "
            >
              <SlidersHorizontal class="size-3.5" />
              <span class="hidden sm:inline">ตัวกรอง</span>
              <ChevronDown
                class="size-3.5 transition-transform duration-200"
                :class="showFilters && 'rotate-180'"
              />
            </button>
            <button
              class="btn btn-ghost btn-sm gap-1 text-base-content/55 hover:text-error"
              :disabled="!hasActiveFilters"
              @click="clearFilters"
            >
              <X class="size-3.5" />
            </button>
          </div>
        </div>
      </div>

      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 -translate-y-1"
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0 -translate-y-1"
      >
        <div  v-if="showFilters"
          class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 pt-4 border-t border-base-200">
          <FormSelect
            v-model="filter.companyId"
            label="บริษัท"
            :options="companyOptions"
          />
          <FormSelect
            v-model="filter.branchId"
            label="สาขา"
            :options="filterBranchOptions"
            :disabled="!filter.companyId"
            :placeholder="filter.companyId ? 'เลือกสาขา' : 'เลือกบริษัทก่อน'"
          />
          <FormSelect
            v-model="filter.groupId"
            label="สายงาน"
            :options="groupOptions"
          />
          <FormSelect
            v-model="filter.departmentId"
            label="ฝ่าย"
            :options="divisionOptions"
          />
          <FormSelect
            v-model="filter.sectionId"
            label="ส่วนงาน"
            :options="sectionOptions"
          />
        </div>
      </Transition>
    </div>

    <!-- Table card -->
    <div class="bg-base-100 rounded-2xl shadow-sm border border-base-200 overflow-hidden">
      <!-- Table toolbar -->
      <div
        class="flex items-center justify-between gap-3 px-5 py-4 border-b border-base-200"
      >
        <select
          v-model.number="filter.pageSize"
          class="select select-bordered select-sm w-20"
        >
          <option :value="10">10</option>
          <option :value="20">20</option>
          <option :value="50">50</option>
          <option :value="100">100</option>
        </select>
        <button class="btn btn-primary btn-sm gap-1.5" @click="openCreateModal">
          <Plus class="size-4" />
          เพิ่มพนักงานใหม่
        </button>
      </div>

      <!-- Loading skeleton -->
      <div v-if="isLoading" class="p-5 space-y-3">
        <div
          v-for="i in 6"
          :key="i"
          class="skeleton h-12 w-full rounded-lg"
        ></div>
      </div>

      <!-- Error -->
      <div
        v-else-if="errorMessage"
        class="flex flex-col items-center gap-2 p-14 text-center"
      >
        <CircleAlert class="size-8 text-error/70" />
        <p class="text-error text-sm">{{ errorMessage }}</p>
      </div>

      <template v-else>
        <!-- Empty state -->
        <div v-if="employees.length === 0"
          class="flex flex-col items-center gap-2 p-14 text-center">
          <Users class="size-8 text-base-content/25" />
          <p class="text-base-content/50 text-sm">
            ไม่พบข้อมูลพนักงานตามเงื่อนไขที่เลือก
          </p>
        </div>

        <template v-else>
          <div class="overflow-x-auto">
            <table class="table min-w-[880px]">
              <thead>
                <tr
                  class="text-xs uppercase tracking-wide text-base-content/45 border-b border-base-200"
                >
                  <th class="bg-base-100">พนักงาน</th>
                  <th class="bg-base-100">บริษัท/สาขา</th>
                  <th class="bg-base-100">ตำแหน่ง</th>
                  <th class="bg-base-100">สังกัด</th>
                  <th class="bg-base-100">สถานะ</th>
                  <th class="bg-base-100 text-right">จัดการ</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(emp, idx) in employees"
                  :key="emp.id"
                  class="hover:bg-base-200/40 transition-colors border-b border-base-200/60 last:border-0">
                  <td>
                    <div class="flex items-center gap-2.5">
                      <div class="avatar placeholder shrink-0">
                        <div class="rounded-full w-9 h-9 flex items-center justify-center"
                          :class="avatarColor(idx)">
                          <span class="text-sm font-semibold leading-none">{{initials(emp.fNameEn)}}</span>
                        </div>
                      </div>
                      <div class="min-w-0">
                        <div class="font-semibold text-base-content/80 truncate flex items-center gap-1.5">
                          <span class="truncate">{{ emp.fNameTh }} {{ emp.lNameTh }}</span>
                        </div>
                        <div class="text-sm text-base-content/45 truncate">
                          {{ emp.empId }} : {{ emp.employeeTypeNameTh }}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td>
                    <div class="text-base-content/80 font-semibold">
                      {{ emp.companyCode }}
                    </div>
                    <div class="text-sm text-base-content/40">
                      {{ emp.branchNameTh }}
                    </div>
                  </td>
                  <td>
                    <div class="truncate flex items-center gap-1.5">
                      <div class="badge badge-soft badge-info badge-sm p-1 font-semibold">
                        L{{ emp.positionLevel }}
                      </div>
                      <div class="text-base-content/80 font-semibold">
                        {{ emp.positionNameTh }}
                      </div>
                    </div>
                    <div class="text-sm text-base-content/40">
                      {{ emp.positionNameEn }}
                    </div>
                  </td>
                  <td>
                    <div class="text-base-content/80 font-semibold">
                      {{ GetAffiliationDisplay(emp).NameTh }}
                    </div>
                    <div class="text-sm text-base-content/40">
                      {{ GetAffiliationDisplay(emp).NameEn }}
                    </div>
                  </td>
                  <td>
                    <span class="badge badge-sm font-normal whitespace-nowrap"
                      :class="statusMeta(emp.status).class">
                      {{ statusMeta(emp.status).label }}
                    </span>
                  </td>
                  <td>
                    <div class="flex items-center justify-end gap-1">
                      <button class="btn btn-ghost btn-xs btn-square"
                        title="ดูรายละเอียด"
                        @click="viewEmployee(emp)">
                        <Eye class="size-4" />
                      </button>
                      <button
                        class="btn btn-ghost btn-xs btn-square text-warning/70 hover:text-warning hover:bg-warning/10"
                        title="แก้ไขข้อมูล"
                        @click="openEditModal(emp)"
                      >
                        <Edit class="size-4" />
                      </button>
                      <button
                        class="btn btn-ghost btn-xs btn-square text-error/70 hover:text-error hover:bg-error/10"
                        title="ลบ"
                        @click="confirmDelete(emp)"
                      >
                        <Trash2 class="size-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div
            class="flex flex-col sm:flex-row items-center justify-between gap-3 px-5 py-4 border-t border-base-200"
          >
            <p class="text-xs text-base-content/45">
              แสดง {{ (filter.page - 1) * filter.pageSize + 1 }}–{{
                Math.min(filter.page * filter.pageSize, totalCount)
              }}
              จาก {{ totalCount }}
              รายการ
            </p>
            <div class="join">
              <button
                class="join-item btn btn-sm btn-ghost"
                :disabled="filter.page === 1"
                @click="goToPage(1)"
              >
                <ChevronsLeft class="size-4" />
              </button>
              <button
                class="join-item btn btn-sm btn-ghost"
                :disabled="filter.page === 1"
                @click="goToPage(filter.page - 1)"
              >
                <ChevronLeft class="size-4" />
              </button>
              <button
                v-for="p in pageWindow"
                :key="p"
                class="join-item btn btn-sm"
                :class="p === filter.page ? 'btn-primary' : 'btn-ghost'"
                @click="goToPage(p)"
              >
                {{ p }}
              </button>
              <button
                class="join-item btn btn-sm btn-ghost"
                :disabled="filter.page >= totalPages"
                @click="goToPage(filter.page + 1)"
              >
                <ChevronRight class="size-4" />
              </button>
              <button
                class="join-item btn btn-sm btn-ghost"
                :disabled="filter.page >= totalPages"
                @click="goToPage(totalPages)"
              >
                <ChevronsRight class="size-4" />
              </button>
            </div>
          </div>
        </template>
      </template>
    </div>
  </div>

  <!-- Modal -->
  <EmployeeFormModal
    :key="selectedEmployee?.id ?? 'create'"
    ref="employeeFormModalRef"
    :is-create="isCreateMode"
    :employee="selectedEmployee"
    :position-options="positionOptions"
    :position-level-options="positionLevelOptions"
    :employee-type-options="employeeTypeOptions"
    :company-options="companyOptions"
    :branch-options="branchOptions"
    :group-options="groupOptions"
    :department-options="departmentOptions"
    :division-options="divisionOptions"
    :section-options="sectionOptions"
    :benefit-plan-options="benefitPlanOptions"
    @company-changed="handleCompanyChanged"
    @save="handleEmployeeSave"
  />

  <EmployeeDetailModal
    ref="employeeDetailModalRef"
    :employee="selectedEmployee"
    @edit="openEditModal"
  />
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, reactive, nextTick } from "vue";
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

// dropdown "หน่วยงาน" รวม 4 ประเภท map เข้า filter จริง
const organizationUnitId = computed<string | null>({
  get() {
    return (
      filter.groupId ||
      filter.departmentId ||
      filter.divisionId ||
      filter.sectionId ||
      null
    );
  },
  set(id) {
    filter.groupId = null;
    filter.departmentId = null;
    filter.divisionId = null;
    filter.sectionId = null;
    if (!id) return;
    if (groupOptions.value.some((o) => o.id === id)) filter.groupId = id;
    else if (departmentOptions.value.some((o) => o.id === id))
      filter.departmentId = id;
    else if (divisionOptions.value.some((o) => o.id === id))
      filter.divisionId = id;
    else if (sectionOptions.value.some((o) => o.id === id))
      filter.sectionId = id;
  },
});

// ========================================
// Static Options
// ========================================
const statusOptions: OptionItem[] = [
  { id: "Active", label: "ทำงานอยู่" },
  { id: "Resigned", label: "ลาออก" },
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
    await notify.error(extractErrorMessage(err));
    console.error("Save error:", err);
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

// ========================================
// Watchers
// ========================================
watch(
  () => [
    filter.status,
    filter.employeeTypeId,
    organizationUnitId.value,
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
</script>
