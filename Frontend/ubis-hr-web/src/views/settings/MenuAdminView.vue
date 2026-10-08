<template>
  <div class="mx-auto max-w-[1600px] space-y-5 p-4 sm:p-6">
    <PageHeader title="จัดการเมนู" description="สร้าง แก้ไข และจัดการโครงสร้างเมนูของระบบตามสิทธิ์ Role" />
    <!-- Filter / Toolbar -->
    <div class="rounded-2xl border border-base-200 bg-base-100 p-4 shadow-sm">
      <div class="flex items-center gap-2.5">
        <!-- Search -->
        <div class="relative min-w-0 flex-1">
          <input v-model="search" type="text"
            class="input input-sm h-9 w-full rounded-xl border-base-300 bg-base-50 text-sm transition-all focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/10"
            placeholder="ค้นหา ชื่อ / Code / Path" />
        </div>

        <!-- Type -->
        <select v-model="typeFilter"
          class="select select-sm h-9 w-[130px] shrink-0 rounded-xl border-base-300 bg-base-50 text-sm">
          <option value="">ทุกชนิด</option>
          <option value="Category">Category</option>
          <option value="Module">Module</option>
          <option value="Page">Page</option>
        </select>

        <!-- Status -->
        <select v-model="activeFilter"
          class="select select-sm h-9 w-[125px] shrink-0 rounded-xl border-base-300 bg-base-50 text-sm">
          <option value="">ทุกสถานะ</option>
          <option value="active">เปิด</option>
          <option value="inactive">ปิด</option>
        </select>
        <!-- Clear filter -->
        <button v-if="hasFilter" type="button"
          class="btn btn-ghost btn-sm h-9 shrink-0 gap-1.5 rounded-xl px-3 text-sm text-base-content/60 hover:bg-base-200"
          title="ล้างตัวกรอง" @click="clearFilters">
          <FilterX class="size-4" />
          <span class="hidden sm:inline">ล้างตัวกรอง</span>
        </button>
        <!-- Add -->
        <button
          class="btn btn-primary btn-sm h-9 shrink-0 rounded-xl px-4 text-sm font-medium shadow-sm shadow-primary/20 transition-all hover:-translate-y-0.5 hover:shadow-md"
          @click="openCreate()">
          <Plus class="size-4" />
          <span class="hidden sm:inline">เพิ่มเมนู</span>
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="isLoading && rows.length === 0" class="rounded-2xl border border-base-200 bg-base-100 p-5 shadow-sm">
      <div v-for="i in 6" :key="i" class="mb-3 h-12 w-full rounded-xl skeleton"></div>
    </div>

    <!-- Error -->
    <div v-else-if="errorMessage && rows.length === 0"
      class="flex flex-col items-center justify-center gap-3 rounded-2xl border border-error/10 bg-base-100 p-14 text-center shadow-sm">
      <div class="flex size-12 items-center justify-center rounded-2xl bg-error/10">
        <CircleAlert class="size-6 text-error" />
      </div>

      <div>
        <p class="font-medium text-error">ไม่สามารถโหลดข้อมูลได้</p>
        <p class="mt-1 text-sm text-base-content/50">
          {{ errorMessage }}
        </p>
      </div>

      <button class="btn btn-ghost btn-sm rounded-xl" @click="load">
        ลองใหม่
      </button>
    </div>

    <!-- Table Card -->
    <div v-else class="overflow-hidden rounded-2xl border border-base-200 bg-base-100 shadow-sm">
      <!-- Table Header -->
      <div class="flex items-center justify-between border-b border-base-200 px-5 py-4">
        <div>
          <h3 class="text-sm font-semibold text-base-content">
            รายการเมนู
          </h3>
          <p class="mt-0.5 text-xs text-base-content/45">
            {{ rows.length }} รายการ
          </p>
        </div>

        <div class="flex items-center gap-2 text-xs text-base-content/45">
          <span class="size-2 rounded-full bg-success"></span>
          เปิดใช้งาน
          <span class="ml-2 size-2 rounded-full bg-base-content/20"></span>
          ปิดใช้งาน
        </div>
      </div>

      <div class="overflow-x-auto">
        <table class="table w-full">
          <thead>
            <tr
              class="border-b border-base-200 bg-base-200/30 text-xs font-semibold uppercase tracking-wide text-base-content/50">
              <th class="py-3.5 pl-5">เมนู</th>
              <th>ชนิด</th>
              <th>Path</th>
              <th>Permission</th>
              <th class="text-center">ลำดับ</th>
              <th class="text-center">สถานะ</th>
              <th class="w-32 pr-5"></th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="r in rows" :key="r.item.id"
              class="group border-b border-base-200/70 transition-colors last:border-0 hover:bg-base-200/25"
              :class="{ 'opacity-50': !r.item.isActive }">
              <!-- Menu -->
              <td class="py-3.5 pl-5">
                <div class="flex items-center gap-2.5" :style="{ paddingLeft: r.depth * 22 + 'px' }">
                  <!-- Tree line -->
                  <div v-if="r.depth > 0" class="flex items-center text-base-content/25">
                    <span class="text-sm">└─</span>
                  </div>

                  <!-- Menu Icon -->
                  <div class="flex size-9 shrink-0 items-center justify-center rounded-xl" :class="{
                    'bg-primary/10 text-primary':
                      r.item.nodeType === 'Category',
                    'bg-secondary/10 text-secondary':
                      r.item.nodeType === 'Module',
                    'bg-base-200 text-base-content/50':
                      r.item.nodeType === 'Page'
                  }">
                    <span class="text-xs font-bold" :class="{
                      'text-primary': r.item.nodeType === 'Category',
                      'text-secondary': r.item.nodeType === 'Module',
                      'text-base-content/50': r.item.nodeType === 'Page'
                    }">
                      {{
                        r.item.nodeType === 'Category'
                          ? 'C'
                          : r.item.nodeType === 'Module'
                            ? 'M'
                            : 'P'
                      }}
                    </span>
                  </div>

                  <div class="min-w-0">
                    <p class="truncate text-sm font-semibold text-base-content">
                      {{ r.item.label }}
                    </p>

                    <p class="mt-0.5 truncate text-xs text-base-content/40">
                      {{ r.item.code }}
                    </p>
                  </div>
                </div>
              </td>

              <!-- Type -->
              <td>
                <span class="inline-flex items-center rounded-lg px-2.5 py-1 text-xs font-medium" :class="{
                  'bg-primary/10 text-primary':
                    r.item.nodeType === 'Category',
                  'bg-secondary/10 text-secondary':
                    r.item.nodeType === 'Module',
                  'bg-base-200 text-base-content/55':
                    r.item.nodeType === 'Page'
                }">
                  {{ r.item.nodeType }}
                </span>
              </td>

              <!-- Path -->
              <td class="max-w-[240px]">
                <span class="block truncate font-mono text-xs text-base-content/55" :title="r.item.path || '-'">
                  {{ r.item.path || '-' }}
                </span>
              </td>

              <!-- Permission -->
              <td class="max-w-[220px]">
                <span class="block truncate font-mono text-xs text-base-content/55"
                  :title="r.item.permissionCode || '-'">
                  {{ r.item.permissionCode || '-' }}
                </span>
              </td>

              <!-- Sort -->
              <td class="text-center">
                <span
                  class="inline-flex size-7 items-center justify-center rounded-lg bg-base-200/70 text-xs font-medium text-base-content/60">
                  {{ r.item.sortOrder }}
                </span>
              </td>

              <!-- Status -->
              <td class="text-center">
                <span class="inline-flex items-center gap-1.5 rounded-lg px-2.5 py-1 text-xs font-medium" :class="r.item.isActive
                    ? 'bg-success/10 text-success'
                    : 'bg-base-200 text-base-content/40'
                  ">
                  <span class="size-1.5 rounded-full" :class="r.item.isActive
                      ? 'bg-success'
                      : 'bg-base-content/25'
                    "></span>

                  {{ r.item.isActive ? 'เปิด' : 'ปิด' }}
                </span>
              </td>

              <!-- Actions -->
              <td class="pr-5">
                <div class="flex justify-end gap-1 opacity-70 transition-opacity group-hover:opacity-100">
                  <button v-if="r.item.nodeType !== 'Page'"
                    class="btn btn-ghost btn-sm btn-square rounded-lg text-base-content/50 hover:bg-primary/10 hover:text-primary"
                    title="เพิ่มเมนูย่อย" @click="openCreate(r.item.id)">
                    <Plus class="size-4" />
                  </button>

                  <button
                    class="btn btn-ghost btn-sm btn-square rounded-lg text-base-content/50 hover:bg-base-200 hover:text-base-content"
                    title="แก้ไข" @click="openEdit(r.item)">
                    <Pencil class="size-4" />
                  </button>

                  <button
                    class="btn btn-ghost btn-sm btn-square rounded-lg text-base-content/40 hover:bg-error/10 hover:text-error"
                    title="ลบ" @click="confirmDelete(r.item)">
                    <Trash2 class="size-4" />
                  </button>
                </div>
              </td>
            </tr>

            <!-- Empty -->
            <tr v-if="rows.length === 0">
              <td colspan="7" class="py-16 text-center">
                <div class="flex flex-col items-center gap-3">
                  <div class="flex size-12 items-center justify-center rounded-2xl bg-base-200 text-base-content/30">
                    <span class="text-lg">☰</span>
                  </div>

                  <div>
                    <p class="text-sm font-medium text-base-content/60">
                      {{ hasFilter ? 'ไม่พบเมนูที่ตรงเงื่อนไข' : 'ยังไม่มีเมนู' }}
                    </p>
                    <p class="mt-1 text-xs text-base-content/40">
                      {{ hasFilter ? 'ลองเปลี่ยนคำค้นหาหรือล้างตัวกรอง' : 'ลองเพิ่มเมนูแรกของระบบ' }}
                    </p>
                  </div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <MenuFormModal ref="formModalRef" :menus="items" @save="handleSave" />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { Plus, Pencil, Trash2, CircleAlert, FilterX } from 'lucide-vue-next'
