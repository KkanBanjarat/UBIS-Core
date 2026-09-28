<template>
  <div class="p-4 sm:p-6 space-y-5 mx-auto">
    <!-- Header -->
    <div>
      <h1 class="text-xl font-semibold text-base-content">ผู้ใช้งานระบบ</h1>
      <p class="text-sm text-base-content/50 mt-0.5">
        {{ totalCount }} รายการ
      </p>
    </div>

    <!-- ผล Sync -->
    <div
      v-if="syncResult"
      class="alert alert-success text-sm flex items-start justify-between gap-3"
      role="status"
    >
      <span>
        Sync เสร็จแล้ว: ทั้งหมด {{ syncResult.total }} คน, เพิ่มใหม่
        {{ syncResult.created }}, อัปเดต {{ syncResult.updated }}, ผูกกับผู้ใช้เดิม
        {{ syncResult.linked }}, ไม่เปลี่ยน {{ syncResult.unchanged }}, ข้าม
        {{ syncResult.skipped }}
      </span>
      <button
        class="btn btn-ghost btn-xs btn-square"
        aria-label="ปิดข้อความ"
        @click="syncResult = null"
      >
        <i class="bi bi-x-lg"></i>
      </button>
    </div>

    <!-- Filter card -->
    <div class="bg-base-100 rounded-2xl shadow-sm border border-base-200 p-5">
      <div class="flex flex-col lg:flex-row gap-3 lg:items-center">
        <div class="relative flex-1 min-w-[240px]">
          <i
            class="bi bi-search absolute left-3.5 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none z-10"
          ></i>
          <input
            v-model="filter.search"
            @input="onSearchInput"
            type="text"
            placeholder="ค้นหาชื่อ, อีเมล, รหัสพนักงาน..."
            class="input input-bordered w-full pl-10 text-sm focus:outline-none focus:border-primary transition-colors"
          />
        </div>
        <div class="flex flex-wrap gap-2 items-center">
          <div class="w-full sm:w-44">
            <FormSelect
              v-model="filter.status"
              :options="statusOptions"
              placeholder="สถานะ: ทั้งหมด"
            />
          </div>
          <div class="w-full sm:w-52">
            <FormSelect
              v-model="filter.source"
              :options="sourceOptions"
              placeholder="ที่มา: ทั้งหมด"
            />
          </div>
          <button
            class="btn btn-ghost btn-sm text-base-content/55 hover:text-error"
            :disabled="!hasActiveFilters"
            title="ล้างตัวกรอง"
            @click="clearFilters"
          >
            <i class="bi bi-x-lg"></i>
          </button>
        </div>
      </div>
    </div>

    <!-- Table card -->
    <div
      class="bg-base-100 rounded-2xl shadow-sm border border-base-200 overflow-hidden"
    >
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
        <div class="flex gap-2">
          <button
            class="btn btn-outline btn-sm gap-1.5"
            :disabled="isSyncing"
            @click="onSync"
          >
            <span v-if="isSyncing" class="loading loading-spinner loading-xs"></span>
            <i v-else class="bi bi-microsoft"></i>
            <span class="hidden sm:inline">Sync จาก Microsoft</span>
          </button>
          <button class="btn btn-primary btn-sm gap-1.5" @click="openCreate">
            <i class="bi bi-plus-lg"></i>
            เพิ่มผู้ใช้
          </button>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="isLoading" class="p-5 space-y-3">
        <div v-for="i in 6" :key="i" class="skeleton h-12 w-full rounded-lg"></div>
      </div>

      <!-- Error -->
      <div
        v-else-if="errorMessage"
        class="flex flex-col items-center gap-2 p-14 text-center"
      >
        <i class="bi bi-exclamation-circle text-3xl text-error/70"></i>
        <p class="text-error text-sm">{{ errorMessage }}</p>
        <button class="btn btn-sm btn-outline" @click="fetchUsers">
          ลองใหม่
        </button>
      </div>

      <template v-else>
        <!-- Empty -->
        <div
          v-if="users.length === 0"
          class="flex flex-col items-center gap-2 p-14 text-center"
        >
          <i class="bi bi-people text-3xl text-base-content/25"></i>
          <p class="text-base-content/50 text-sm">
            {{
              hasActiveFilters
                ? "ไม่พบผู้ใช้ตามเงื่อนไขที่เลือก"
                : "ยังไม่มีผู้ใช้ กด Sync จาก Microsoft หรือเพิ่มผู้ใช้ใหม่"
            }}
          </p>
        </div>

        <template v-else>
          <div class="overflow-x-auto">
            <table class="table min-w-[860px]">
              <thead>
                <tr
                  class="text-xs tracking-wide text-base-content/45 border-b border-base-200"
                >
                  <th class="bg-base-100">ผู้ใช้งาน</th>
                  <th class="bg-base-100">รหัสพนักงาน</th>
                  <th class="bg-base-100">ที่มา</th>
                  <th class="bg-base-100">เข้าใช้ล่าสุด</th>
                  <th class="bg-base-100 text-center">ใช้งาน</th>
                  <th class="bg-base-100 text-right">จัดการ</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="(u, idx) in users"
                  :key="u.id"
                  class="hover:bg-base-200/40 transition-colors border-b border-base-200/60 last:border-0"
                >
                  <td>
                    <div class="flex items-center gap-2.5">
                      <div class="avatar placeholder shrink-0">
                        <div
                          class="rounded-full w-9 h-9 flex items-center justify-center"
                          :class="avatarColor(idx)"
                        >
                          <span class="text-sm font-semibold leading-none">
                            {{ initials(u.displayName) }}
                          </span>
                        </div>
                      </div>
                      <div class="min-w-0">
                        <div class="font-semibold text-base-content/80 truncate">
                          {{ u.displayName }}
                        </div>
                        <div class="text-sm text-base-content/45 truncate">
                          {{ u.email }}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td class="text-base-content/70">
                    {{ u.employeeCode || "-" }}
                  </td>
                  <td>
                    <span
                      class="badge badge-sm font-normal whitespace-nowrap"
                      :class="u.isEntra ? 'badge-info badge-soft' : 'badge-ghost'"
                    >
                      {{ u.isEntra ? "Microsoft" : "สร้างในระบบ" }}
                    </span>
                  </td>
                  <td class="text-sm text-base-content/55 whitespace-nowrap">
                    {{ formatDate(u.lastLoginAt) }}
                  </td>
                  <td class="text-center">
                    <input
                      type="checkbox"
                      class="toggle toggle-success toggle-sm"
                      :checked="u.isActive"
                      :aria-label="`เปิด/ปิดการใช้งาน ${u.displayName}`"
                      @change="toggleActive(u)"
                    />
                  </td>
                  <td>
                    <div class="flex items-center justify-end gap-1">
                      <button
                        class="btn btn-ghost btn-xs btn-square text-warning/70 hover:text-warning hover:bg-warning/10"
                        title="แก้ไข"
                        @click="openEdit(u)"
                      >
                        <i class="bi bi-pencil-square"></i>
                      </button>
                      <button
                        v-if="!u.isEntra"
                        class="btn btn-ghost btn-xs btn-square text-info/70 hover:text-info hover:bg-info/10"
                        title="ตั้งรหัสผ่านใหม่"
                        @click="openResetPassword(u)"
                      >
                        <i class="bi bi-key"></i>
                      </button>
                      <button
                        class="btn btn-ghost btn-xs btn-square text-error/70 hover:text-error hover:bg-error/10"
                        title="ลบ"
                        @click="confirmDelete(u)"
                      >
                        <i class="bi bi-trash3"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          <div
            class="flex flex-col sm:flex-row items-center justify-between gap-3 px-5 py-4 border-t border-base-200"
          >
            <p class="text-xs text-base-content/45">
              แสดง {{ (filter.page - 1) * filter.pageSize + 1 }}–{{
                Math.min(filter.page * filter.pageSize, totalCount)
              }}
              จาก {{ totalCount }} รายการ
            </p>
            <div class="join">
              <button
                class="join-item btn btn-sm btn-ghost"
                :disabled="filter.page === 1"
                @click="goToPage(1)"
              >
                <i class="bi bi-chevron-double-left"></i>
              </button>
              <button
                class="join-item btn btn-sm btn-ghost"
                :disabled="filter.page === 1"
                @click="goToPage(filter.page - 1)"
              >
                <i class="bi bi-chevron-left"></i>
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
                <i class="bi bi-chevron-right"></i>
              </button>
              <button
                class="join-item btn btn-sm btn-ghost"
                :disabled="filter.page >= totalPages"
                @click="goToPage(totalPages)"
              >
                <i class="bi bi-chevron-double-right"></i>
              </button>
            </div>
          </div>
        </template>
      </template>
    </div>

    <!-- Modal: เพิ่ม / แก้ไข -->
    <dialog ref="formDialog" class="modal">
      <div class="modal-box">
        <h3 class="font-semibold text-lg mb-4">
          {{ isCreate ? "เพิ่มผู้ใช้ (ไม่มี Microsoft)" : "แก้ไขผู้ใช้งาน" }}
        </h3>

        <form class="space-y-3" @submit.prevent="submitForm">
          <div v-if="isEntraEdit" class="alert alert-info alert-soft text-xs">
            ผู้ใช้นี้มาจาก Microsoft ชื่อและอีเมลแก้ที่นี่ไม่ได้ (แก้ได้เฉพาะรหัสพนักงานและสถานะ)
          </div>

          <div>
            <label class="block text-sm mb-1" for="uf-email">อีเมล</label>
            <input
              id="uf-email"
              v-model="form.email"
              type="email"
              class="input input-bordered w-full"
              :disabled="isEntraEdit"
            />
          </div>
          <div>
            <label class="block text-sm mb-1" for="uf-name">ชื่อแสดงผล</label>
            <input
              id="uf-name"
              v-model="form.displayName"
              type="text"
              class="input input-bordered w-full"
              :disabled="isEntraEdit"
            />
          </div>
          <div>
            <label class="block text-sm mb-1" for="uf-code">รหัสพนักงาน</label>
            <input
              id="uf-code"
              v-model="form.employeeCode"
              type="text"
              class="input input-bordered w-full"
            />
          </div>
          <div v-if="isCreate">
            <label class="block text-sm mb-1" for="uf-pw">
              รหัสผ่าน (อย่างน้อย 8 ตัวอักษร)
            </label>
            <input
              id="uf-pw"
              v-model="form.password"
              type="password"
              autocomplete="new-password"
              class="input input-bordered w-full"
            />
          </div>
          <label v-if="!isCreate" class="flex items-center gap-2 cursor-pointer">
            <input
              v-model="form.isActive"
              type="checkbox"
              class="toggle toggle-success toggle-sm"
            />
            <span class="text-sm">เปิดใช้งานบัญชี</span>
          </label>

          <p v-if="formError" class="text-error text-sm" role="alert">
            {{ formError }}
          </p>

          <div class="modal-action">
            <button type="button" class="btn btn-ghost btn-sm" @click="formDialog?.close()">
              ยกเลิก
            </button>
            <button type="submit" class="btn btn-primary btn-sm" :disabled="isSaving">
              <span v-if="isSaving" class="loading loading-spinner loading-xs"></span>
              บันทึก
            </button>
          </div>
        </form>
      </div>
      <form method="dialog" class="modal-backdrop"><button>ปิด</button></form>
    </dialog>

    <!-- Modal: ตั้งรหัสผ่านใหม่ -->
    <dialog ref="pwDialog" class="modal">
      <div class="modal-box">
        <h3 class="font-semibold text-lg mb-1">ตั้งรหัสผ่านใหม่</h3>
        <p class="text-sm text-base-content/50 mb-4">
          {{ pwTarget?.displayName }} ({{ pwTarget?.email }})
        </p>

        <form class="space-y-3" @submit.prevent="submitResetPassword">
          <div>
            <label class="block text-sm mb-1" for="pw-new">
              รหัสผ่านใหม่ (อย่างน้อย 8 ตัวอักษร)
            </label>
            <input
              id="pw-new"
              v-model="newPassword"
              type="password"
              autocomplete="new-password"
              class="input input-bordered w-full"
            />
          </div>
          <p v-if="pwError" class="text-error text-sm" role="alert">{{ pwError }}</p>
          <div class="modal-action">
            <button type="button" class="btn btn-ghost btn-sm" @click="pwDialog?.close()">
              ยกเลิก
            </button>
            <button type="submit" class="btn btn-primary btn-sm" :disabled="isSaving">
              <span v-if="isSaving" class="loading loading-spinner loading-xs"></span>
              บันทึก
            </button>
          </div>
        </form>
      </div>
      <form method="dialog" class="modal-backdrop"><button>ปิด</button></form>
    </dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { storeToRefs } from "pinia";
