<template>
  <div class="w-full p-4 sm:p-6">

    <!-- Header -->
    <div class="mb-5 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div class="flex items-center gap-3">
        <div
          class="flex size-11 shrink-0 items-center justify-center rounded-xl
                 bg-primary/10 text-primary"
        >
          <ClipboardCheck class="size-5" />
        </div>

        <div>
          <h1 class="text-xl font-semibold tracking-tight">
            รายการรออนุมัติ
          </h1>

          <p class="mt-0.5 text-xs text-base-content/45">
            เอกสารที่ถึงคิวให้คุณอนุมัติ
          </p>
        </div>
      </div>

      <!-- Summary -->
      <div
        v-if="!isLoading"
        class="flex items-center gap-2 self-start rounded-xl
               border border-base-200 bg-base-100 px-3 py-2 sm:self-auto"
      >
        <span class="size-2 rounded-full bg-warning"></span>

        <span class="text-xs text-base-content/50">
          รออนุมัติ
        </span>

        <span class="text-sm font-semibold">
          {{ items.length }}
        </span>

        <span class="text-xs text-base-content/40">
          รายการ
        </span>
      </div>
    </div>


    <!-- Loading -->
    <div v-if="isLoading" class="space-y-3">
      <div
        v-for="i in 3"
        :key="i"
        class="rounded-2xl border border-base-200 bg-base-100 px-5 py-4"
      >
        <div class="flex items-center gap-4">
          <div class="skeleton size-9 rounded-lg"></div>

          <div class="min-w-0 flex-1">
            <div class="skeleton h-3.5 w-40"></div>
            <div class="skeleton mt-2 h-3 w-56"></div>
          </div>

          <div class="hidden gap-2 sm:flex">
            <div class="skeleton h-8 w-20 rounded-lg"></div>
            <div class="skeleton h-8 w-20 rounded-lg"></div>
          </div>
        </div>
      </div>
    </div>


    <!-- Empty -->
    <div
      v-else-if="items.length === 0"
      class="rounded-2xl border border-base-200 bg-base-100"
    >
      <div class="flex min-h-[320px] flex-col items-center justify-center text-center">
        <div
          class="flex size-14 items-center justify-center rounded-2xl
                 bg-base-200/60"
        >
          <ClipboardCheck class="size-7 text-base-content/25" />
        </div>

        <h2 class="mt-4 text-sm font-semibold text-base-content/65">
          ไม่มีรายการรออนุมัติ
        </h2>

        <p class="mt-1 text-xs text-base-content/40">
          เอกสารที่รอการอนุมัติจะแสดงที่หน้านี้
        </p>
      </div>
    </div>


    <!-- Approval List -->
    <div v-else class="space-y-3">

      <div
        v-for="item in items"
        :key="item.transApproveId"
        class="group relative overflow-hidden rounded-2xl
               border border-base-200 bg-base-100
               transition-all duration-200
               hover:border-primary/25
               hover:shadow-sm"
      >

        <!-- Accent -->
        <div
          class="absolute inset-y-0 left-0 w-0.5 bg-warning/70
                 transition-colors duration-200
                 group-hover:bg-primary"
        ></div>

        <div class="p-4 pl-5 sm:p-5 sm:pl-6">

          <!-- Header -->
          <div
            class="flex flex-col gap-3
                   sm:flex-row sm:items-start sm:justify-between"
          >

            <!-- Document -->
            <div class="min-w-0">

              <div class="flex flex-wrap items-center gap-2">

                <h4 class="text-sm font-semibold text-primary">
                  เอกสาร{{ docTypeLabel(item.docType) }}
                </h4>

                <span
                  class="inline-flex items-center gap-1 rounded-full
                         bg-warning/10 px-2 py-0.5
                         text-[10px] font-medium text-warning"
                >
                  <span class="size-1.5 rounded-full bg-warning"></span>
                  รออนุมัติ
                </span>

              </div>

              <button
                type="button"
                class="mt-1.5 flex max-w-full items-center gap-1
                       text-left text-sm font-medium
                       text-base-content/80
                       transition-colors hover:text-primary"
                @click="openDetail(item)"
              >
                <span class="truncate">
                  เลขที่เอกสาร : {{ item.docNumber }}
                </span>

                <ExternalLink
                  class="size-3.5 shrink-0 text-base-content/25
                         transition-colors
                         group-hover:text-primary"
                />
              </button>

            </div>


            <!-- Actions -->
            <div
              class="flex shrink-0 items-center gap-1.5
                     sm:pt-0.5"
            >

              <button
                type="button"
                class="btn btn-sm btn-ghost
                       h-8 min-h-8 px-2.5
                       text-error hover:bg-error/10"
                title="ไม่อนุมัติ"
                @click="openDenial(item, 'disapprove')"
              >
                <X class="size-3.5" />

                <span class="hidden md:inline">
                  ไม่อนุมัติ
                </span>
              </button>

              <button
                type="button"
                class="btn btn-sm btn-ghost
                       h-8 min-h-8 px-2.5
                       text-warning hover:bg-warning/10"
                title="ตีกลับ"
                @click="openDenial(item, 'reject')"
              >
                <Undo2 class="size-3.5" />

                <span class="hidden md:inline">
                  ตีกลับ
                </span>
              </button>

              <button
                type="button"
                class="btn btn-sm btn-success
                       h-8 min-h-8 gap-1.5 px-4
                       shadow-sm"
                title="อนุมัติ"
                @click="confirmApprove(item)"
              >
                <Check class="size-3.5" />
                <span>อนุมัติ</span>
              </button>

            </div>

          </div>


          <!-- Information -->
          <div
            class="mt-4 flex flex-col gap-3
                   border-t border-base-200 pt-3
                   md:flex-row md:items-center md:gap-8"
          >

            <!-- Employee -->
            <div class="flex min-w-0 items-center gap-2.5">

              <div
                class="flex size-8 shrink-0 items-center justify-center
                       rounded-lg bg-base-200/60"
              >
                <User class="size-4 text-base-content/50" />
              </div>

              <div class="min-w-0">
                <p class="text-[10px] uppercase tracking-wide text-base-content/35">
                  ผู้ขอ
                </p>

                <p class="truncate text-sm font-medium">
                  {{ item.employeeNameTh }}
                </p>
              </div>

            </div>


            <!-- Date -->
            <div class="flex items-center gap-2.5">

              <div
                class="flex size-8 shrink-0 items-center justify-center
                       rounded-lg bg-base-200/60"
              >
                <CalendarDays class="size-4 text-base-content/50" />
              </div>

              <div>
                <p class="text-[10px] uppercase tracking-wide text-base-content/35">
                  วันที่เอกสาร
                </p>

                <p class="text-sm font-medium">
                  {{ formatDate(item.docDate) }}
                </p>
              </div>

            </div>


            <!-- Remark -->
            <div
              v-if="item.documentDetail"
              class="flex min-w-0 items-center gap-2.5 md:flex-1"
            >

              <div
                class="flex size-8 shrink-0 items-center justify-center
                       rounded-lg bg-base-200/60"
              >
                <MessageSquare class="size-4 text-base-content/50" />
              </div>

              <div class="min-w-0">
                <p class="text-[10px] uppercase tracking-wide text-base-content/35">
                  Notes
                </p>

                <p
                  class="truncate text-sm text-base-content/60"
                  :title="item.documentDetail"
                >
                  {{ item.documentDetail }}
                </p>
              </div>

            </div>

          </div>

        </div>
      </div>

    </div>


    <!-- Denial Modal -->
    <dialog ref="rejectDialogRef" class="modal">

      <div class="modal-box max-w-md rounded-2xl">

        <!-- Modal Header -->
        <div class="flex items-start gap-3">

          <div
            class="flex size-10 shrink-0 items-center justify-center rounded-xl"
            :class="
              denialMode === 'reject'
                ? 'bg-warning/10 text-warning'
                : 'bg-error/10 text-error'
            "
          >
            <component
              :is="denialMode === 'reject' ? Undo2 : X"
              class="size-5"
            />
          </div>

          <div>
            <h3 class="text-base font-semibold">
              {{
                denialMode === 'reject'
                  ? 'ตีกลับเอกสาร'
                  : 'ไม่อนุมัติเอกสาร'
              }}
            </h3>

            <p class="mt-1 text-xs leading-relaxed text-base-content/45">
              {{
                denialMode === 'reject'
                  ? 'ผู้ขอเบิกจะสามารถแก้ไขและส่งเอกสารกลับมาใหม่ได้'
                  : 'เอกสารจะสิ้นสุดและไม่สามารถแก้ไขได้อีก'
              }}
            </p>
          </div>

        </div>


        <!-- Reason -->
        <div class="mt-5">

          <label class="text-xs font-medium text-base-content/60">
            เหตุผล
          </label>

          <textarea
            v-model="rejectReason"
            rows="4"
            placeholder="กรุณาระบุเหตุผล..."
            class="textarea textarea-bordered mt-2 w-full
                   resize-none text-sm
                   focus:border-primary focus:outline-none"
          ></textarea>

          <p class="mt-1.5 text-[11px] text-base-content/35">
            กรุณาระบุเหตุผลเพื่อให้ผู้ขอทราบสาเหตุ
          </p>

        </div>


        <!-- Modal Actions -->
        <div class="mt-5 flex justify-end gap-2">

          <button
            type="button"
            class="btn btn-ghost btn-sm"
            @click="rejectDialogRef?.close()"
          >
            ยกเลิก
          </button>

          <button
            type="button"
            class="btn btn-sm px-5"
            :class="
              denialMode === 'reject'
                ? 'btn-warning'
                : 'btn-error'
            "
            :disabled="!rejectReason.trim()"
            @click="submitDenial"
          >
            {{
              denialMode === 'reject'
                ? 'ยืนยันตีกลับ'
                : 'ยืนยันไม่อนุมัติ'
            }}
          </button>

        </div>

      </div>

      <form method="dialog" class="modal-backdrop">
        <button>close</button>
      </form>

    </dialog>


    <ApprovalDetailModal ref="detailModalRef" />

  </div>
