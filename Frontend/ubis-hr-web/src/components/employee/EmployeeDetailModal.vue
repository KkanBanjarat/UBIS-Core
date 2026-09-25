<template>
  <dialog ref="dialogRef" class="modal">
    <div
      class="modal-box flex max-h-[92vh] w-11/12 max-w-4xl flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white p-0 shadow-2xl"
    >
      <!-- ===================================================== -->
      <!-- HERO -->
      <!-- ===================================================== -->
      <div class="relative shrink-0 overflow-hidden">
        <!-- Background -->
        <div
          class="absolute inset-0 bg-gradient-to-br from-emerald-50 via-white to-sky-50"
        ></div>

        <div
          class="absolute -right-24 -top-24 size-64 rounded-full bg-emerald-200/30 blur-3xl"
        ></div>

        <div
          class="absolute -bottom-28 left-1/3 size-60 rounded-full bg-sky-200/20 blur-3xl"
        ></div>

        <!-- Close -->
        <button
          type="button"
          class="btn btn-ghost btn-sm btn-square absolute right-4 top-4 z-10 rounded-xl bg-white/70 text-slate-500 shadow-sm backdrop-blur hover:bg-white hover:text-slate-800"
          @click="close"
        >
          <X class="size-4" />
        </button>

        <!-- Hero content -->
        <div class="relative px-6 pb-6 pt-7 sm:px-8">
          <div class="flex items-end gap-4">
            <!-- Avatar -->
            <div class="relative shrink-0">
              <div
                class="flex size-20 items-center justify-center rounded-2xl text-2xl font-bold shadow-lg ring-4 ring-white"
                :class="avatarColor"
              >
                {{ initials }}
              </div>

              <span
                v-if="employee?.status === 'Active'"
                class="absolute bottom-0.5 right-0.5 size-4 rounded-full border-2 border-white bg-emerald-500 shadow-sm"
              ></span>
            </div>

            <!-- Name -->
            <div class="min-w-0 flex-1 pb-0.5">
              <div class="mb-1.5 flex flex-wrap items-center gap-2">
                <span
                  class="inline-flex items-center gap-1.5 rounded-full border border-emerald-100 bg-white/80 px-2.5 py-1 text-[10px] font-semibold text-emerald-600 backdrop-blur"
                >
                  <span class="size-1.5 rounded-full bg-emerald-500"></span>
                  EMPLOYEE
                </span>

                <span
                  class="rounded-full px-2.5 py-1 text-[10px] font-semibold"
                  :class="statusMeta.class"
                >
                  {{ statusMeta.label }}
                </span>
              </div>

              <h3
                class="truncate text-lg font-bold tracking-tight text-slate-800 sm:text-xl"
              >
                {{ employee?.prefixNameTh }}
                {{ employee?.fNameTh }}
                {{ employee?.lNameTh }}
              </h3>

              <p class="mt-0.5 truncate text-xs text-slate-400 sm:text-sm">
                {{ employee?.fNameEn }}
                {{ employee?.lNameEn }}
              </p>
            </div>
          </div>

          <!-- Position -->
          <div class="mt-5 flex flex-wrap gap-2">
            <span
              v-if="employee?.positionNameTh"
              class="inline-flex items-center gap-1.5 rounded-lg border border-white/80 bg-white/70 px-2.5 py-1.5 text-xs font-medium text-slate-600 shadow-sm backdrop-blur"
            >
              <Briefcase class="size-3.5 text-emerald-500" />
              {{ employee.positionNameTh }}
            </span>

            <span
              v-if="employee?.branchNameTh"
              class="inline-flex items-center gap-1.5 rounded-lg border border-white/80 bg-white/70 px-2.5 py-1.5 text-xs font-medium text-slate-600 shadow-sm backdrop-blur"
            >
              <MapPin class="size-3.5 text-sky-500" />
              {{ employee.branchNameTh }}
            </span>
          </div>
        </div>
      </div>

      <!-- ===================================================== -->
      <!-- QUICK STATS -->
      <!-- ===================================================== -->
      <div
        class="grid shrink-0 grid-cols-3 divide-x divide-slate-200 border-y border-slate-200 bg-white"
      >
        <div class="px-3 py-3.5 text-center sm:px-5">
          <p class="text-[10px] font-medium uppercase tracking-wider text-slate-400">
            รหัสพนักงาน
          </p>

          <p
            class="mt-1 truncate text-sm font-bold text-slate-700"
          >
            {{ employee?.empId || '-' }}
          </p>
        </div>

        <div class="px-3 py-3.5 text-center sm:px-5">
          <p class="text-[10px] font-medium uppercase tracking-wider text-slate-400">
            วันเริ่มงาน
          </p>

          <p
            class="mt-1 truncate text-sm font-bold text-slate-700"
          >
            {{ formattedHireDate }}
          </p>
        </div>

        <div class="px-3 py-3.5 text-center sm:px-5">
          <p class="text-[10px] font-medium uppercase tracking-wider text-slate-400">
            อายุงาน
          </p>

          <p
            class="mt-1 truncate text-sm font-bold text-emerald-600"
          >
            {{ tenure }}
          </p>
        </div>
      </div>

      <!-- ===================================================== -->
      <!-- BODY -->
      <!-- ===================================================== -->
      <div class="flex-1 overflow-y-auto bg-slate-50/50 px-5 py-5 sm:px-7">
        <div class="space-y-5">
          <!-- ================================================= -->
          <!-- CONTACT -->
          <!-- ================================================= -->
          <section
            class="rounded-2xl border border-slate-200 bg-white shadow-sm"
          >
            <SectionHeader
              :icon="UserRound"
              title="ข้อมูลติดต่อ"
              subtitle="Contact information"
              tone="emerald"
            />

            <div class="grid grid-cols-1 gap-3 p-4 sm:grid-cols-2">
              <InfoRow
                :icon="Mail"
                label="Email"
                :value="employee?.email"
                copyable
              />

              <InfoRow
                :icon="VenetianMask"
                label="เพศ"
                :value="genderLabel"
              />
            </div>
          </section>

          <!-- ================================================= -->
          <!-- POSITION -->
          <!-- ================================================= -->
          <section
            class="rounded-2xl border border-slate-200 bg-white shadow-sm"
          >
            <SectionHeader
              :icon="Briefcase"
              title="ตำแหน่งงาน"
              subtitle="Employment information"
              tone="sky"
            />

            <div class="grid grid-cols-1 gap-3 p-4 sm:grid-cols-2">
              <InfoRow
                :icon="IdCard"
                label="ตำแหน่ง"
                :value="employee?.positionNameTh"
              />

              <InfoRow
                :icon="ChevronsUp"
                label="ระดับตำแหน่ง"
                :value="employee?.positionLevelNameTh"
              />

              <InfoRow
                :icon="Tag"
                label="ประเภทพนักงาน"
                :value="employee?.employeeTypeNameTh"
              />

              <InfoRow
                :icon="Users"
                label="หัวหน้างาน"
                :value="employee?.reportToNameTh"
              />
            </div>
          </section>

          <!-- ================================================= -->
          <!-- ORGANIZATION -->
          <!-- ================================================= -->
          <section
            class="rounded-2xl border border-slate-200 bg-white shadow-sm"
          >
            <SectionHeader
              :icon="Building2"
              title="สังกัดองค์กร"
              subtitle="Organization structure"
              tone="violet"
            />

            <div class="grid grid-cols-1 gap-3 p-4 sm:grid-cols-2">
              <InfoRow
                :icon="Landmark"
                label="บริษัท"
                :value="employee?.companyNameTh"
              />

              <InfoRow
                :icon="MapPin"
                label="สาขา"
                :value="branchDisplay"
              />

              <InfoRow
                v-if="employee?.groupNameTh"
                :icon="Network"
                label="สายงาน"
                :value="employee?.groupNameTh"
              />

              <InfoRow
                v-if="employee?.departmentNameTh"
                :icon="Network"
                label="ฝ่าย"
                :value="employee?.departmentNameTh"
              />

              <InfoRow
                v-if="employee?.divisionNameTh"
                :icon="Network"
                label="แผนก"
                :value="employee?.divisionNameTh"
              />

              <InfoRow
                v-if="employee?.sectionNameTh"
                :icon="Network"
                label="ส่วนงาน"
                :value="employee?.sectionNameTh"
              />
            </div>
          </section>

          <!-- ================================================= -->
          <!-- BENEFIT -->
          <!-- ================================================= -->
          <section class="rounded-2xl border border-slate-200 bg-white shadow-sm">
            <SectionHeader
              :icon="Gift"
              title="กลุ่มสวัสดิการ"
              subtitle="Benefit plans"
              tone="amber"
            />

            <div class="p-4">
              <div
                v-if="employee?.benefitPlans?.length"
                class="grid grid-cols-1 gap-3 md:grid-cols-2"
              >
                <div
                  v-for="p in employee.benefitPlans"
                  :key="p.id"
                  class="rounded-xl border border-slate-200 bg-slate-50/50 p-4 transition-colors hover:border-emerald-200 hover:bg-emerald-50/20"
                >
                  <div class="flex items-center justify-between gap-3">
                    <div class="flex min-w-0 items-center gap-2">
                      <span
                        class="flex size-7 shrink-0 items-center justify-center rounded-lg bg-amber-50 text-amber-600"
                      >
                        <Gift class="size-3.5" />
                      </span>

                      <p class="truncate text-sm font-semibold text-slate-700">
                        {{ p.nameTh }}
                      </p>
                    </div>

                    <span
                      class="size-2 shrink-0 rounded-full bg-emerald-400"
                    ></span>
                  </div>

                  <div
                    v-if="p.items?.length"
                    class="mt-3 divide-y divide-slate-100 rounded-lg border border-slate-100 bg-white"
                  >
                    <div
                      v-for="item in p.items"
                      :key="item.benefitId"
                      class="flex items-center justify-between gap-3 px-3 py-2.5"
                    >
                      <div class="min-w-0">
                        <p class="truncate text-xs font-medium text-slate-600">
                          {{ item.benefitNameTh }}
                        </p>

                        <p
                          v-if="item.benefitNameEn"
                          class="truncate text-[10px] text-slate-400"
                        >
                          {{ item.benefitNameEn }}
                        </p>
                      </div>

                      <span
                        class="shrink-0 rounded-md bg-emerald-50 px-2 py-1 text-[10px] font-semibold text-emerald-600"
                      >
                        {{ item.limitAmount.toLocaleString('th-TH') }}
                        บาท
                      </span>
                    </div>
                  </div>

                  <p
                    v-else
                    class="mt-3 rounded-lg bg-slate-100 px-3 py-2 text-xs text-slate-400"
                  >
                    แผนนี้ยังไม่มีสวัสดิการกำหนดไว้
                  </p>
                </div>
              </div>

              <!-- Empty -->
              <div
                v-else
                class="flex flex-col items-center justify-center rounded-xl border border-dashed border-slate-200 bg-slate-50/50 py-10 text-center"
              >
                <div
                  class="flex size-12 items-center justify-center rounded-2xl bg-amber-50 text-amber-500"
                >
                  <Gift class="size-5" />
                </div>

                <p class="mt-3 text-sm font-medium text-slate-500">
                  ยังไม่ได้กำหนดกลุ่มสวัสดิการ
                </p>

                <p class="mt-1 text-xs text-slate-400">
                  กรุณาติดต่อฝ่ายบุคคล
                </p>
              </div>
            </div>
          </section>

          <!-- ================================================= -->
          <!-- ORG CHART -->
          <!-- ================================================= -->
          <section
            class="rounded-2xl border border-slate-200 bg-white shadow-sm"
          >
            <SectionHeader
              :icon="Network"
              title="สายการบังคับบัญชา"
              subtitle="Organization chart"
              tone="violet"
            />

            <div
              v-if="employee?.orgChart"
              class="overflow-x-auto p-5"
            >
              <div class="flex min-w-fit justify-center py-2">
                <OrgChartNodeTimeline
                  :node="employee.orgChart"
                  variant="modal"
                />
              </div>
            </div>

            <div
              v-else
              class="flex flex-col items-center justify-center py-10 text-center"
            >
              <div
                class="flex size-11 items-center justify-center rounded-xl bg-slate-100 text-slate-400"
              >
                <Network class="size-5" />
              </div>

              <p class="mt-3 text-sm font-medium text-slate-500">
                ไม่มีข้อมูลสายการบังคับบัญชา
              </p>
            </div>
          </section>

          <!-- ================================================= -->
          <!-- AUDIT -->
          <!-- ================================================= -->
          <section
            v-if="employee?.createdAt || employee?.updatedAt"
            class="rounded-2xl border border-slate-200 bg-white shadow-sm"
          >
            <SectionHeader
              :icon="History"
              title="ประวัติการบันทึก"
              subtitle="Audit information"
              tone="slate"
            />

            <div class="grid grid-cols-1 gap-3 p-4 sm:grid-cols-2">
              <InfoRow
                v-if="employee?.createdAt"
                :icon="PlusCircle"
                label="สร้างเมื่อ"
                :value="formatDateTime(employee.createdAt)"
                :sub="employee?.createdBy"
              />

              <InfoRow
                v-if="employee?.updatedAt"
                :icon="RefreshCw"
                label="แก้ไขล่าสุด"
                :value="formatDateTime(employee.updatedAt)"
                :sub="employee?.updatedBy"
              />
            </div>
          </section>
        </div>
      </div>

      <!-- ===================================================== -->
      <!-- FOOTER -->
      <!-- ===================================================== -->
      <div
        class="flex shrink-0 items-center justify-between gap-3 border-t border-slate-200 bg-white px-5 py-3.5 sm:px-7"
      >
        <div class="hidden items-center gap-1.5 text-[10px] text-slate-400 sm:flex">
          <span class="size-1.5 rounded-full bg-emerald-500"></span>
          UBIS CORE
        </div>

        <div class="ml-auto flex gap-2">
          <button
            type="button"
            class="btn btn-ghost rounded-xl px-4 text-slate-500 hover:bg-slate-100"
            @click="close"
          >
            ปิด
          </button>

          <button
            type="button"
            class="btn rounded-xl border-0 bg-gradient-to-r from-emerald-500 to-green-600 px-4 text-white shadow-md shadow-emerald-500/20 hover:from-emerald-600 hover:to-green-700"
            @click="handleEdit"
          >
            <Edit class="size-4" />
            แก้ไขข้อมูล
          </button>
        </div>
      </div>
    </div>

    <form method="dialog" class="modal-backdrop">
      <button @click="close">ปิด</button>
    </form>
  </dialog>
