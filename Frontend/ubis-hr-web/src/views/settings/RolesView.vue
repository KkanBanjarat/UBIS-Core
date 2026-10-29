
<template>
  <div class="p-4 sm:p-6 space-y-6 mx-auto max-w-[1600px]">

    <!-- Page Header -->
    <div class="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-3">
      <div>
        <div class="flex items-center gap-2.5">
          <div class="size-9 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
            <i class="bi bi-shield-lock-fill text-lg"></i>
          </div>

          <div>
            <h1 class="text-xl font-bold text-base-content">
              Role และสิทธิ์
            </h1>
            <p class="text-sm text-base-content/50 mt-0.5">
              กำหนดเมนูและระดับสิทธิ์การเข้าถึงของแต่ละ Role
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Error -->
    <div v-if="errorMessage" class="alert alert-error rounded-2xl text-sm shadow-sm">
      <i class="bi bi-exclamation-circle"></i>
      <span>{{ errorMessage }}</span>
    </div>

    <!-- Loading -->
    <div v-if="isLoading" class="grid grid-cols-1 lg:grid-cols-[320px_minmax(0,1fr)] gap-5">
      <div class="bg-base-100 rounded-3xl p-5 border border-base-200">
        <div class="skeleton h-7 w-32 mb-5"></div>
        <div class="space-y-3">
          <div v-for="i in 5" :key="i" class="skeleton h-16 rounded-2xl"></div>
        </div>
      </div>

      <div class="bg-base-100 rounded-3xl p-6 border border-base-200">
        <div class="skeleton h-8 w-48 mb-3"></div>
        <div class="skeleton h-5 w-72 mb-7"></div>
        <div class="skeleton h-72 rounded-2xl"></div>
      </div>
    </div>

    <!-- Main -->
    <div v-else class="grid grid-cols-1 lg:grid-cols-[320px_minmax(0,1fr)] gap-5 items-start">

      <!-- ========================= -->
      <!-- LEFT : ROLE LIST -->
      <!-- ========================= -->
      <section class="bg-base-100 rounded-3xl border border-base-200 shadow-sm overflow-hidden lg:sticky lg:top-5">

        <!-- Role Header -->
        <div class="px-5 py-4 border-b border-base-200">
          <div class="flex items-center justify-between gap-3">
            <div>
              <div class="flex items-center gap-2">
                <i class="bi bi-people-fill text-primary"></i>
                <h2 class="font-bold text-sm">
                  Role ทั้งหมด
                </h2>
              </div>

              <p class="text-xs text-base-content/40 mt-1">
                {{ roles.length }} Role ในระบบ
              </p>
            </div>

            <button class="btn btn-primary btn-sm rounded-xl px-3 shadow-sm gap-1.5" title="เพิ่ม Role"
              @click="openCreateRole">
              <i class="bi bi-plus-lg"></i>
              <span class="hidden sm:inline">เพิ่ม Role</span>
            </button>
          </div>
        </div>

        <!-- Role List -->
        <div class="p-2.5">
          <div v-if="roles.length === 0" class="py-12 text-center">
            <div class="size-12 mx-auto rounded-2xl bg-base-200/60 flex items-center justify-center mb-3">
              <i class="bi bi-people text-xl text-base-content/30"></i>
            </div>

            <p class="text-sm font-medium text-base-content/60">
              ยังไม่มี Role
            </p>

            <p class="text-xs text-base-content/35 mt-1">
              กดเพิ่ม Role เพื่อเริ่มต้น
            </p>
          </div>

          <div v-else class="space-y-1">
            <button v-for="r in roles" :key="r.id" type="button"
              class="group w-full text-left rounded-2xl px-3.5 py-3 transition-all duration-200" :class="selectedRoleId === r.id
                  ? 'bg-primary/10 ring-1 ring-primary/15'
                  : 'hover:bg-base-200/50'
                " @click="selectRole(r.id)">
              <div class="flex items-center gap-3">

                <!-- Role Icon -->
                <div class="size-10 shrink-0 rounded-xl flex items-center justify-center transition-colors" :class="selectedRoleId === r.id
                    ? 'bg-primary text-primary-content shadow-sm'
                    : 'bg-base-200/70 text-base-content/50 group-hover:bg-base-200'
                  ">
                  <i class="bi bi-person-badge-fill"></i>
                </div>

                <!-- Name -->
                <div class="min-w-0 flex-1">
                  <div class="text-sm font-semibold truncate" :class="selectedRoleId === r.id
                      ? 'text-primary'
                      : 'text-base-content'
                    ">
                    {{ r.name }}
                  </div>

                  <div v-if="r.description" class="text-xs text-base-content/40 truncate mt-0.5">
                    {{ r.description }}
                  </div>

                  <div v-else class="text-xs text-base-content/30 mt-0.5">
                    ไม่มีคำอธิบาย
                  </div>
                </div>

                <!-- Actions -->
                <div class="flex items-center gap-0.5 shrink-0 opacity-0 group-hover:opacity-100 transition-opacity">
                  <button type="button" class="btn btn-ghost btn-xs btn-square rounded-lg text-warning"
                    title="แก้ไข Role" @click.stop="openEditRole(r)">
                    <i class="bi bi-pencil-square"></i>
                  </button>

                  <button type="button" class="btn btn-ghost btn-xs btn-square rounded-lg text-error" title="ลบ Role"
                    @click.stop="confirmDeleteRole(r)">
                    <i class="bi bi-trash3"></i>
                  </button>
                </div>

                <!-- Selected Indicator -->
                <i v-if="selectedRoleId === r.id" class="bi bi-chevron-right text-primary text-sm shrink-0"></i>

              </div>
            </button>
          </div>
        </div>
      </section>

      <!-- ========================= -->
      <!-- RIGHT : PERMISSION -->
      <!-- ========================= -->
      <section class="min-w-0">

        <!-- Selected Role Header -->
        <div class="bg-base-100 rounded-3xl border border-base-200 shadow-sm overflow-hidden">

          <div class="px-5 sm:px-6 py-5">
            <div v-if="selectedRole" class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">

              <div class="flex items-center gap-3.5 min-w-0">

                <div
                  class="size-12 shrink-0 rounded-2xl bg-primary text-primary-content flex items-center justify-center shadow-sm">
                  <i class="bi bi-shield-check text-xl"></i>
                </div>

                <div class="min-w-0">
                  <div class="flex items-center gap-2 flex-wrap">
                    <h2 class="text-lg font-bold truncate">
                      {{ selectedRole.name }}
                    </h2>

                    <span class="badge badge-success badge-sm gap-1 font-medium">
                      <span class="size-1.5 rounded-full bg-current"></span>
                      กำลังแก้ไข
                    </span>
                  </div>

                  <p v-if="selectedRole.description" class="text-sm text-base-content/50 truncate mt-0.5">
                    {{ selectedRole.description }}
                  </p>

                  <p v-else class="text-sm text-base-content/40 mt-0.5">
                    กำหนดสิทธิ์การเข้าถึงเมนูสำหรับ Role นี้
                  </p>
                </div>
              </div>
              <div class="flex items-center gap-2 text-xs text-base-content/40 shrink-0">
                <i class="bi bi-shield-lock"></i>
                สิทธิ์การเข้าถึงเมนู
              </div>
            </div>
            <!-- No Role -->
            <div v-else class="flex flex-col items-center justify-center text-center py-14">
              <div class="size-16 rounded-3xl bg-base-200/60 flex items-center justify-center mb-4">
                <i class="bi bi-shield-lock text-2xl text-base-content/25"></i>
              </div>

              <h2 class="font-bold text-base-content/70">
                เลือก Role เพื่อจัดการสิทธิ์
              </h2>

              <p class="text-sm text-base-content/40 mt-1">
                เลือก Role จากรายการด้านซ้าย
              </p>
            </div>
          </div>
                   <!-- Tabs + Panels -->
          <div v-if="selectedRole" class="border-t border-base-200">
            <div class="px-5 sm:px-6 pt-4">
              <div role="tablist" class="tabs tabs-box w-fit">
                <button role="tab" class="tab" :class="activeTab === 'menu' && 'tab-active'"
                  @click="activeTab = 'menu'">
                  เมนูและสิทธิ์การเข้าถึง
                </button>
                <button role="tab" class="tab" :class="activeTab === 'members' && 'tab-active'"
                  @click="activeTab = 'members'">
                  สมาชิก
                </button>
              </div>
            </div>
            <div class="p-4 sm:p-5">
              <RoleMenuPanel v-show="activeTab === 'menu'" ref="menuPanelRef" :role-id="selectedRoleId"
                :role-name="selectedRole?.name ?? null" />

              <RoleMembersPanel v-if="activeTab === 'members'" :role-id="selectedRoleId"
                :role-name="selectedRole?.name ?? null" />
            </div>
          </div>
        </div>
      </section>
    </div>

    <!-- ======================================== -->
    <!-- Modal : เพิ่ม / แก้ไข Role -->
    <!-- ======================================== -->
    <dialog ref="roleDialog" class="modal">
      <div class="modal-box max-w-md p-0 overflow-hidden rounded-3xl">

        <!-- Header -->
        <div class="px-6 py-5 border-b border-base-200 flex items-start gap-3">
          <div class="size-10 shrink-0 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
            <i :class="isCreateRole
                ? 'bi bi-person-plus-fill'
                : 'bi bi-pencil-square'
              "></i>
          </div>

          <div>
            <h3 class="font-bold text-lg">
              {{ isCreateRole ? "เพิ่ม Role" : "แก้ไข Role" }}
            </h3>

            <p class="text-xs text-base-content/40 mt-0.5">
              {{
                isCreateRole
                  ? "สร้าง Role ใหม่สำหรับกำหนดสิทธิ์"
                  : "แก้ไขข้อมูลของ Role นี้"
              }}
            </p>
          </div>
        </div>

        <!-- Form -->
        <form class="px-6 py-5 space-y-4" @submit.prevent="submitRole">
          <div>
            <label class="block text-sm font-medium mb-1.5" for="rf-name">
              ชื่อ Role
              <span class="text-error">*</span>
            </label>

            <input id="rf-name" v-model="roleForm.name" type="text"
              class="input input-bordered w-full rounded-xl focus:input-primary" placeholder="เช่น Admin, HR, Manager"
              autocomplete="off" />
          </div>

          <div>
            <label class="block text-sm font-medium mb-1.5" for="rf-desc">
              คำอธิบาย
            </label>

            <input id="rf-desc" v-model="roleForm.description" type="text"
              class="input input-bordered w-full rounded-xl focus:input-primary"
              placeholder="อธิบายหน้าที่หรือกลุ่มผู้ใช้งาน" />
          </div>

          <div v-if="roleFormError" class="alert alert-error rounded-xl text-sm py-3" role="alert">
            <i class="bi bi-exclamation-circle"></i>
            <span>{{ roleFormError }}</span>
          </div>

          <div class="flex justify-end gap-2 pt-2">
            <button type="button" class="btn btn-ghost btn-sm rounded-xl" @click="roleDialog?.close()">
              ยกเลิก
            </button>

            <button type="submit" class="btn btn-primary btn-sm rounded-xl px-5 gap-2" :disabled="isSavingRole">
              <span v-if="isSavingRole" class="loading loading-spinner loading-xs"></span>

              <i v-else class="bi bi-check-lg"></i>

              บันทึก
            </button>
          </div>
        </form>
      </div>

      <form method="dialog" class="modal-backdrop">
        <button>ปิด</button>
      </form>
    </dialog>

    <!-- ======================================== -->
    <!-- Modal : Permission -->
    <!-- ======================================== -->
    <dialog ref="permissionDialog" class="modal">
      <div class="modal-box max-w-md p-0 overflow-hidden rounded-3xl">

        <div class="px-6 py-5 border-b border-base-200 flex items-start gap-3">
          <div class="size-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
            <i class="bi bi-key-fill"></i>
          </div>

          <div>
            <h3 class="font-bold text-lg">
              เพิ่ม Permission
            </h3>

            <p class="text-xs text-base-content/40 mt-0.5">
              เพิ่ม Permission ใหม่เข้าสู่ระบบ
            </p>
          </div>
        </div>

        <form class="px-6 py-5 space-y-4" @submit.prevent="submitPermission">
          <div>
            <label class="block text-sm font-medium mb-1.5" for="pf-code">
              Code
              <span class="text-error">*</span>
            </label>

            <input id="pf-code" v-model="permissionForm.code" type="text" placeholder="เช่น employee.write"
              class="input input-bordered w-full rounded-xl font-mono focus:input-primary" autocomplete="off" />
          </div>

          <div>
            <label class="block text-sm font-medium mb-1.5" for="pf-desc">
              คำอธิบาย
            </label>

            <input id="pf-desc" v-model="permissionForm.description" type="text" placeholder="เช่น แก้ไขข้อมูลพนักงาน"
              class="input input-bordered w-full rounded-xl focus:input-primary" />
          </div>

          <div v-if="permissionFormError" class="alert alert-error rounded-xl text-sm py-3" role="alert">
            <i class="bi bi-exclamation-circle"></i>
            <span>{{ permissionFormError }}</span>
          </div>

          <div class="flex justify-end gap-2 pt-2">
            <button type="button" class="btn btn-ghost btn-sm rounded-xl" @click="permissionDialog?.close()">
              ยกเลิก
            </button>

            <button type="submit" class="btn btn-primary btn-sm rounded-xl px-5 gap-2" :disabled="isSavingPermission">
              <span v-if="isSavingPermission" class="loading loading-spinner loading-xs"></span>

              <i v-else class="bi bi-check-lg"></i>

              บันทึก
            </button>
          </div>
        </form>
      </div>

      <form method="dialog" class="modal-backdrop">
        <button>ปิด</button>
      </form>
    </dialog>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import { storeToRefs } from "pinia";
