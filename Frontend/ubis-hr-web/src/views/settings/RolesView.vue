<template>
  <div class="p-4 sm:p-6 space-y-5 mx-auto">
    <div>
      <h1 class="text-xl font-semibold text-base-content">Role และ Permission</h1>
      <p class="text-sm text-base-content/50 mt-0.5">
        กำหนดสิทธิ์ที่แต่ละ Role ทำได้
      </p>
    </div>

    <div v-if="errorMessage" class="alert alert-error text-sm">
      {{ errorMessage }}
    </div>

    <div v-if="isLoading" class="grid grid-cols-1 lg:grid-cols-[280px_1fr] gap-5">
      <div class="skeleton h-64 rounded-2xl"></div>
      <div class="skeleton h-64 rounded-2xl"></div>
    </div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-[280px_1fr] gap-5">
      <!-- รายชื่อ Role -->
      <div class="bg-base-100 rounded-2xl shadow-sm border border-base-200 overflow-hidden">
        <div class="flex items-center justify-between px-4 py-3.5 border-b border-base-200">
          <span class="text-sm font-semibold">Role ทั้งหมด</span>
          <button class="btn btn-primary btn-xs gap-1" title="เพิ่ม Role" @click="openCreateRole">
            <i class="bi bi-plus-lg"></i>
          </button>
        </div>
        <ul>
          <li v-if="roles.length === 0" class="p-4 text-sm text-base-content/40 text-center">
            ยังไม่มี Role
          </li>
          <li
            v-for="r in roles"
            :key="r.id"
            class="flex items-center justify-between gap-2 px-4 py-3 border-b border-base-200/60 last:border-0 cursor-pointer hover:bg-base-200/40"
            :class="selectedRoleId === r.id && 'bg-primary/5'"
            @click="selectedRoleId = r.id"
          >
            <div class="min-w-0">
              <div class="text-sm font-semibold truncate" :class="selectedRoleId === r.id && 'text-primary'">
                {{ r.name }}
              </div>
              <div v-if="r.description" class="text-xs text-base-content/40 truncate">
                {{ r.description }}
              </div>
            </div>
            <div class="flex gap-1 shrink-0">
              <button
                class="btn btn-ghost btn-xs btn-square text-warning/70"
                title="แก้ไข"
                @click.stop="openEditRole(r)"
              >
                <i class="bi bi-pencil-square"></i>
              </button>
              <button
                class="btn btn-ghost btn-xs btn-square text-error/70"
                title="ลบ"
                @click.stop="confirmDeleteRole(r)"
              >
                <i class="bi bi-trash3"></i>
              </button>
            </div>
          </li>
        </ul>
      </div>

      <!-- Checkbox Permission ของ Role ที่เลือก -->
      <div class="space-y-5">
        <div class="bg-base-100 rounded-2xl shadow-sm border border-base-200 overflow-hidden">
          <div class="flex items-center justify-between px-5 py-3.5 border-b border-base-200">
            <span class="text-sm font-semibold">
              {{ selectedRole ? `สิทธิ์ของ "${selectedRole.name}"` : "เลือก Role ทางซ้ายก่อน" }}
            </span>
            <button
              class="btn btn-outline btn-xs gap-1"
              title="เพิ่ม Permission ใหม่เข้าระบบ"
              @click="openCreatePermission"
            >
              <i class="bi bi-plus-lg"></i>
              เพิ่ม Permission
            </button>
          </div>

          <div v-if="!selectedRole" class="p-10 text-center text-sm text-base-content/40">
            เลือก Role เพื่อกำหนดสิทธิ์
          </div>

          <div v-else-if="permissions.length === 0" class="p-10 text-center text-sm text-base-content/40">
            ยังไม่มี Permission ในระบบ กด "เพิ่ม Permission" ด้านบนเพื่อเริ่มสร้าง
          </div>

          <ul v-else class="divide-y divide-base-200/60">
            <li
              v-for="p in permissions"
              :key="p.id"
              class="flex items-center gap-3 px-5 py-3 hover:bg-base-200/30"
            >
              <input
                type="checkbox"
                class="checkbox checkbox-primary checkbox-sm"
                :checked="isGranted(p.id)"
                :disabled="!!pendingKey && pendingKey === toggleKey(p.id)"
                @change="onToggle(p.id)"
              />
              <div class="min-w-0 flex-1">
                <div class="text-sm font-mono">{{ p.code }}</div>
                <div v-if="p.description" class="text-xs text-base-content/40">
                  {{ p.description }}
                </div>
              </div>
              <button
                class="btn btn-ghost btn-xs btn-square text-error/60 shrink-0"
                title="ลบ Permission นี้ออกจากระบบ"
                @click="confirmDeletePermission(p)"
              >
                <i class="bi bi-trash3"></i>
              </button>
            </li>
          </ul>
        </div>

        <p class="text-xs text-base-content/40 px-1">
          <i class="bi bi-info-circle"></i>
          Code ของ Permission ต้องตรงกับที่ผูกไว้ในโค้ดฝั่ง Backend เป๊ะ (เช่น
          <code class="font-mono">employee.write</code>) เพิ่มชื่อที่ไม่มีโค้ดใดเช็คใช้จะไม่มีผลอะไร
        </p>
      </div>
    </div>

    <!-- Modal: เพิ่ม/แก้ไข Role -->
    <dialog ref="roleDialog" class="modal">
      <div class="modal-box max-w-md">
        <h3 class="font-semibold text-lg mb-4">
          {{ isCreateRole ? "เพิ่ม Role" : "แก้ไข Role" }}
        </h3>
        <form class="space-y-3" @submit.prevent="submitRole">
          <div>
            <label class="block text-sm mb-1" for="rf-name">ชื่อ Role</label>
            <input id="rf-name" v-model="roleForm.name" type="text" class="input input-bordered w-full" />
          </div>
          <div>
            <label class="block text-sm mb-1" for="rf-desc">คำอธิบาย</label>
            <input id="rf-desc" v-model="roleForm.description" type="text" class="input input-bordered w-full" />
          </div>
          <p v-if="roleFormError" class="text-error text-sm" role="alert">{{ roleFormError }}</p>
          <div class="modal-action">
            <button type="button" class="btn btn-ghost btn-sm" @click="roleDialog?.close()">ยกเลิก</button>
            <button type="submit" class="btn btn-primary btn-sm" :disabled="isSavingRole">
              <span v-if="isSavingRole" class="loading loading-spinner loading-xs"></span>
              บันทึก
            </button>
          </div>
        </form>
      </div>
      <form method="dialog" class="modal-backdrop"><button>ปิด</button></form>
    </dialog>

    <!-- Modal: เพิ่ม Permission ใหม่ -->
    <dialog ref="permissionDialog" class="modal">
      <div class="modal-box max-w-md">
        <h3 class="font-semibold text-lg mb-1">เพิ่ม Permission</h3>
        <p class="text-xs text-base-content/40 mb-4">
          Code ต้องตรงกับที่ผูกไว้ในโค้ดฝั่ง Backend เช่น
          <code class="font-mono">employee.write</code>
        </p>
        <form class="space-y-3" @submit.prevent="submitPermission">
          <div>
            <label class="block text-sm mb-1" for="pf-code">Code</label>
            <input
              id="pf-code"
              v-model="permissionForm.code"
              type="text"
              placeholder="เช่น employee.write"
              class="input input-bordered w-full font-mono"
              autocomplete="off"
            />
          </div>
          <div>
            <label class="block text-sm mb-1" for="pf-desc">คำอธิบาย</label>
            <input
              id="pf-desc"
              v-model="permissionForm.description"
              type="text"
              placeholder="เช่น แก้ไขข้อมูลพนักงาน"
              class="input input-bordered w-full"
            />
          </div>
          <p v-if="permissionFormError" class="text-error text-sm" role="alert">
            {{ permissionFormError }}
          </p>
          <div class="modal-action">
            <button type="button" class="btn btn-ghost btn-sm" @click="permissionDialog?.close()">
              ยกเลิก
            </button>
            <button type="submit" class="btn btn-primary btn-sm" :disabled="isSavingPermission">
              <span v-if="isSavingPermission" class="loading loading-spinner loading-xs"></span>
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
import { ref, reactive, computed, onMounted } from "vue";
import { storeToRefs } from "pinia";
import { useRoleStore } from "../../stores/roleStore";
import type {
  RoleItem,
  PermissionItem,
  RoleFormPayload,
  PermissionFormPayload,
} from "../../types/Role";
import { notify, extractErrorMessage } from "../../utils/notify";

