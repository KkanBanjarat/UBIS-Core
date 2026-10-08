<template>
  <div class="bg-base-100 rounded-2xl border border-base-200 shadow-sm overflow-hidden">
    <!-- Header -->
    <div class="flex items-center justify-between gap-4 px-5 py-4 border-b border-base-200">
      <div class="min-w-0">
        <div class="flex items-center gap-2">
          <div
            class="flex size-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary"
          >
            <i class="bi bi-shield-lock text-sm"></i>
          </div>

          <div class="min-w-0">
            <h3 class="text-sm font-semibold truncate">
              {{ roleName ? `เมนูของ "${roleName}"` : "เลือก Role ทางซ้ายก่อน" }}
            </h3>

            <p class="text-[11px] text-base-content/45 mt-0.5">
              กำหนดระดับสิทธิ์การเข้าถึงของแต่ละเมนู
            </p>
          </div>
        </div>
      </div>

      <div
        v-if="roleId && !isReadonly"
        class="flex items-center gap-2 shrink-0"
      >
        <span
          v-if="isDirty"
          class="hidden sm:inline-flex items-center gap-1.5 text-[11px] text-warning"
        >
          <span class="size-1.5 rounded-full bg-warning"></span>
          มีการเปลี่ยนแปลง
        </span>

        <button
          class="btn btn-ghost btn-sm"
          :disabled="!isDirty || isSaving"
          @click="reset"
        >
          ยกเลิก
        </button>

        <button
          class="btn btn-primary btn-sm px-4"
          :disabled="!isDirty || isSaving"
          @click="save"
        >
          <span
            v-if="isSaving"
            class="loading loading-spinner loading-xs"
          ></span>

          <i v-else class="bi bi-check2"></i>
          บันทึก
        </button>
      </div>
    </div>

    <!-- Empty -->
    <div
      v-if="!roleId"
      class="px-6 py-14 text-center"
    >
      <div
        class="mx-auto mb-3 flex size-11 items-center justify-center rounded-xl bg-base-200/70 text-base-content/35"
      >
        <i class="bi bi-shield-lock text-lg"></i>
      </div>

      <p class="text-sm font-medium text-base-content/60">
        เลือก Role เพื่อกำหนดสิทธิ์
      </p>

      <p class="mt-1 text-xs text-base-content/35">
        เมนูที่ Role สามารถเข้าถึงได้จะแสดงที่นี่
      </p>
    </div>

    <!-- Loading -->
    <div
      v-else-if="isLoading"
      class="p-5 space-y-2"
    >
      <div
        v-for="i in 6"
        :key="i"
        class="skeleton h-10 w-full rounded-xl"
      ></div>
    </div>

    <!-- Error -->
    <div
      v-else-if="errorMessage"
      class="flex flex-col items-center gap-3 py-14 text-center"
    >
      <div
        class="flex size-11 items-center justify-center rounded-xl bg-error/10 text-error"
      >
        <i class="bi bi-exclamation-triangle"></i>
      </div>

      <p class="text-sm font-medium text-error">
        {{ errorMessage }}
      </p>

      <button
        class="btn btn-sm btn-primary"
        @click="load"
      >
        ลองใหม่
      </button>
    </div>

    <template v-else>
      <!-- SuperAdmin -->
      <div
        v-if="isReadonly"
        class="flex items-center gap-2 px-5 py-3 bg-info/5 border-b border-base-200 text-xs text-info"
      >
        <i class="bi bi-info-circle"></i>
        <span>
          SuperAdmin เข้าถึงทุกเมนูอัตโนมัติ ไม่ต้องกำหนดสิทธิ์
        </span>
      </div>

      <!-- Permission hint -->
      <div
        class="hidden sm:flex items-center justify-end gap-4 px-5 py-2.5 bg-base-200/25 border-b border-base-200 text-[10px] text-base-content/45"
      >
        <span>ไม่มีสิทธิ์</span>
        <span>ดู</span>
        <span>แก้ไข</span>
        <span>ทั้งหมด</span>
      </div>

      <!-- Menu list -->
      <ul>
        <li
          v-for="row in rows"
          :key="row.item.menuId"
          class="permission-row flex items-center gap-3 px-5 py-2.5 border-b border-base-200/50 last:border-0"
          :class="{
            'permission-category': row.item.nodeType === 'Category',
            'permission-module': row.item.nodeType === 'Module',
          }"
        >
          <!-- Menu name -->
          <div
            class="min-w-0 flex-1 flex items-center gap-2"
            :style="{ paddingLeft: row.depth * 20 + 'px' }"
          >
            <i
              v-if="row.item.nodeType === 'Category'"
              class="bi bi-folder2-open text-primary/70 text-sm shrink-0"
            ></i>

            <i
              v-else-if="row.item.nodeType === 'Module'"
              class="bi bi-folder text-base-content/40 text-xs shrink-0"
            ></i>

            <span
              class="text-sm truncate"
              :class="{
                'font-semibold': row.item.nodeType === 'Category',
                'font-medium': row.item.nodeType === 'Module',
                'text-base-content/80': row.item.nodeType === 'Page',
              }"
            >
              {{ row.item.label }}
            </span>

            <span
              v-if="row.item.nodeType !== 'Page'"
              class="shrink-0 rounded-md bg-base-200/70 px-1.5 py-0.5 text-[10px] text-base-content/45"
            >
              {{ grantedCount(row) }}/{{ row.pageIds.length }}
            </span>

            <span
              v-else-if="row.item.path"
              class="hidden lg:block ml-1 text-[10px] font-mono text-base-content/30 truncate"
            >
              {{ row.item.path }}
            </span>
          </div>

          <!-- Page permission -->
          <select
            v-if="row.item.nodeType === 'Page'"
            class="select select-bordered select-sm permission-select w-36 shrink-0"
            :class="{
              'permission-none': (levels[row.item.menuId] ?? 0) === 0,
              'permission-read': (levels[row.item.menuId] ?? 0) === 1,
              'permission-write': (levels[row.item.menuId] ?? 0) === 2,
              'permission-all': (levels[row.item.menuId] ?? 0) === 3,
            }"
            :value="levels[row.item.menuId] ?? 0"
            :disabled="isReadonly"
            @change="onLevelChange(row.item.menuId, $event)"
          >
            <option
              v-for="o in levelOptions"
              :key="o.value"
              :value="o.value"
            >
              {{ o.label }}
            </option>
          </select>

          <!-- Category / Module -->
          <select
            v-else-if="!isReadonly && row.pageIds.length"
            class="select select-bordered select-sm permission-group-select w-36 shrink-0"
            @change="onGroupChange(row, $event)"
          >
            <option value="">
              ตั้งทั้งกลุ่ม…
            </option>

            <option
              v-for="o in levelOptions"
              :key="o.value"
              :value="o.value"
            >
              {{ o.label }}
            </option>
          </select>
        </li>
      </ul>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import { useRoleMenuStore } from "../../stores/roleMenuStore";