import { useRoleStore } from "../../stores/roleStore";
import RoleMenuPanel from "../../components/role/RoleMenuPanel.vue";
import RoleMembersPanel from "../../components/role/RoleMembersPanel.vue";
import type {
  RoleItem,
  PermissionItem,
  RoleFormPayload,
  PermissionFormPayload,
} from "../../types/Role";
import { notify, extractErrorMessage } from "../../utils/notify";
const activeTab = ref<"menu" | "members" | "permission">("menu");
const roleStore = useRoleStore();

const {
  roles,
  permissions,
  isLoading,
  errorMessage,
  grantMap,
} = storeToRefs(roleStore);

const menuPanelRef =
  ref<InstanceType<typeof RoleMenuPanel>>();

const selectedRoleId =
  ref<string | null>(null);

const selectedRole = computed<RoleItem | null>(
  () =>
    roles.value.find(
      (r) => r.id === selectedRoleId.value,
    ) ?? null,
);

function toggleKey(permissionId: string) {
  return `${selectedRoleId.value}:${permissionId}`;
}

function isGranted(permissionId: string) {
  return !!grantMap.value[
    toggleKey(permissionId)
  ];
}

const pendingKey =
  ref<string | null>(null);

async function onToggle(permissionId: string) {
  if (!selectedRoleId.value) return;

  const key = toggleKey(permissionId);
  const existingId = grantMap.value[key];

  pendingKey.value = key;

  try {
    if (existingId) {
      await roleStore.revoke(existingId);
    } else {
      await roleStore.grant(
        selectedRoleId.value,
        permissionId,
      );
    }
  } catch (err: any) {
    await notify.error(
      extractErrorMessage(err),
      "เปลี่ยนสิทธิ์ไม่สำเร็จ",
    );
  } finally {
    pendingKey.value = null;
  }
}

