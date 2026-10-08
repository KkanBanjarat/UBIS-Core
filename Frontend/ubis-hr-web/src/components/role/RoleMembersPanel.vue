<template>
  <div class="bg-base-100 rounded-2xl border border-base-200 shadow-sm overflow-hidden">
    <!-- Header -->
    <div class="flex items-center justify-between px-5 py-4 border-b border-base-200">
      <div class="flex items-center gap-2 min-w-0">
        <div class="flex size-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
          <i class="bi bi-people text-sm"></i>
        </div>
        <div class="min-w-0">
          <h3 class="text-sm font-semibold truncate">
            {{ roleName ? `สมาชิกใน "${roleName}"` : 'เลือก Role ทางซ้ายก่อน' }}
          </h3>
          <p v-if="roleId" class="text-[11px] text-base-content/45 mt-0.5">{{ members.length }} คน</p>
        </div>
      </div>
    </div>

    <div v-if="!roleId" class="p-10 text-center text-sm text-base-content/40">
      เลือก Role เพื่อดูและจัดการสมาชิก
    </div>

    <template v-else>
      <!-- เพิ่มสมาชิก -->
      <div class="px-5 py-4 border-b border-base-200 bg-base-200/20">
        <div class="flex gap-2">
          <div class="relative flex-1 min-w-0">
            <input
              v-model="query"
              type="text"
              class="input input-bordered input-sm w-full pr-8"
              placeholder="เลือกผู้ใช้ที่จะเพิ่มเข้า Role (พิมพ์เพื่อค้นหาได้)"
              @focus="openList"
              @blur="open = false"
              @input="onQueryInput"
            />
            <i
              class="bi bi-chevron-down pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-xs text-base-content/40"
            ></i>
          </div>

          <button class="btn btn-primary btn-sm gap-1.5" :disabled="!picked || isAdding" @click="addMember">
            <span v-if="isAdding" class="loading loading-spinner loading-xs"></span>
            <i v-else class="bi bi-plus-lg"></i>
            เพิ่ม
          </button>
        </div>

        <!-- รายชื่อผู้ใช้: แสดงใต้ช่อง ดันเนื้อหาลง จึงไม่ถูกกรอบตัด -->
        <ul
          v-if="open"
          class="mt-2 max-h-80 overflow-y-auto rounded-xl border border-base-200 bg-base-100 shadow-sm"
        >
          <li v-if="isSearching" class="px-3 py-3 text-xs text-base-content/40">กำลังโหลด...</li>
          <li
            v-else-if="candidates.length === 0"
            class="px-3 py-3 text-xs text-base-content/40"
          >
            ไม่พบผู้ใช้ที่เพิ่มได้ (หรืออยู่ใน Role นี้หมดแล้ว)
          </li>

          <!-- mousedown.prevent กัน input เสีย focus ก่อนคลิกเลือก -->
          <li
            v-for="u in candidates"
            :key="u.id"
            class="cursor-pointer px-3 py-2.5 hover:bg-base-200/60"
            @mousedown.prevent="pick(u)"
          >
            <div class="truncate text-sm font-medium">{{ u.displayName }}</div>
            <div class="truncate text-xs text-base-content/40">
              {{ u.email }}<span v-if="u.employeeCode"> · {{ u.employeeCode }}</span>
            </div>
          </li>

          <li
            v-if="!isSearching && candidates.length > 0"
            class="sticky bottom-0 border-t border-base-200 bg-base-100 px-3 py-2 text-[11px] text-base-content/35"
          >
            ไม่เจอคนที่ต้องการ? พิมพ์ชื่อ อีเมล หรือรหัสพนักงานเพื่อค้นหา
          </li>
        </ul>
      </div>

      <!-- Loading / Error -->
      <div v-if="isLoading" class="p-5 space-y-2">
        <div v-for="i in 3" :key="i" class="skeleton h-12 w-full rounded-lg"></div>
      </div>
      <div v-else-if="errorMessage" class="p-8 text-center text-sm text-error">
        {{ errorMessage }}
        <button class="btn btn-ghost btn-xs ml-2" @click="load">ลองใหม่</button>
      </div>

      <!-- รายการสมาชิก -->
      <div v-else-if="members.length === 0" class="p-10 text-center text-sm text-base-content/40">
        ยังไม่มีสมาชิกใน Role นี้
      </div>
      <ul v-else class="divide-y divide-base-200/60">
        <li v-for="m in members" :key="m.userRoleId" class="flex items-center gap-3 px-5 py-3 hover:bg-base-200/30">
          <div class="flex size-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
            {{ m.displayName?.charAt(0)?.toUpperCase() ?? '?' }}
          </div>

          <div class="min-w-0 flex-1">
            <div class="flex items-center gap-2">
              <span class="truncate text-sm font-medium">{{ m.displayName }}</span>
              <span v-if="!m.isActive" class="badge badge-ghost badge-sm">ปิดใช้งาน</span>
            </div>
            <div class="truncate text-xs text-base-content/40">
              {{ m.email }}<span v-if="m.employeeCode"> · {{ m.employeeCode }}</span>
            </div>
          </div>

          <button
            class="btn btn-ghost btn-xs btn-square text-error/60 hover:text-error hover:bg-error/10 shrink-0"
            title="ถอดออกจาก Role"
            @click="confirmRemove(m)"
          >
            <i class="bi bi-trash3"></i>
          </button>
        </li>
      </ul>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed, onMounted } from 'vue'