import { useUserStore } from "../../stores/userStore";
import type { OptionItem } from "../../stores/employeeStore.ts";
import type { UserItem, UserFilter, SyncResult } from "../../types/User";
import FormSelect from "../../components/ui/FormSelect.vue";
import { notify, extractErrorMessage } from "../../utils/notify";

const userStore = useUserStore();
const { users, totalCount, isLoading, isSyncing, errorMessage } =
  storeToRefs(userStore);

// ========================================
// Filter & Pagination (ฝั่ง Server เหมือนหน้ารายชื่อพนักงาน)
// ========================================
const defaultFilter: UserFilter = {
  search: "",
  status: null,
  source: null,
  page: 1,
  pageSize: 10,
};
const filter = reactive<UserFilter>({ ...defaultFilter });

const statusOptions: OptionItem[] = [
  { id: "active", label: "ใช้งานอยู่" },
  { id: "inactive", label: "ปิดการใช้งาน" },
];
const sourceOptions: OptionItem[] = [
  { id: "entra", label: "Microsoft" },
  { id: "local", label: "สร้างในระบบ" },
];

const totalPages = computed(
  () => Math.ceil(totalCount.value / filter.pageSize) || 1,
);
const hasActiveFilters = computed(
  () => !!filter.search || !!filter.status || !!filter.source,
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

function fetchUsers() {
  return userStore.fetchList(filter);
}
function resetPageAndFetch() {
  filter.page = 1;
  fetchUsers();
}
function goToPage(p: number) {
  filter.page = p;
  fetchUsers();
}
function clearFilters() {
  Object.assign(filter, defaultFilter);
  fetchUsers();
}

let debounceTimer: ReturnType<typeof setTimeout>;
function onSearchInput() {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(resetPageAndFetch, 300);
}

watch(
  () => [filter.status, filter.source, filter.pageSize],
  resetPageAndFetch,
);

// ========================================
// Display helpers
// ========================================
const avatarPalette = [
  "bg-emerald-100 text-emerald-700",
  "bg-teal-100 text-teal-700",
  "bg-green-100 text-green-700",
  "bg-lime-100 text-lime-800",
  "bg-cyan-100 text-cyan-700",
  "bg-emerald-200 text-emerald-800",
];
const avatarColor = (idx: number) => avatarPalette[idx % avatarPalette.length];
const initials = (name: string) => name?.charAt(0)?.toUpperCase() ?? "?";
const formatDate = (d?: string | null) =>
  d
    ? new Date(d).toLocaleString("th-TH", {
        dateStyle: "short",
        timeStyle: "short",
      })
    : "-";

// ========================================
// Sync จาก Microsoft (Entra)
// ========================================
const syncResult = ref<SyncResult | null>(null);

async function onSync() {
  syncResult.value = null;
  try {
    syncResult.value = await userStore.syncEntra();
    await fetchUsers();
  } catch (err: any) {
    await notify.error(extractErrorMessage(err), "Sync ไม่สำเร็จ");
  }
}

// ========================================
// เปิด/ปิดการใช้งาน
// ========================================
async function toggleActive(u: UserItem) {
  try {
    await userStore.update(u.id, {
      email: u.email,
      displayName: u.displayName,
      employeeCode: u.employeeCode ?? null,
      employeeId: u.employeeId ?? null,
      isActive: !u.isActive,
    });
  } catch (err: any) {
    await notify.error(extractErrorMessage(err), "เปลี่ยนสถานะไม่สำเร็จ");
  }
  // โหลดหน้าปัจจุบันใหม่เสมอ เพื่อให้ปุ่ม Toggle ตรงกับข้อมูลจริง
  await fetchUsers();
}

// ========================================
// Modal: เพิ่ม / แก้ไข
// ========================================
const formDialog = ref<HTMLDialogElement>();
const isCreate = ref(true);
const editing = ref<UserItem | null>(null);
const isSaving = ref(false);
const formError = ref("");
const form = reactive({
  email: "",
  displayName: "",
  employeeCode: "",
  password: "",
  isActive: true,
});

const isEntraEdit = computed(() => !isCreate.value && !!editing.value?.isEntra);

function openCreate() {
  isCreate.value = true;
  editing.value = null;
  Object.assign(form, {
    email: "",
    displayName: "",
    employeeCode: "",
    password: "",
    isActive: true,
  });
  formError.value = "";
  formDialog.value?.showModal();
}

function openEdit(u: UserItem) {
  isCreate.value = false;
  editing.value = u;
  Object.assign(form, {
    email: u.email,
    displayName: u.displayName,
    employeeCode: u.employeeCode ?? "",
    password: "",
    isActive: u.isActive,
  });
  formError.value = "";
  formDialog.value?.showModal();
}

async function submitForm() {
  formError.value = "";

  if (!isEntraEdit.value) {
    if (!form.email.trim() || !form.email.includes("@"))
      return (formError.value = "กรุณากรอกอีเมลให้ถูกต้อง");
    if (!form.displayName.trim())
      return (formError.value = "กรุณากรอกชื่อแสดงผล");
  }
  if (isCreate.value && form.password.length < 8)
    return (formError.value = "รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร");

  isSaving.value = true;
  try {
    if (isCreate.value) {
      await userStore.create({
        email: form.email.trim(),
        displayName: form.displayName.trim(),
        employeeCode: form.employeeCode.trim() || null,
        employeeId: null,
        password: form.password,
        isActive: true,
      });
    } else {
      await userStore.update(editing.value!.id, {
        email: form.email.trim(),
        displayName: form.displayName.trim(),
        employeeCode: form.employeeCode.trim() || null,
        employeeId: editing.value!.employeeId ?? null,
        isActive: form.isActive,
      });
    }
    formDialog.value?.close();
    if (isCreate.value) filter.page = 1;
    await fetchUsers();
    await notify.success(
      isCreate.value ? "เพิ่มผู้ใช้สำเร็จ" : "บันทึกการแก้ไขสำเร็จ",
    );
  } catch (err: any) {
    // แสดงในกล่องเอง เพราะ Popup อาจไปอยู่หลัง Modal
    formError.value = extractErrorMessage(err);
  } finally {
    isSaving.value = false;
  }
}

// ========================================
// Modal: ตั้งรหัสผ่านใหม่ (เฉพาะผู้ใช้ที่ไม่มี Microsoft)
// ========================================
const pwDialog = ref<HTMLDialogElement>();
const pwTarget = ref<UserItem | null>(null);
const newPassword = ref("");
const pwError = ref("");

function openResetPassword(u: UserItem) {
  pwTarget.value = u;
  newPassword.value = "";
  pwError.value = "";
  pwDialog.value?.showModal();
}

async function submitResetPassword() {
  pwError.value = "";
  if (newPassword.value.length < 8)
    return (pwError.value = "รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร");

  isSaving.value = true;
  try {
    await userStore.resetPassword(pwTarget.value!.id, newPassword.value);
    pwDialog.value?.close();
    await notify.success("ตั้งรหัสผ่านใหม่สำเร็จ");
  } catch (err: any) {
    pwError.value = extractErrorMessage(err);
  } finally {
    isSaving.value = false;
  }
}

// ========================================
// ลบ
// ========================================
async function confirmDelete(u: UserItem) {
  const ok = await notify.confirm(
    `ยืนยันลบผู้ใช้ "${u.displayName}" ใช่หรือไม่`,
    "ยืนยันการลบ",
  );
  if (!ok) return;

  try {
    await userStore.remove(u.id);
    // ลบแถวสุดท้ายของหน้า ให้ถอยไปหน้าก่อนหน้า
    if (users.value.length === 1 && filter.page > 1) filter.page -= 1;
    await fetchUsers();
    await notify.success("ลบผู้ใช้สำเร็จ");
  } catch (err: any) {
    await notify.error(extractErrorMessage(err), "ลบไม่สำเร็จ");
  }
}

onMounted(fetchUsers);
</script>