const roleStore = useRoleStore();
const { roles, permissions, isLoading, errorMessage, grantMap } = storeToRefs(roleStore);

const selectedRoleId = ref<string | null>(null);
const selectedRole = computed<RoleItem | null>(
  () => roles.value.find((r) => r.id === selectedRoleId.value) ?? null,
);

function toggleKey(permissionId: string) {
  return `${selectedRoleId.value}:${permissionId}`;
}
function isGranted(permissionId: string) {
  return !!grantMap.value[toggleKey(permissionId)];
}

// กันกดรัวจนยิง Request ซ้อน (ติ๊กแล้ว Disable เฉพาะแถวนั้นระหว่างรอผล)
const pendingKey = ref<string | null>(null);

async function onToggle(permissionId: string) {
  if (!selectedRoleId.value) return;
  const key = toggleKey(permissionId);
  const existingId = grantMap.value[key];

  pendingKey.value = key;
  try {
    if (existingId) {
      await roleStore.revoke(existingId);
    } else {
      await roleStore.grant(selectedRoleId.value, permissionId);
    }
  } catch (err: any) {
    await notify.error(extractErrorMessage(err), "เปลี่ยนสิทธิ์ไม่สำเร็จ");
  } finally {
    pendingKey.value = null;
  }
}

// ========================================
// Modal: เพิ่ม/แก้ไข Role
// ========================================
const roleDialog = ref<HTMLDialogElement>();
const isCreateRole = ref(true);
const editingRole = ref<RoleItem | null>(null);
const isSavingRole = ref(false);
const roleFormError = ref("");
const roleForm = reactive<RoleFormPayload>({ name: "", description: "" });