async function selectRole(id: string) {
  if (id === selectedRoleId.value) return;

  if (menuPanelRef.value?.isDirty) {
    const ok = await notify.confirm(
      "มีการเปลี่ยนแปลงสิทธิ์เมนูที่ยังไม่บันทึก ต้องการทิ้งการเปลี่ยนแปลงใช่หรือไม่",
      "ยังไม่ได้บันทึก",
    );

    if (!ok) return;
  }

  selectedRoleId.value = id;
}

// ========================================
// Role Modal
// ========================================

const roleDialog =
  ref<HTMLDialogElement>();

const isCreateRole = ref(true);

const editingRole =
  ref<RoleItem | null>(null);

const isSavingRole =
  ref(false);

const roleFormError =
  ref("");

const roleForm =
  reactive<RoleFormPayload>({
    name: "",
    description: "",
  });

function openCreateRole() {
  isCreateRole.value = true;
  editingRole.value = null;

  Object.assign(roleForm, {
    name: "",
    description: "",
  });

  roleFormError.value = "";

  roleDialog.value?.showModal();
}

function openEditRole(r: RoleItem) {
  isCreateRole.value = false;
  editingRole.value = r;

  Object.assign(roleForm, {
    name: r.name,
    description: r.description ?? "",
  });

  roleFormError.value = "";

  roleDialog.value?.showModal();
}

