<template>
  <dialog ref="dialogRef" class="modal">
    <div class="modal-box w-11/12 max-w-3xl max-h-[92vh] p-0 overflow-hidden flex flex-col rounded-2xl">
      <!-- Header -->
      <div class="px-6 py-5 border-b border-base-200 shrink-0 flex items-center justify-between">
        <div>
          <h3 class="font-semibold text-base-content text-base">
            รายละเอียดเอกสาร
          </h3>
          <p class="text-xs text-base-content/45 mt-0.5">
            {{ docNumber }}
          </p>
        </div>

        <button type="button"
          class="btn btn-ghost btn-sm btn-square rounded-lg"
          @click="close">
          <X class="size-4" />
        </button>
      </div>

      <div class="overflow-y-auto px-6 py-5 space-y-5 flex-1">
        <!-- Document Detail -->
        <component
          :is="documentConfig.detailComponent"
          v-if="documentConfig"
          :doc-number="docNumber"
        />
        <section
          v-else
          class="rounded-xl border border-dashed border-warning/40 bg-warning/5 p-4 text-sm"
        >
          ไม่รองรับการแสดงรายละเอียดเอกสารประเภทนี้ ({{ docType }})
        </section>

        <!-- Attachments -->
        <section
          class="overflow-hidden rounded-2xl border border-base-300 bg-base-100 shadow-sm"
        >
          <div class="border-b border-base-300 bg-base-200/50 px-5 py-4">
            <div class="flex items-center gap-3">
              <div
                class="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-content shadow-sm"
              >
                <Paperclip class="size-4" />
              </div>

              <div>
                <h3 class="text-sm font-semibold text-base-content">
                  ไฟล์แนบ
                </h3>
                <p class="mt-0.5 text-xs text-base-content/45">
                  เอกสารประกอบคำขอ
                </p>
              </div>
            </div>
          </div>

          <div class="p-4 sm:p-5">
            <AttachmentList
              :doc-type="docType"
              :doc-number="docNumber"
              readonly
              :target="dialogRef"
            />
          </div>
        </section>

        <!-- Approval Timeline -->
        <div
          v-if="isLoadingTrail"
          class="rounded-2xl border border-base-300 bg-base-100 p-5"
        >
          <div class="skeleton mb-6 h-5 w-36"></div>

          <div
            v-for="i in 3"
            :key="i"
            class="mb-6 flex gap-4"
          >
            <div class="skeleton size-10 shrink-0 rounded-full"></div>

            <div class="flex-1 space-y-2">
              <div class="skeleton h-4 w-40"></div>
              <div class="skeleton h-3 w-28"></div>
            </div>
          </div>
        </div>

        <div
          v-else
          class="overflow-hidden rounded-2xl border border-base-300 bg-base-100 shadow-sm"
        >
          <!-- Timeline Header -->
          <div class="border-b border-base-300 bg-base-200/50 px-5 py-4">
            <div class="flex items-center gap-3">
              <div
                class="flex size-9 items-center justify-center rounded-lg bg-primary text-primary-content shadow-sm"
              >
                <ClipboardCheck class="size-4" />
              </div>

              <div>
                <h3 class="text-sm font-semibold text-base-content">
                  ลำดับการอนุมัติ
                </h3>

                <p class="mt-0.5 text-xs text-base-content/45">
                  ขั้นตอนและสถานะการอนุมัติเอกสาร
                </p>
              </div>
            </div>
          </div>

          <!-- Timeline -->
          <div class="p-5 sm:p-7">
            <div
              v-if="approvalSteps.length"
              class="relative"
            >
              <div
                class="absolute bottom-5 left-5 top-5 w-0.5 bg-base-300"
              ></div>

              <div
                v-for="(step, index) in approvalSteps"
                :key="step.id"
                class="relative flex gap-4"
              >
                <!-- Status Icon -->
                <div
                  class="relative z-10 flex size-10 shrink-0 items-center justify-center rounded-full border-4 border-base-100 shadow-md"
                  :class="getTimelineIconClass(step.status)"
                >
                  <Check
                    v-if="step.status === 'approved'"
                    class="size-4"
                  />

                  <Clock3
                    v-else-if="step.status === 'waitApprove'"
                    class="size-4"
                  />

                  <X
                    v-else-if="
                      step.status === 'rejected' ||
                      step.status === 'disapproved'
                    "
                    class="size-4"
                  />

                  <RotateCcw
                    v-else-if="step.status === 'recalled'"
                    class="size-4"
                  />

                  <span
                    v-else
                    class="size-2 rounded-full bg-current"
                  ></span>
                </div>

                <!-- Timeline Card -->
                <div
                  class="mb-7 flex-1 rounded-xl border px-4 py-3.5 shadow-sm transition-all"
                  :class="[
                    getTimelineCardClass(step.status),
                    index === approvalSteps.length - 1 ? 'mb-0' : '',
                  ]"
                >
                  <div
                    class="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between"
                  >
                    <div>
                      <div class="flex items-center gap-2">
                        <p class="text-sm font-semibold">
                          {{ step.title }}
                        </p>

                        <span
                          v-if="step.status === 'waitApprove'"
                          class="size-1.5 animate-pulse rounded-full bg-warning"
                        ></span>
                      </div>

                      <p class="mt-0.5 text-xs text-base-content/50">
                        {{ step.actor }}
                      </p>
                    </div>

                    <!-- Status Badge -->
                    <span
                      class="badge badge-sm w-fit font-medium"
                      :class="getTimelineBadgeClass(step.status)"
                    >
                      {{ getTimelineStatusText(step.status) }}
                    </span>
                  </div>

                  <div
                    v-if="step.date"
                    class="mt-3 flex items-center gap-1.5 text-[11px] text-base-content/40"
                  >
                    <Clock3 class="size-3" />
                    {{ step.date }}
                  </div>

                  <div
                    v-if="step.remark"
                    class="mt-3 rounded-lg bg-base-100/70 px-3 py-2 text-xs text-base-content/60"
                  >
                    {{ step.remark }}
                  </div>
                </div>
              </div>
            </div>

            <p
              v-else
              class="text-center text-sm text-base-content/40 py-6"
            >
              ยังไม่มีข้อมูลขั้นตอนอนุมัติ
            </p>
          </div>
        </div>

        <!-- Reasons -->
        <div
          v-if="!isLoadingTrail && trailReasons.length"
          class="overflow-hidden rounded-2xl border border-base-300 bg-base-100 shadow-sm"
        >
          <div class="border-b border-base-300 bg-base-200/50 px-5 py-4">
            <div class="flex items-center gap-3">
              <div
                class="flex size-9 items-center justify-center rounded-lg bg-warning text-warning-content shadow-sm"
              >
                <MessageSquareWarning class="size-4" />
              </div>

              <div>
                <h3 class="text-sm font-semibold text-base-content">
                  เหตุผลการตีกลับ / ไม่อนุมัติ
                </h3>

                <p class="mt-0.5 text-xs text-base-content/45">
                  บันทึกจากผู้อนุมัติในรอบนี้
                </p>
              </div>
            </div>
          </div>

          <div class="divide-y divide-base-300">
            <div
              v-for="(r, idx) in trailReasons"
              :key="idx"
              class="px-5 py-4"
            >
              <div class="flex items-center justify-between gap-3 flex-wrap">
                <span
                  class="badge badge-sm font-medium"
                  :class="
                    r.status === 'Disapproved'
                      ? 'badge-error badge-outline'
                      : 'badge-error badge-outline'
                  "
                >
                  {{
                    r.status === "Disapproved"
                      ? "ไม่อนุมัติ"
                      : "ตีกลับ"
                  }}
                </span>

                <span class="text-[11px] text-base-content/40">
                  {{ formatDateTime(r.createdAt) }} · {{ r.createdBy }}
                </span>
              </div>

              <p
                class="mt-2 rounded-lg bg-base-200/40 px-3 py-2.5 text-sm text-base-content/70"
              >
                {{ r.reason }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <form method="dialog" class="modal-backdrop">
      <button>close</button>
    </form>
  </dialog>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { useApprovalStore } from "../../stores/approvalStore.ts";
import AttachmentList from "../attachment/AttachmentList.vue";
import { approvalDocumentRegistry } from "../../config/approvalDocuments";
import type { ApprovalStep, ApprovalReason } from "../../types/Approval";

import {
  X,
  ClipboardCheck,
  Check,
  Clock3,
  MessageSquareWarning,
  Paperclip,
  RotateCcw,
} from "lucide-vue-next";

const approvalStore = useApprovalStore();

const dialogRef = ref<HTMLDialogElement>();
const isLoadingTrail = ref(false);

const docType = ref("");
const docNumber = ref("");

const trailSteps = ref<ApprovalStep[]>([]);
const trailReasons = ref<ApprovalReason[]>([]);

const documentConfig = computed(
  () => approvalDocumentRegistry[docType.value]
);

type TimelineStatus =
  | "approved"
  | "waitApprove"
  | "rejected"
  | "disapproved"
  | "pending"
  | "recalled"
  | "upcoming";

interface TimelineStep {
  id: number;
  title: string;
  actor: string;
  status: TimelineStatus;
  date: string;
  remark: string;
}

function toTimelineStatus(status: string): TimelineStatus {
  switch (status) {
    case "Approved":
      return "approved";

    case "WaitApprove":
      return "waitApprove";

    case "Rejected":
      return "rejected";

    case "Disapproved":
      return "disapproved";

    case "pending":
    case "Pending":
      return "pending";

    case "Recalled":
    case "recalled":
      return "recalled";

    default:
      return "upcoming";
  }
}

const approvalSteps = computed<TimelineStep[]>(() => {
  // หา Step ที่ถูกตีกลับ / ไม่อนุมัติจริง
  const deniedStepNos = trailSteps.value
    .filter(
      (s) =>
        s.status === "Rejected" ||
        s.status === "Disapproved"
    )
    .map((s) => s.stepNo);

  const actualDeniedStepNo =
    deniedStepNos.length > 0
      ? Math.min(...deniedStepNos)
      : null;

  const latestReason = trailReasons.value[0];

  return trailSteps.value.map((step) => {
    const isActualDenialPoint =
      step.stepNo === actualDeniedStepNo;

    const remark = isActualDenialPoint
      ? latestReason?.reason ?? ""
      : "";

    return {
      id: step.stepNo,
      title: `ลำดับที่ ${step.stepNo}`,
      actor: step.approverNameTh,
      status: toTimelineStatus(step.status),
      date: step.approvedDate
        ? formatDateTime(step.approvedDate)
        : "",
      remark,
    };
  });
});

function formatDateTime(d: string) {
  return new Date(d).toLocaleString("th-TH", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function getTimelineIconClass(status: TimelineStatus) {
  switch (status) {
    case "approved":
      return "bg-success text-success-content border-success";

    case "waitApprove":
      return "bg-warning text-warning-content border-warning";

    case "rejected":
      return "bg-error text-error-content border-error";

    case "disapproved":
      return "bg-error text-error-content border-error";

    case "recalled":
      return "bg-info text-info-content border-info";

    case "pending":
      return "bg-base-200 text-base-content/40 border-base-300";

    default:
      return "bg-base-200 text-base-content/30 border-base-300";
  }
}

function getTimelineCardClass(status: TimelineStatus) {
  switch (status) {
    case "approved":
      return "border-success/25 bg-success/5";

    case "waitApprove":
      return "border-warning/30 bg-warning/5 ring-1 ring-warning/10";

    case "rejected":
      return "border-error/25 bg-error/5";

    case "disapproved":
      return "border-error/25 bg-error/5";

    case "recalled":
      return "border-info/25 bg-info/5";

    case "pending":
      return "border-base-300 bg-base-200/30";

    default:
      return "border-base-300 bg-base-100";
  }
}

function getTimelineBadgeClass(status: TimelineStatus) {
  switch (status) {
    case "approved":
      return "badge-success badge-outline";

    case "waitApprove":
      return "badge-warning badge-outline";

    case "rejected":
      return "badge-error badge-outline";

    case "disapproved":
      return "badge-error badge-outline";

    case "recalled":
      return "badge-info badge-outline";

    case "pending":
      return "badge-ghost";

    default:
      return "badge-ghost";
  }
}

function getTimelineStatusText(status: TimelineStatus) {
  switch (status) {
    case "approved":
      return "อนุมัติ";

    case "waitApprove":
      return "รออนุมัติ";

    case "rejected":
      return "ตีกลับ";

    case "disapproved":
      return "ไม่อนุมัติ";

    case "pending":
      return "รอลำดับก่อนหน้าอนุมัติ";

    case "recalled":
      return "ดึงเอกสารกลับแล้ว";

    default:
      return "รอดำเนินการ";
  }
}

async function open(type: string, docNum: string) {
  docType.value = type;
  docNumber.value = docNum;

  isLoadingTrail.value = true;

  dialogRef.value?.showModal();

  try {
    const trail = await approvalStore.getTrail(type, docNum);

    trailSteps.value = trail.steps;
    trailReasons.value = trail.reasons;
  } catch (err) {
    console.error("Failed to load approval trail:", err);
  } finally {
    isLoadingTrail.value = false;
  }
}

function close() {
  dialogRef.value?.close();
}

defineExpose({
  open,
  close,
});
</script>