import PageHeader from '../../components/ui/PageHeader.vue'
import MenuFormModal from '../../components/menu/MenuFormModal.vue'
import { useMenuAdminStore } from '../../stores/menuAdminStore'
import type { MenuAdmin, SaveMenuPayload } from '../../types/MenuAdmin'
import { notify, extractErrorMessage } from '../../utils/notify'

const store = useMenuAdminStore()
const { items, isLoading, error: errorMessage } = storeToRefs(store)

const formModalRef = ref<InstanceType<typeof MenuFormModal>>()

const search = ref('')
const typeFilter = ref<'' | 'Category' | 'Module' | 'Page'>('')
const activeFilter = ref<'' | 'active' | 'inactive'>('')
const hasFilter = computed(
  () => !!(search.value.trim() || typeFilter.value || activeFilter.value)
)
// id ของเมนูที่ตรงเงื่อนไข + บรรพบุรุษทั้งหมด
const visibleIds = computed(() => {
  const q = search.value.trim().toLowerCase()
  const filtering = q || typeFilter.value || activeFilter.value

  if (!filtering) return null

  const byId = new Map(items.value.map((m) => [m.id, m]))
  const ids = new Set<string>()

  for (const m of items.value) {
    const okText =
      !q ||
      m.label.toLowerCase().includes(q) ||
      m.code.toLowerCase().includes(q) ||
      (m.path ?? '').toLowerCase().includes(q)

    const okType =
      !typeFilter.value || m.nodeType === typeFilter.value

    const okActive =
      !activeFilter.value ||
      (activeFilter.value === 'active'
        ? m.isActive
        : !m.isActive)

    if (!(okText && okType && okActive)) continue

    ids.add(m.id)

    let p = m.parentId
      ? byId.get(m.parentId)
      : undefined

    while (p && !ids.has(p.id)) {
      ids.add(p.id)
      p = p.parentId
        ? byId.get(p.parentId)
        : undefined
    }
  }

  return ids
})