async function submitRole() {
  roleFormError.value = "";

  if (!roleForm.name.trim()) {
    roleFormError.value =
      "กรุณากรอกชื่อ Role";

    return;
  }

  isSavingRole.value = true;

  try {
    const payload: RoleFormPayload = {
      name: roleForm.name.trim(),
      description:
        roleForm.description?.trim() || null,
    };

    if (isCreateRole.value) {
      await roleStore.createRole(payload);
    } else {
      await roleStore.updateRole(
        editingRole.value!.id,
        payload,
      );
    }

    roleDialog.value?.close();

    await notify.success(
      isCreateRole.value
        ? "เพิ่ม Role สำเร็จ"
        : "บันทึกสำเร็จ",
    );
  } catch (err: any) {
    roleFormError.value =
      extractErrorMessage(err);
  } finally {
    isSavingRole.value = false;
  }
}

async function confirmDeleteRole(
  r: RoleItem,
) {
  const ok = await notify.confirm(
    `ยืนยันลบ Role "${r.name}" ใช่หรือไม่`,
    "ยืนยันการลบ",
  );

  if (!ok) return;

  try {
    await roleStore.deleteRole(r.id);

    if (selectedRoleId.value === r.id) {
      selectedRoleId.value = null;
    }

    await notify.success(
      "ลบ Role สำเร็จ",
    );
  } catch (err: any) {
    await notify.error(
      extractErrorMessage(err),
      "ลบไม่สำเร็จ",
    );
  }
}