import type { RoleMenuItem } from "../../types/RoleMenu";
import { notify, extractErrorMessage } from "../../utils/notify";

const props = defineProps<{
  roleId: string | null;
  roleName?: string | null;
}>();

const store = useRoleMenuStore();

const levelOptions = [
  { value: 0, label: "ไม่มีสิทธิ์" },
  { value: 1, label: "ดู (Read)" },
  { value: 2, label: "แก้ไข (Write)" },
  { value: 3, label: "ทั้งหมด (All)" },
];

const items = ref<RoleMenuItem[]>([]);
const levels = ref<Record<string, number>>({});
const original = ref<Record<string, number>>({});
const isLoading = ref(false);
const isSaving = ref(false);
const errorMessage = ref("");
let reqId = 0;

const isReadonly = computed(() => props.roleName === "SuperAdmin");

interface Row {
  item: RoleMenuItem;
  depth: number;
  pageIds: string[];
}

const rows = computed<Row[]>(() => {
  const children = new Map<string | null, RoleMenuItem[]>();

  for (const m of items.value) {
    if (!children.has(m.parentId)) {
      children.set(m.parentId, []);
    }

    children.get(m.parentId)!.push(m);
  }

  for (const list of children.values()) {
    list.sort((a, b) => a.sortOrder - b.sortOrder);
  }

  const collectPages = (id: string): string[] =>
    (children.get(id) ?? []).flatMap((c) =>
      c.nodeType === "Page"
        ? [c.menuId]
        : collectPages(c.menuId),
    );

  const result: Row[] = [];

  const walk = (parent: string | null, depth: number) => {
    for (const m of children.get(parent) ?? []) {
      result.push({
        item: m,
        depth,
        pageIds:
          m.nodeType === "Page"
            ? [m.menuId]
            : collectPages(m.menuId),
      });

      walk(m.menuId, depth + 1);
    }
  };

  walk(null, 0);

  return result;
});

const isDirty = computed(() =>
  Object.keys(original.value).some(
    (k) => (levels.value[k] ?? 0) !== original.value[k],
  ),
);

function grantedCount(row: Row) {
  return row.pageIds.filter(
    (id) => (levels.value[id] ?? 0) > 0,
  ).length;
}

function onLevelChange(menuId: string, e: Event) {
  levels.value[menuId] = Number(
    (e.target as HTMLSelectElement).value,
  );
}