// แปลงรายการเป็น Tree Row
const rows = computed(() => {
  const ids = new Set(
    items.value
      .filter(
        (x) =>
          !visibleIds.value ||
          visibleIds.value.has(x.id)
      )
      .map((x) => x.id)
  )

  const byParent = new Map<string | null, MenuAdmin[]>()

  for (const m of items.value) {
    if (
      visibleIds.value &&
      !visibleIds.value.has(m.id)
    ) {
      continue
    }

    const key =
      m.parentId && ids.has(m.parentId)
        ? m.parentId
        : null

    if (!byParent.has(key)) {
      byParent.set(key, [])
    }

    byParent.get(key)!.push(m)
  }

  const out: {
    item: MenuAdmin
    depth: number
  }[] = []

  const walk = (
    parentId: string | null,
    depth: number
  ) => {
    const list = (
      byParent.get(parentId) ?? []
    ).sort(
      (a, b) => a.sortOrder - b.sortOrder
    )

    for (const m of list) {
      out.push({
        item: m,
        depth
      })

      walk(m.id, depth + 1)
    }
  }

  walk(null, 0)

  return out
})
function clearFilters() {
  search.value = ''
  typeFilter.value = ''
  activeFilter.value = ''
}
async function load() {
  try {
    await store.fetchAll()
  } catch {
    // error แสดงจาก store.error
  }
}

function openCreate(parentId: string | null = null) {
  formModalRef.value?.open(
    undefined,
    parentId
  )
}

function openEdit(item: MenuAdmin) {
  formModalRef.value?.open(item)
}

async function handleSave(data: {
  id: string | null
  payload: SaveMenuPayload
}) {
  try {
    if (data.id) {
      await store.update(
        data.id,
        data.payload
      )
    } else {
      await store.create(
        data.payload
      )
    }

    formModalRef.value?.close()

    await notify.success(
      'บันทึกเมนูสำเร็จ'
    )
  } catch (err) {
    formModalRef.value?.setError(
      extractErrorMessage(err)
    )
  }
}

async function confirmDelete(
  item: MenuAdmin
) {
  const ok = await notify.confirm(
    `ยืนยันลบเมนู "${item.label}" ใช่หรือไม่\nสิทธิ์ของ Role ที่ผูกกับเมนูนี้จะถูกถอนด้วย`,
    'ยืนยันการลบ'
  )

  if (!ok) return

  try {
    await store.remove(item.id)

    await notify.success(
      'ลบเมนูสำเร็จ'
    )
  } catch (err) {
    await notify.error(
      extractErrorMessage(err),
      'ลบไม่สำเร็จ'
    )
  }
}

onMounted(load)
</script>