// ========================================
// Permission Modal
// ========================================

const permissionDialog =
  ref<HTMLDialogElement>();

const isSavingPermission =
  ref(false);

const permissionFormError =
  ref("");

const permissionForm =
  reactive<PermissionFormPayload>({
    code: "",
    description: "",
  });

function openCreatePermission() {
  Object.assign(permissionForm, {
    code: "",
    description: "",
  });

  permissionFormError.value = "";

  permissionDialog.value?.showModal();
}

async function submitPermission() {
  permissionFormError.value = "";

  const code =
    permissionForm.code.trim();

  if (!code) {
    permissionFormError.value =
      "กรุณากรอก Code";

    return;
  }

  if (!/^[a-z0-9_.]+$/i.test(code)) {
    permissionFormError.value =
      "Code ควรเป็นตัวอักษรอังกฤษ ตัวเลข จุด หรือขีดล่างเท่านั้น (ตรวจสอบให้ตรงกับโค้ดฝั่ง Backend)";

    return;
  }

  isSavingPermission.value = true;

  try {
    await roleStore.createPermission({
      code,
      description:
        permissionForm.description?.trim() ||
        null,
    });

    permissionDialog.value?.close();

    await notify.success(
      "เพิ่ม Permission สำเร็จ",
    );
  } catch (err: any) {
    permissionFormError.value =
      extractErrorMessage(err);
  } finally {
    isSavingPermission.value = false;
  }
}

async function confirmDeletePermission(
  p: PermissionItem,
) {
  const ok = await notify.confirm(
    `ยืนยันลบ Permission "${p.code}" ใช่หรือไม่ (Role ที่มีสิทธิ์นี้จะไม่มีสิทธิ์นี้อีกต่อไป)`,
    "ยืนยันการลบ",
  );

  if (!ok) return;

  try {
    await roleStore.deletePermission(
      p.id,
    );

    await notify.success(
      "ลบ Permission สำเร็จ",
    );
  } catch (err: any) {
    await notify.error(
      extractErrorMessage(err),
      "ลบไม่สำเร็จ",
    );
  }
}

onMounted(async () => {
  await roleStore.fetchRoles();

  if (roles.value.length > 0) {
    selectedRoleId.value =
      roles.value[0].id;
  }
});
</script>
```