function openCreateRole() {
  isCreateRole.value = true;
  editingRole.value = null;
  Object.assign(roleForm, { name: "", description: "" });
  roleFormError.value = "";
  roleDialog.value?.showModal();
}
function openEditRole(r: RoleItem) {
  isCreateRole.value = false;
  editingRole.value = r;
  Object.assign(roleForm, { name: r.name, description: r.description ?? "" });
  roleFormError.value = "";
  roleDialog.value?.showModal();
}

async function submitRole() {
  roleFormError.value = "";
  if (!roleForm.name.trim()) {
    roleFormError.value = "กรุณากรอกชื่อ Role";
    return;
  }
  isSavingRole.value = true;
  try {
    const payload: RoleFormPayload = {
      name: roleForm.name.trim(),
      description: roleForm.description?.trim() || null,
    };
    if (isCreateRole.value) await roleStore.createRole(payload);
    else await roleStore.updateRole(editingRole.value!.id, payload);

    roleDialog.value?.close();
    await notify.success(isCreateRole.value ? "เพิ่ม Role สำเร็จ" : "บันทึกสำเร็จ");
  } catch (err: any) {
    roleFormError.value = extractErrorMessage(err);
  } finally {
    isSavingRole.value = false;
  }
}

async function confirmDeleteRole(r: RoleItem) {
  const ok = await notify.confirm(`ยืนยันลบ Role "${r.name}" ใช่หรือไม่`, "ยืนยันการลบ");
  if (!ok) return;
  try {
    await roleStore.deleteRole(r.id);
    if (selectedRoleId.value === r.id) selectedRoleId.value = null;
    await notify.success("ลบ Role สำเร็จ");
  } catch (err: any) {
    await notify.error(extractErrorMessage(err), "ลบไม่สำเร็จ");
  }
}

// ========================================
// Modal: เพิ่ม Permission ใหม่เข้าระบบ
// ========================================
const permissionDialog = ref<HTMLDialogElement>();
const isSavingPermission = ref(false);
const permissionFormError = ref("");
const permissionForm = reactive<PermissionFormPayload>({ code: "", description: "" });

function openCreatePermission() {
  Object.assign(permissionForm, { code: "", description: "" });
  permissionFormError.value = "";
  permissionDialog.value?.showModal();
}

async function submitPermission() {
  permissionFormError.value = "";
  const code = permissionForm.code.trim();
  if (!code) {
    permissionFormError.value = "กรุณากรอก Code";
    return;
  }
  // เตือนรูปแบบเบื้องต้น (a-z, 0-9, จุด, ขีดล่าง) — ไม่บล็อก เผื่อมีรูปแบบอื่นที่ใช้จริงอยู่
  if (!/^[a-z0-9_.]+$/i.test(code)) {
    permissionFormError.value =
      "Code ควรเป็นตัวอักษรอังกฤษ ตัวเลข จุด หรือขีดล่างเท่านั้น (ตรวจสอบให้ตรงกับโค้ดฝั่ง Backend)";
    return;
  }

  isSavingPermission.value = true;
  try {
    await roleStore.createPermission({
      code,
      description: permissionForm.description?.trim() || null,
    });
    permissionDialog.value?.close();
    await notify.success("เพิ่ม Permission สำเร็จ");
  } catch (err: any) {
    permissionFormError.value = extractErrorMessage(err);
  } finally {
    isSavingPermission.value = false;
  }
}

async function confirmDeletePermission(p: PermissionItem) {
  const ok = await notify.confirm(
    `ยืนยันลบ Permission "${p.code}" ใช่หรือไม่ (Role ที่มีสิทธิ์นี้จะไม่มีสิทธิ์นี้อีกต่อไป)`,
    "ยืนยันการลบ",
  );
  if (!ok) return;
  try {
    await roleStore.deletePermission(p.id);
    await notify.success("ลบ Permission สำเร็จ");
  } catch (err: any) {
    await notify.error(extractErrorMessage(err), "ลบไม่สำเร็จ");
  }
}

onMounted(async () => {
  await roleStore.fetchAll();
  if (roles.value.length > 0) selectedRoleId.value = roles.value[0].id;
});
</script>