</template>

<script setup lang="ts">
import {
  ref,
  computed,
  h,
  defineComponent,
  type PropType,
  type Component,
} from 'vue'

import type { Employee } from '../../types/Employee'

import {
  X,
  Edit,
  UserRound,
  Mail,
  VenetianMask,
  Briefcase,
  IdCard,
  ChevronsUp,
  Tag,
  Users,
  Building2,
  Landmark,
  MapPin,
  Network,
  History,
  PlusCircle,
  RefreshCw,
  Copy,
  Check,
  Gift,
} from 'lucide-vue-next'

import OrgChartNodeTimeline from './OrgChartNodeTimeline.vue'

const props = defineProps<{
  employee?: Employee | null
}>()

const emit = defineEmits<{
  edit: [employee: Employee]
}>()

const dialogRef = ref<HTMLDialogElement>()

function open() {
  dialogRef.value?.showModal()
}

function close() {
  dialogRef.value?.close()
}

function handleEdit() {
  if (props.employee) {
    emit('edit', props.employee)
    close()
  }
}

defineExpose({
  open,
  close,
})

// ============================================================
// Display helpers
// ============================================================

const avatarPalette = [
  'bg-emerald-100 text-emerald-700',
  'bg-teal-100 text-teal-700',
  'bg-green-100 text-green-700',
  'bg-lime-100 text-lime-800',
  'bg-cyan-100 text-cyan-700',
  'bg-emerald-200 text-emerald-800',
]