</template>


<script setup lang="ts">
import ApprovalDetailModal from '../../components/approval/ApprovalDetailModal.vue'
import { approvalDocumentRegistry } from '../../config/approvalDocuments'
import { ref, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useApprovalStore } from '../../stores/approvalStore.ts'
import { notify, extractErrorMessage } from '../../utils/notify'
import type { Approval } from '../../types/Approval'
import {
  ClipboardCheck,
  Check,
  X,
  Undo2,
  User,
  CalendarDays,
  ExternalLink,
  MessageSquare,
} from 'lucide-vue-next'

const approvalStore = useApprovalStore()

const { items, isLoading } = storeToRefs(approvalStore)

const rejectDialogRef = ref<HTMLDialogElement>()
const rejectReason = ref('')
const rejectTarget = ref<Approval | null>(null)
const denialMode = ref<'reject' | 'disapprove'>('reject')

const detailModalRef =
  ref<InstanceType<typeof ApprovalDetailModal>>()


function openDetail(item: Approval) {
  detailModalRef.value?.open(
    item.docType,
    item.docNumber
  )
}


function docTypeLabel(docType: string) {
  return approvalDocumentRegistry[docType]?.label ?? docType
}


function formatDate(d: string) {
  return new Date(d).toLocaleDateString('th-TH', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}


async function confirmApprove(item: Approval) {
  const ok = await notify.confirm(
    `ยืนยันอนุมัติเอกสาร "${item.docNumber}" ใช่หรือไม่`,
    'ยืนยันการอนุมัติ'
  )

  if (!ok) return

  try {
    await approvalStore.approve(
      item.transApproveId
    )

    await notify.success(
      'อนุมัติสำเร็จ'
    )
  } catch (err) {
    await notify.error(
      extractErrorMessage(err),
      'อนุมัติไม่สำเร็จ'
    )
  }
}


function openDenial(
  item: Approval,
  mode: 'reject' | 'disapprove'
) {
  rejectTarget.value = item
  denialMode.value = mode
  rejectReason.value = ''
  rejectDialogRef.value?.showModal()
}


async function submitDenial() {
  if (
    !rejectTarget.value ||
    !rejectReason.value.trim()
  ) {
    return
  }

  try {
    await approvalStore.deny(
      rejectTarget.value.transApproveId,
      rejectReason.value.trim(),
      denialMode.value === 'disapprove'
    )

    rejectDialogRef.value?.close()

    await notify.success(
      denialMode.value === 'reject'
        ? 'ตีกลับเอกสารสำเร็จ'
        : 'ไม่อนุมัติเอกสารสำเร็จ'
    )
  } catch (err) {
    await notify.error(
      extractErrorMessage(err),
      denialMode.value === 'reject'
        ? 'ตีกลับไม่สำเร็จ'
        : 'ไม่อนุมัติไม่สำเร็จ',
      rejectDialogRef.value
    )
  }
}


onMounted(() => {
  approvalStore.fetchMyPending()
})
</script>

