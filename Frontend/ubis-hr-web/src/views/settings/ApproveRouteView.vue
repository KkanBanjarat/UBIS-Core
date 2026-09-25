<template>
  <div class="mx-auto space-y-5 p-4 sm:p-6">
    <PageHeader
      title="สายอนุมัติเอกสาร"
      description="กำหนดว่าเอกสารแต่ละประเภทต้องผ่านการอนุมัติจากใครบ้าง"
    >
      <template #actions>
        <button class="btn btn-primary btn-sm gap-1.5 text-sm" @click="openCreateModal()">
          <Plus class="size-4" />
          เพิ่มขั้นตอน
        </button>
      </template>
    </PageHeader>

    <!-- Loading -->
    <div v-if="isLoading" class="space-y-4">
      <div
        v-for="i in 2"
        :key="i"
        class="rounded-2xl border border-base-200 bg-base-100 p-5"
      >
        <div class="skeleton mb-4 h-6 w-40"></div>
        <div v-for="j in 3" :key="j" class="skeleton mb-2 h-14 w-full rounded-xl"></div>
      </div>
    </div>
    <!-- Error -->
    <div v-else-if="errorMessage"
      class="flex flex-col items-center gap-2 rounded-2xl border border-base-200 bg-base-100 p-14 text-center">
      <CircleAlert class="size-8 text-error/70" />
      <p class="text-sm text-error">
        {{ errorMessage }}
      </p>
    </div>
    <!-- Empty -->
    <div v-else-if="routes.length === 0"
      class="flex flex-col items-center gap-2 rounded-2xl border border-base-200 bg-base-100 p-14 text-center">
      <GitBranch class="size-8 text-base-content/25" />
      <p class="text-sm text-base-content/50">ยังไม่ได้ตั้งค่าสายอนุมัติ</p>
      <button class="btn btn-primary btn-sm mt-2 gap-1.5 text-sm"
        @click="openCreateModal()">
        <Plus class="size-4" />
        เพิ่มขั้นตอนแรก
      </button>
    </div>
    <!-- Content: จัดกลุ่มตามประเภทเอกสาร -->
    <div v-else class="space-y-5">
      <div v-for="[docType, steps] in routesByDocType"
        :key="docType"
        class="overflow-hidden rounded-2xl border border-base-200 bg-base-100 shadow-sm">
        <!-- Doc Type Header -->
        <div class="flex items-center justify-between gap-3 border-b border-base-200 bg-base-200/40 px-5 py-4">
          <div class="flex items-center gap-3">
            <div class="flex size-10 items-center justify-center rounded-lg bg-primary text-primary-content shadow-sm">
              <FileText class="size-[18px]" />
            </div>
            <div>
              <h3 class="text-base font-semibold text-base-content">
                {{ docTypeLabel(docType) }}
              </h3>
              <p class="mt-0.5 text-sm text-base-content/50">
                {{ steps.length }} ขั้นตอน
              </p>
            </div>
          </div>
          <button class="btn btn-ghost btn-sm gap-1.5 border border-base-300 text-sm"
            @click="openCreateModal(docType)">
            <Plus class="size-4" />
            เพิ่มขั้นตอน
          </button>
        </div>
        <!-- Steps -->
        <div class="p-5">
          <div class="relative">
            <!-- Timeline Line -->
            <div class="absolute bottom-5 left-5 top-5 w-0.5 bg-base-300"></div>
            <div v-for="(step, index) in steps"
              :key="step.id"
              class="relative flex gap-4">
              <!-- Step Number -->
              <div class="relative z-10 flex size-10 shrink-0 items-center justify-center rounded-full border-4 border-base-100 text-sm font-bold shadow-md"
                :class="
                  step.isActive
                    ? 'bg-primary text-primary-content'
                    : 'bg-base-300 text-base-content/40'
                ">
                {{ step.stepNo }}
              </div>
              <!-- Step Content -->
              <div class="group mb-4 flex-1 rounded-xl border px-4 py-3.5 shadow-sm transition-all hover:shadow-md"
                :class="[
                  step.isActive
                    ? 'border-base-200 bg-base-100'
                    : 'border-dashed border-base-300 bg-base-200/30',
                  index === steps.length - 1 ? 'mb-0' : '',
                ]">
                <div class="flex items-start justify-between gap-3">
                  <div class="min-w-0">
                    <!-- Step Name -->
                    <div class="flex flex-wrap items-center gap-2">
                      <p class="text-base font-semibold"
                        :class="!step.isActive && 'text-base-content/45'">
                        {{ step.stepName }}
                      </p>
                      <span v-if="!step.isActive" class="badge badge-ghost badge-sm">
                        ปิดใช้งาน
                      </span>
                    </div>
                    <!-- Approver -->
                    <div class="mt-1.5 flex items-center gap-1.5">
                      <component
                        :is="approverTypeIcon(step.approverType)"
                        class="size-4 shrink-0 text-base-content/40"
                      />

                      <span class="text-sm text-base-content/60">
                        {{ approverDescription(step) }}
                      </span>
                    </div>
                  </div>

                  <!-- Actions -->
                  <div
                    class="flex shrink-0 items-center gap-0.5 opacity-0 transition-opacity group-hover:opacity-100"
                  >
                    <button
                      class="btn btn-ghost btn-sm btn-square text-warning/70 hover:bg-warning/10 hover:text-warning"
                      title="แก้ไข"
                      @click="openEditModal(step)"
                    >
                      <Edit class="size-4" />
                    </button>

                    <button
                      class="btn btn-ghost btn-sm btn-square text-error/70 hover:bg-error/10 hover:text-error"
                      title="ลบ"
                      @click="confirmDelete(step)"
                    >
                      <Trash2 class="size-4" />
                    </button>
                  </div>
                </div>

                <!-- Updated -->
                <p class="mt-2.5 text-xs text-base-content/40">
                  แก้ไขล่าสุด {{ formatDateTime(step.updatedAt) }} โดย
                  {{ step.updatedBy }}
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <ApproveRouteFormModal
    ref="formModalRef"
    :employee-options="allowedEmployees"
    @save="handleSave"
  />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { storeToRefs } from "pinia";