import accessApi from '../../services/accessApi'
import { useRoleMemberStore } from '../../stores/roleMemberStore'
import type { RoleMember, RoleScope } from '../../types/RoleMember'
import type { UserItem } from '../../types/User'
import { notify, extractErrorMessage } from '../../utils/notify'

const props = defineProps<{ roleId: string | null; roleName: string | null }>()

const store = useRoleMemberStore()

// หลังบ้านต้องการค่า Scope เสมอ: ใช้ค่าเดียวกับสมาชิกเดิมในระบบ (เปลี่ยนค่าเดียวตรงนี้ได้)
const DEFAULT_SCOPE: RoleScope = 'Branch'

const members = ref<RoleMember[]>([])
const isLoading = ref(false)
const errorMessage = ref('')

const query = ref('')
const open = ref(false)
const picked = ref<UserItem | null>(null)
const results = ref<UserItem[]>([])
const isSearching = ref(false)
const isAdding = ref(false)

const memberIds = computed(() => new Set(members.value.map((m) => m.userId)))
const candidates = computed(() => results.value.filter((u) => !memberIds.value.has(u.id)))

async function load() {
  if (!props.roleId) {
    members.value = []
    return
  }
  isLoading.value = true
  errorMessage.value = ''
  const id = props.roleId
  try {
    const data = await store.fetch(id)
    if (id === props.roleId) members.value = data
  } catch (err) {
    errorMessage.value = extractErrorMessage(err)
  } finally {
    isLoading.value = false
  }
}

// ---------- Dropdown ผู้ใช้ ----------
async function search(q: string) {
  isSearching.value = true
  try {
    const res = await accessApi.post('/Users/user-list', {
      search: q, status: 'active', source: null, page: 1, pageSize: 100,
    })
    results.value = res.data.items ?? []
  } catch {
    results.value = []
  } finally {
    isSearching.value = false
  }
}

// กดช่อง = เปิดรายชื่อทันที (โหลดครั้งแรกตอนเปิด)
function openList() {
  open.value = true
  if (results.value.length === 0) search('')
}

let timer: ReturnType<typeof setTimeout>
function onQueryInput() {
  picked.value = null
  open.value = true
  clearTimeout(timer)
  timer = setTimeout(() => search(query.value.trim()), 300)
}

function pick(u: UserItem) {
  picked.value = u
  query.value = u.displayName
  open.value = false
}

function clearPick() {
  picked.value = null
  query.value = ''
  results.value = []
  open.value = false
}

async function addMember() {
  if (!props.roleId || !picked.value) return
  isAdding.value = true
  try {
    await store.add(picked.value.id, props.roleId, DEFAULT_SCOPE)
    clearPick()
    await load()
    await notify.success('เพิ่มสมาชิกสำเร็จ')
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'เพิ่มสมาชิกไม่สำเร็จ')
  } finally {
    isAdding.value = false
  }
}

async function confirmRemove(m: RoleMember) {
  const ok = await notify.confirm(
    `ยืนยันถอด "${m.displayName}" ออกจาก Role "${props.roleName}" ใช่หรือไม่`,
    'ยืนยันการถอด',
  )
  if (!ok) return
  try {
    await store.remove(m.userRoleId)
    members.value = members.value.filter((x) => x.userRoleId !== m.userRoleId)
    await notify.success('ถอดสมาชิกสำเร็จ')
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ถอดไม่สำเร็จ')
  }
}

onMounted(load)

watch(() => props.roleId, () => {
  clearPick()
  load()
})

defineExpose({ reload: load })
</script>