function onGroupChange(row: Row, e: Event) {
  const select = e.target as HTMLSelectElement;

  if (select.value === "") return;

  const level = Number(select.value);

  for (const id of row.pageIds) {
    levels.value[id] = level;
  }

  select.value = "";
}

async function load() {
  if (!props.roleId) {
    items.value = [];
    levels.value = {};
    original.value = {};
    return;
  }

  const id = ++reqId;

  isLoading.value = true;
  errorMessage.value = "";

  try {
    const data = await store.fetch(props.roleId);

    if (id !== reqId) return;

    items.value = data;

    const map: Record<string, number> = {};

    for (const m of data) {
      if (m.nodeType === "Page") {
        map[m.menuId] = m.accessLevel;
      }
    }

    levels.value = { ...map };
    original.value = { ...map };
  } catch (err) {
    if (id !== reqId) return;

    console.error("Failed to load role menus:", err);
    errorMessage.value = "โหลดเมนูของ Role ไม่สำเร็จ";
  } finally {
    if (id === reqId) {
      isLoading.value = false;
    }
  }
}

function reset() {
  levels.value = { ...original.value };
}

async function save() {
  if (!props.roleId || !isDirty.value) return;

  isSaving.value = true;

  try {
    const payload = Object.entries(levels.value).map(
      ([menuId, accessLevel]) => ({
        menuId,
        accessLevel,
      }),
    );

    await store.save(props.roleId, payload);

    original.value = { ...levels.value };

    await notify.success("บันทึกสิทธิ์เมนูสำเร็จ");
  } catch (err) {
    await notify.error(
      extractErrorMessage(err),
      "บันทึกไม่สำเร็จ",
    );
  } finally {
    isSaving.value = false;
  }
}
watch(() => props.roleId, load, { immediate: true });

defineExpose({ isDirty });
</script>

<style scoped>
.permission-row {
  transition:
    background-color 0.15s ease,
    box-shadow 0.15s ease;
}

.permission-row:hover {
  background-color: color-mix(
    in srgb,
    var(--color-base-200) 35%,
    transparent
  );
}

.permission-category {
  background-color: color-mix(
    in srgb,
    var(--color-base-200) 55%,
    var(--color-base-100)
  );
}

.permission-category:hover {
  background-color: var(--color-base-200);
}

.permission-module {
  background-color: color-mix(
    in srgb,
    var(--color-base-200) 25%,
    var(--color-base-100)
  );
}

/* =========================
   Select
========================= */

.permission-select,
.permission-group-select {
  min-height: 34px;
  height: 34px;
  border-radius: 10px;

  font-size: 12px;
  font-weight: 500;

  /* สำคัญ: ให้ select เป็นทึบ */
  opacity: 1;
  backdrop-filter: none;

  transition:
    border-color 0.15s ease,
    background-color 0.15s ease,
    box-shadow 0.15s ease;
}

.permission-select:focus,
.permission-group-select:focus {
  outline: none;
  box-shadow: 0 0 0 3px color-mix(
    in srgb,
    var(--color-primary) 10%,
    transparent
  );
}

/* =========================
   Permission colors
========================= */

.permission-none {
  color: color-mix(
    in srgb,
    var(--color-base-content) 50%,
    transparent
  );

  background-color: var(--color-base-100);
  border-color: color-mix(
    in srgb,
    var(--color-base-content) 12%,
    transparent
  );
}

.permission-read {
  color: var(--color-info);

  background-color: color-mix(
    in srgb,
    var(--color-info) 7%,
    var(--color-base-100)
  );

  border-color: color-mix(
    in srgb,
    var(--color-info) 20%,
    transparent
  );
}

.permission-write {
  color: var(--color-warning);

  background-color: color-mix(
    in srgb,
    var(--color-warning) 8%,
    var(--color-base-100)
  );

  border-color: color-mix(
    in srgb,
    var(--color-warning) 20%,
    transparent
  );
}

.permission-all {
  color: var(--color-success);

  background-color: color-mix(
    in srgb,
    var(--color-success) 8%,
    var(--color-base-100)
  );

  border-color: color-mix(
    in srgb,
    var(--color-success) 20%,
    transparent
  );
}

.permission-group-select {
  color: color-mix(
    in srgb,
    var(--color-base-content) 55%,
    transparent
  );

  background-color: var(--color-base-100);

  border-color: color-mix(
    in srgb,
    var(--color-base-content) 12%,
    transparent
  );
}

/* =========================
   Mobile
========================= */

@media (max-width: 640px) {
  .permission-row {
    padding-left: 14px;
    padding-right: 14px;
    gap: 8px;
  }

  .permission-row > div:first-child {
    padding-left: 0 !important;
  }

  .permission-select,
  .permission-group-select {
    width: 112px;
    min-width: 112px;
  }
}
</style>