const avatarColor = computed(() => {
  const seed = props.employee?.empId?.length ?? 0
  return avatarPalette[seed % avatarPalette.length]
})

const initials = computed(() => {
  const en = props.employee?.fNameEn
  return en?.charAt(0)?.toUpperCase() ?? '?'
})

function statusMetaFor(rawStatus?: string) {
  if (rawStatus === 'ปกติ' || rawStatus === 'Active') {
    return {
      label: 'ทำงานอยู่',
      class: 'bg-emerald-100 text-emerald-700',
    }
  }

  if (rawStatus === 'ลาออก' || rawStatus === 'Resigned') {
    return {
      label: 'ลาออก',
      class: 'bg-red-100 text-red-600',
    }
  }

  return {
    label: rawStatus || '-',
    class: 'bg-slate-100 text-slate-500',
  }
}

const statusMeta = computed(() =>
  statusMetaFor(props.employee?.status),
)

const genderLabel = computed(() => {
  const map: Record<string, string> = {
    Male: 'ชาย',
    Female: 'หญิง',
    Other: 'อื่นๆ',
  }

  return props.employee?.gender
    ? map[props.employee.gender] ?? props.employee.gender
    : '-'
})

const branchDisplay = computed(() => {
  return props.employee?.branchNameTh || '-'
})