import { useRouteApproveStore } from "../../stores/routeApproveStore";
import hrApi from "../../services/hrApi";
import PageHeader from "../../components/ui/PageHeader.vue";
import ApproveRouteFormModal from "../../components/settings/ApproveRouteFormModal.vue";
import type { EmployeeOption } from "../../components/ui/EmployeeSelect.vue";
import type { RouteApprove } from "../../types/RouteApprove";
import { notify, extractErrorMessage } from "../../utils/notify";
import {
  Plus,
  Edit,
  Trash2,
  CircleAlert,
  GitBranch,
  FileText,
  Users,
  User,
  Building2,
} from "lucide-vue-next";

const routeStore = useRouteApproveStore();
const { routes, isLoading, errorMessage } = storeToRefs(routeStore);

const allowedEmployees = ref<EmployeeOption[]>([]);
const formModalRef = ref<InstanceType<typeof ApproveRouteFormModal>>();
const editingId = ref<string | null>(null);

const routesByDocType = computed(() => routeStore.routesByDocType);

// ========================================
// Display Helpers
// ========================================
function docTypeLabel(docType: string) {
  const map: Record<string, string> = {
    PrettyCash: "เบิกสวัสดิการ / เงินสดย่อย",
  };
  return map[docType] ?? docType;
}

function approverTypeIcon(type: string) {
  if (type === "ManagerChain") return Users;
  if (type === "FixedEmployee") return User;
  return Building2;
}

function approverDescription(step: RouteApprove) {
  switch (step.approverType) {
    case "ManagerChain":
      return `หัวหน้าตามสายบังคับบัญชา ระดับ L${step.minPositionLevel} ขึ้นไป`;
    case "FixedEmployee":
      return step.fixedEmployeeNameTh ?? "(ยังไม่ได้ระบุผู้อนุมัติ)";
    case "BranchAdmin":
      return "ผู้ดูแลสาขาหลักของผู้ขอเบิก";
    default:
      return step.approverType;
  }
}

function formatDateTime(d: string) {
  return new Date(d).toLocaleString("th-TH", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

// ========================================
// Modal
// ========================================
function openCreateModal(docType?: string) {
  editingId.value = null;

  // ถ้าเพิ่มจากในกลุ่มเอกสาร ให้ใส่ประเภทและลำดับถัดไปให้อัตโนมัติ
  formModalRef.value?.open(null, docType);
}

function openEditModal(item: RouteApprove) {
  editingId.value = item.id;
  formModalRef.value?.open(item);
}

async function handleSave(payload: any, isCreate: boolean) {
  try {
    if (isCreate) await routeStore.create(payload);
    else await routeStore.update(editingId.value!, payload);

    formModalRef.value?.close();
    await routeStore.fetchAll();
    await notify.success(isCreate ? "เพิ่มขั้นตอนสำเร็จ" : "บันทึกการแก้ไขสำเร็จ");
  } catch (err) {
    // Error จาก Validation ฝั่ง Backend ให้แสดงใน Modal เลย ไม่ต้องปิด Modal
    formModalRef.value?.setError(extractErrorMessage(err));
  }
}

async function confirmDelete(item: RouteApprove) {
  const ok = await notify.confirm(
    `ยืนยันลบขั้นตอน "${item.stepName}" ใช่หรือไม่`,
    "ยืนยันการลบ"
  );
  if (!ok) return;

  try {
    await routeStore.remove(item.id);
    await notify.success("ลบขั้นตอนสำเร็จ");
    await routeStore.fetchAll();
  } catch (err) {
    await notify.error(extractErrorMessage(err), "ลบไม่สำเร็จ");
  }
}

// ========================================
// Lifecycle
// ========================================
onMounted(async () => {
  routeStore.fetchAll();

  try {
    const res = await hrApi.post('/Employees/search', { pageSize: 500, status: 'Active' })
    allowedEmployees.value = res.data.items.map((x: any) => ({
      id: x.id,
      empId: x.empId,
      fullNameTh: `${x.firstNameTh} ${x.lastNameTh}`,
      positionNameTh: x.positionNameTh,
    }))
  } catch (err) {
    console.error("Failed to load employees:", err);
  }
});
</script>