function formatDateOnly(value?: string | null) {
  if (!value) return '-'

  const d = new Date(value)

  if (isNaN(d.getTime())) return '-'

  return d.toLocaleDateString('th-TH', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

function formatDateTime(value?: string | null) {
  if (!value) return '-'

  const d = new Date(value)

  if (isNaN(d.getTime())) return '-'

  return d.toLocaleString('th-TH', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const formattedHireDate = computed(() =>
  formatDateOnly(props.employee?.hireDate),
)

const tenure = computed(() => {
  const value = props.employee?.hireDate

  if (!value) return '-'

  const start = new Date(value)

  if (isNaN(start.getTime())) return '-'

  const now = new Date()

  let years = now.getFullYear() - start.getFullYear()
  let months = now.getMonth() - start.getMonth()

  if (now.getDate() < start.getDate()) {
    months -= 1
  }

  if (months < 0) {
    years -= 1
    months += 12
  }

  if (years <= 0 && months <= 0) {
    return 'น้อยกว่า 1 เดือน'
  }

  const parts: string[] = []

  if (years > 0) {
    parts.push(`${years} ปี`)
  }

  if (months > 0) {
    parts.push(`${months} เดือน`)
  }

  return parts.join(' ')
})

// ============================================================
// Section Header
// ============================================================

const sectionTone = {
  emerald: 'bg-emerald-50 text-emerald-600',
  sky: 'bg-sky-50 text-sky-600',
  violet: 'bg-violet-50 text-violet-600',
  amber: 'bg-amber-50 text-amber-600',
  slate: 'bg-slate-100 text-slate-500',
}

const SectionHeader = defineComponent({
  props: {
    icon: {
      type: [Object, Function] as PropType<Component>,
      required: true,
    },

    title: {
      type: String,
      required: true,
    },

    subtitle: {
      type: String,
      required: true,
    },

    tone: {
      type: String,
      default: 'emerald',
    },
  },

  setup(p) {
    return () =>
      h(
        'div',
        {
          class:
            'flex items-center gap-3 border-b border-slate-100 px-4 py-3.5 sm:px-5',
        },
        [
          h(
            'div',
            {
              class: `flex size-9 shrink-0 items-center justify-center rounded-xl ${
                sectionTone[p.tone as keyof typeof sectionTone] ??
                sectionTone.emerald
              }`,
            },
            [h(p.icon, { class: 'size-4' })],
          ),

          h('div', { class: 'min-w-0' }, [
            h(
              'p',
              {
                class:
                  'text-sm font-semibold text-slate-800',
              },
              p.title,
            ),

            h(
              'p',
              {
                class: 'text-[10px] text-slate-400',
              },
              p.subtitle,
            ),
          ]),
        ],
      )
  },
})

// ============================================================
// Info Row
// ============================================================

const InfoRow = defineComponent({
  props: {
    icon: {
      type: [Object, Function] as PropType<Component>,
      required: true,
    },

    label: {
      type: String,
      required: true,
    },

    value: {
      type: String as PropType<string | null | undefined>,
      default: null,
    },

    sub: {
      type: String as PropType<string | null | undefined>,
      default: null,
    },

    copyable: {
      type: Boolean,
      default: false,
    },
  },

  setup(p) {
    const copied = ref(false)

    async function copy() {
      if (!p.value) return

      try {
        await navigator.clipboard.writeText(p.value)

        copied.value = true

        setTimeout(() => {
          copied.value = false
        }, 1500)
      } catch {
        // ignore clipboard failures silently
      }
    }

    return () =>
      h(
        'div',
        {
          class:
            'group flex min-w-0 items-start gap-3 rounded-xl border border-slate-100 bg-slate-50/60 px-3.5 py-3 transition-colors hover:border-emerald-100 hover:bg-emerald-50/20',
        },
        [
          h(
            'div',
            {
              class:
                'mt-0.5 flex size-8 shrink-0 items-center justify-center rounded-lg bg-white text-slate-400 shadow-sm ring-1 ring-slate-100',
            },
            [h(p.icon, { class: 'size-3.5' })],
          ),

          h(
            'div',
            {
              class: 'min-w-0 flex-1',
            },
            [
              h(
                'p',
                {
                  class:
                    'text-[10px] font-medium uppercase tracking-wide text-slate-400',
                },
                p.label,
              ),

              h(
                'p',
                {
                  class:
                    'mt-0.5 truncate text-sm font-semibold text-slate-700',
                },
                p.value || '-',
              ),

              p.sub
                ? h(
                    'p',
                    {
                      class:
                        'mt-0.5 truncate text-[10px] text-slate-400',
                    },
                    `โดย ${p.sub}`,
                  )
                : null,
            ],
          ),

          p.copyable && p.value
            ? h(
                'button',
                {
                  type: 'button',
                  class:
                    'btn btn-ghost btn-xs btn-square shrink-0 rounded-lg text-slate-400 hover:bg-emerald-50 hover:text-emerald-600',
                  onClick: copy,
                  title: copied.value ? 'คัดลอกแล้ว' : 'คัดลอก',
                },
                [
                  h(copied.value ? Check : Copy, {
                    class: 'size-3.5',
                  }),
                ],
              )
            : null,
        ],
      )
  },
})
</script>
